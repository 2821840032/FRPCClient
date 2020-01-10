using Newtonsoft.Json;
using FRPCClient.AttributeEntity;
using FRPCClient.Entity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;
using System.Linq;
namespace FRPCClient
{
    /// <summary>
    /// 请求监听类
    /// </summary>
   public class MonitorReceived
    {

        /// <summary>
        /// 容器对象
        /// </summary>
        public UnityInIt<FRPCEasyClient, RequestExecutiveInformation, RequestBaseInfo,BaseProvideServices> ServiceProvider { get; private set; }

        FRPCEasyClient EasySocket { get; set; }

        /// <summary>
        /// 请求监听类
        /// </summary>
        /// <param name="socket">连接对象</param>
        public MonitorReceived(FRPCEasyClient socket) {
            this.EasySocket = socket;
            Type baseProvideServicesType = typeof(BaseProvideServices);
            ServiceProvider = new UnityInIt<FRPCEasyClient, RequestExecutiveInformation, RequestBaseInfo, BaseProvideServices>(
                baseProvideServicesType.GetProperty("Socket"), 
                baseProvideServicesType.GetProperty("Info"),
                baseProvideServicesType.GetProperty("RequestInfo"),
                baseProvideServicesType.GetProperty("Container"),
                baseProvideServicesType.GetProperty("RequestClientSession"));
        }

        /// <summary>
        /// 收到请求事件
        /// </summary>
        /// <param name="stringPackageInfo"></param>
        public void Handle(RequestBaseInfo stringPackageInfo)
        {
            RequestExecutiveInformation info;
            try
            {
                info = JsonConvert.DeserializeObject<RequestExecutiveInformation>(stringPackageInfo.bodyMeg);
            }
            catch (Exception e)
            {
                EasySocket.Log("解析失败" + stringPackageInfo.bodyMeg+"。原因："+e.Message, LoggerType.Error);
                return;
            }

            if (info.ReturnValue != null && EasySocket.RemoteCallQueue.GetTaskIDAndSuccess(info.ID, info.ReturnValue))
            {
                //处理完成
            }
            else if (info.ReturnValue != null)
            {
                EasySocket.Log($"收到一个意外的请求 它有结果但是没有找到该任务的信息 ID:{info.ID} FullName:{info.FullName} Return:{info.ReturnValue} 来自于:{EasySocket.RemoteEndpoint.ToString()}", LoggerType.Error);
            }
            else
            {
                ImplementFunc(info, stringPackageInfo);
            }
        }

        /// <summary>
        /// 执行RPC的调用
        /// </summary>
        /// <param name="info">信息</param>
        /// <param name="requestInfo">基础信息</param>
        async void ImplementFunc(RequestExecutiveInformation info, RequestBaseInfo requestInfo)
        {
            await Task.Yield();
            //接收RPC的请求
            if (ServiceProvider.GetService(info.FullName, EasySocket, info,requestInfo, EasySocket.UnityContainer, info.RequestClientSession, out object executionObj, out var iServerType))
            {

                var methodType = iServerType.GetMethod(info.MethodName);

                List<object> attribtes = new List<object>();
                attribtes.AddRange(iServerType.CustomAttributes.Select(d => d.Constructor.Invoke(null)).Where(d=>d is RequestFilterAttribte).ToArray());
                attribtes.AddRange(methodType.GetCustomAttributes(true).Where(d => d is RequestFilterAttribte).ToArray());
                //Filter前

                object result = null;

                if (executionObj is BaseProvideServices)
                {
                    if (!BeforeExecutionAttribte(attribtes, (BaseProvideServices)executionObj,ref result, methodType.ReturnType))
                    {
                        return;
                    }
                }
              

                List<object> paraList = new List<object>();
                var paras = methodType.GetParameters();
                for (int i = 0; i < info.Arguments.Count; i++)
                {
                    paraList.Add(JsonConvert.DeserializeObject(info.Arguments[i], paras[i].ParameterType));
                }
               

                try
                {
                    result= result?? methodType.Invoke(executionObj, paraList.ToArray());
                }
                catch (Exception e)
                {
                    info.ReturnValue = null;
                    Console.WriteLine("处理请求时候出现异常:"+e);
                    EasySocket.Requestexception?.HandleException(e);
                }
                if (executionObj is BaseProvideServices)
                {
                    if (AfterxecutionExecutionAttribte(attribtes, (BaseProvideServices)executionObj,ref result, methodType.ReturnType))
                    {
                        info.ReturnValue = JsonConvert.SerializeObject(result);
                        var msg = JsonConvert.SerializeObject(info);
                        EasySocket.SendMessage(msg);
                    }
                }
                else {

                    info.ReturnValue = JsonConvert.SerializeObject(result);
                    var msg = JsonConvert.SerializeObject(info);
                    EasySocket.SendMessage(msg);
                }
             
            }
            else {
                EasySocket.Log("收到一个未知的请求" +info.FullName, LoggerType.Error);
            }
        }

   /// <summary>
   /// 执行方法前
   /// </summary>
   /// <param name="attributes">特性头</param>
   /// <param name="executionObj">执行对象</param>
   /// <param name="result">结果</param>
   /// <param name="returnType">结果类型</param>
   /// <returns></returns>
        private bool BeforeExecutionAttribte(List<object> attributes, BaseProvideServices executionObj,ref object result, Type returnType)
        {
            var isImplement = true;
            foreach (RequestFilterAttribte item in attributes)
            {
                if (!item.BeforeExecution(executionObj, ref result, returnType))
                {
                    isImplement = false;
                }
            }
            return isImplement;
        }

        /// <summary>
        /// 执行方法后
        /// </summary>
        /// <param name="attributes">过滤类型</param>
        /// <param name="executionObj">执行对象</param>
        /// <param name="impResult">结果</param>
        /// <param name="returnType">结果类型</param>
        /// <returns></returns>
        private bool AfterxecutionExecutionAttribte(List<object> attributes,BaseProvideServices executionObj,ref object impResult, Type returnType) {
            var isImplement = true;
            foreach (RequestFilterAttribte item in attributes)
            {
                if (!item.Afterxecution(executionObj, ref impResult, returnType))
                {
                    isImplement = false;
                }
            }
            return isImplement;
        }
    }
}
