using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using FRPCClient;
using FRPCClient.Entity;

namespace FRPCClientAOPContainer
{

    /// <summary>
    /// AOP 容器对象
    /// </summary>
    public class AOPContainer
    {
         
      
        
        ProxyGenerator generator { get; set; }

        /// <summary>
        /// 转换跳过的类型
        /// </summary>
        public Dictionary<string, Func<AOPFilterEntity>> TransformationSkipType { get; set; }
        /// <summary>
        /// 容器对象
        /// </summary>
        public AOPContainer()
        {
            generator = new ProxyGenerator();
            TransformationSkipType = new Dictionary<string, Func<AOPFilterEntity>>();
            TransformationSkipType.Add(typeof(void).FullName,()=> { return new AOPFilterEntity() { FullName= typeof(void).FullName,IsReturn=false, IsReplaceResult=true, Result=null}; });
            TransformationSkipType.Add(typeof(Task).FullName, () => { return new AOPFilterEntity { FullName = typeof(Task).FullName, IsReturn = true, IsReplaceResult=true, Result = Task.CompletedTask }; });

        }

        /// <summary>
        /// 获取远程类
        /// </summary>
        /// <param name="session">需要通讯的对象</param>
        /// <param name="serverType">需要实例化的类型</param>
        ///  <param name="RRPCSessionID">指定的Session完成任务而不是随机，若服务器有指定则会有限服务器指定的对象</param>
        /// <returns></returns>
        public object GetServices(FRPCEasyClient session, Type serverType, Guid? RRPCSessionID = null)
        {
            return generator.CreateInterfaceProxyWithoutTarget(serverType, new AOPRPCInterceptor((invocation) => {
                return ImplementFunc(invocation, session, RRPCSessionID);
            }));
        }

        /// <summary>
        /// 获取远程类
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="session">需要通讯的对象</param>
        /// <param name="RRPCSessionID">指定由谁来进行调用而不是随机</param>
        /// <returns></returns>
        public T GetServices<T>(FRPCEasyClient session,Guid? RRPCSessionID = null) where T:class
        {
            return generator.CreateInterfaceProxyWithoutTarget<T>(new AOPRPCInterceptor((invcation)=> {
               return ImplementFunc(invcation, session, RRPCSessionID);
            }));
        }

        /// <summary>
        /// 调用实现函数
        /// </summary>
        /// <param name="invocation">信息</param>
        /// <param name="session">连接对象</param>
        /// <param name="RRPCSessionID">指定由谁来进行调用而不是随机</param>
        /// <returns></returns>
        public object ImplementFunc(IInvocation invocation, FRPCEasyClient session,Guid? RRPCSessionID=null)
        {
            RequestExecutiveInformation information = new RequestExecutiveInformation()
            {
                RRPCSessionID=RRPCSessionID,
                AssemblyFullName = invocation.Method.DeclaringType.Assembly.FullName,
                FullName = invocation.Method.DeclaringType.FullName,
                ID = Guid.NewGuid(),
                MethodName = invocation.Method.Name,
                Arguments = invocation.Arguments.Select(d => JsonConvert.SerializeObject(d)).ToList()
            };
            //在这里不能使用task  会极大的阻塞线程的执行 原因未知
            var result = session.RemoteCallQueue.AddTaskQueue(information, session);
            session.RemoteCallQueue.RemoteExecutionFunc(result);

            AOPFilterEntity filterType=null;
            if (TransformationSkipType.TryGetValue(invocation.Method.ReturnType.FullName, out var value))
            {
                filterType = value.Invoke();
                if (filterType.IsReturn)
                {
                    return filterType.Result;
                }
            }
            result.WaitHandle.WaitOne();
            switch (result.State)
            {
                case ReceiveMessageState.Wait:
                    throw new Exception("任务出现错误，目前正在等待状态，却通过了健康检查");
                case ReceiveMessageState.Success:
                    if (filterType != null && filterType.IsReplaceResult)
                    {
                        return filterType.Result;
                    }
                    else
                    {
                        var obj = JsonConvert.DeserializeObject(result.ReturnValue, invocation.Method.ReturnType);
                        return obj;
                    }
                case ReceiveMessageState.Overtime:
                    throw new Exception("任务超时：" + result.ReturnValue);
                case ReceiveMessageState.Error:
                    throw new Exception("任务出现了异常：" + result.ReturnValue);
                case ReceiveMessageState.Other:
                    throw new Exception("不存在的任务");
                default:
                    break;
            }
            throw new Exception("任务出现了异常：它没有按规矩走");
        }
    }


}
