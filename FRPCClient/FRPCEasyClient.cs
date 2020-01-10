using Newtonsoft.Json;
using SuperSocket.ClientEngine;
using FRPCClient;
using FRPCClient.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FRPCClient
{
    /// <summary>
    /// Client Socket
    /// </summary>
   public class FRPCEasyClient : EasyClient
    {
        
        /// <summary>
        /// unity 容器对象 一般用来存储如 数据库连接对象 工具之类的单例或者工厂
        /// 在RPCsetup中全局唯一 且能在服务中获取到它
        /// </summary>
        public IUnityContainer UnityContainer { get; set; }

        /// <summary>
        /// 接收请求处理类
        /// </summary>
        private MonitorReceived MonitorReceivedHandle { get; set; }

        /// <summary>
        /// 远程地址
        /// </summary>
        public IPEndPoint RemoteEndpoint { get;private set; }

        /// <summary>
        /// 远程任务队列
        /// </summary>
        public RemoteCallQueue RemoteCallQueue { get;private set; }

        RetryMechanism RetryMechanismHandle { get; set; }

        /// <summary>
        /// 异常处理 在执行Invoke的时候触发
        /// </summary>
        public  RequestException Requestexception { get; set; }

        /// <summary>
        ///  创建一个连接对象
        /// </summary>
        /// <param name="ip">远程地址</param>
        /// <param name="prot">端口</param>
        /// <param name="maxRetryCount">最大超时后重试次数</param>
        /// <param name="acion">初始化注入的对象</param>
        /// <param name="second">超时时间</param>
        /// <param name="immediateConnection">是否为立即连接</param>
        public FRPCEasyClient(string ip, int prot, Action<IUnityContainer> acion=null,int second=10,int maxRetryCount=3, bool immediateConnection=true) {
            RemoteCallQueue = new RemoteCallQueue(second, maxRetryCount);
            UnityContainer = new UnityContainer();
            acion?.Invoke(UnityContainer);

            MonitorReceivedHandle = new MonitorReceived(this);

            RetryMechanismHandle = new RetryMechanism(this);

            Error += RetryMechanismHandle.OnError;
            Closed += RetryMechanismHandle.OnClose;
            RemoteEndpoint = new IPEndPoint(IPAddress.Parse(ip), prot);
            Initialize(new FRPCReceiveFilter(), MonitorReceivedHandle.Handle);
            
            if (immediateConnection)
            {
                RetryMechanismHandle.ConnectionInit();
            }
        }

        /// <summary>
        /// 启动连接
        /// </summary>
        /// <returns></returns>
        public async Task ConnectionStart() {
            await RetryMechanismHandle.ConnectionInit();
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            if (!IsConnected)
            {
                this.Log("发送请求失败 当前状态未连接 消息内容"+message, LoggerType.Error);
                return;
            }
            var dataBody = Encoding.UTF8.GetBytes(message);

            var dataLen = BitConverter.GetBytes(dataBody.Length);//int类型占4位，根据协议这里也只能4位，否则会出错

            var sendData = new byte[4 + dataBody.Length];//长度为4

            // +-------+-------------------------------+
            // |request|                               |
            // | name  |    request body               |
            // |  (4)  |                               |
            // |       |                               |
            // +-------+-------------------------------+

            Array.ConstrainedCopy(dataLen, 0, sendData, 0, 4);
            Array.ConstrainedCopy(dataBody, 0, sendData, 4, dataBody.Length);

            base.Send(sendData);
        }

        /// <summary>
        /// 为所有启动的服务器注册服务
        /// </summary>
        /// <typeparam name="IT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public void AddServer<IT, T>()
           where IT : class
           where T : IT
        {
            MonitorReceivedHandle.ServiceProvider.AddServer<IT, T>();
        }

        /// <summary>
        /// 为所有启动的客户端注册服务 
        /// 此方法会注入特定属性
        /// </summary>
        /// <typeparam name="IT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public void AddProvideServices<IT, T>()
           where IT : class
           where T : BaseProvideServices, IT
        {
            MonitorReceivedHandle.ServiceProvider.AddProvideServices<IT, T>();
        }
    }
}
