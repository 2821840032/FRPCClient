using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace FRPCClient.Entity
{
    /// <summary>
    /// 提供服务的基础类 如果你需要用到其中的信息的话
    /// </summary>
   public class BaseProvideServices
    {
        /// <summary>
        /// 操作连接对象
        /// </summary>
        public FRPCEasyClient Socket { get; set; }

        /// <summary>
        /// 本次任务的信息
        /// </summary>
        public RequestExecutiveInformation Info { get; set; }

        /// <summary>
        /// 最基础的请求信息
        /// </summary>
        public RequestBaseInfo RequestInfo { get; set; }

        /// <summary>
        /// 容器对象
        /// </summary>
        public IUnityContainer Container { get; internal set; }

        /// <summary>
        /// 如果是转发的 那么他将会有值
        /// 用来记录是谁发起的请求
        /// 请求客户端Session
        /// </summary>
        public Guid? RequestClientSession { get; set; }
    }
}
