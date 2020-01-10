using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FRPCClient.Entity
{
    /// <summary>
    /// 任务存储的信息
    /// </summary>
    public class RemoteCallEntrity
    {

        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }


        /// <summary>
        /// 通讯对象
        /// </summary>
        public FRPCEasyClient ClientSocket { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public ReceiveMessageState State { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 远程任务处理结果
        /// </summary>
        public string ReturnValue { get; set; }


        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 任务信息
        /// </summary>
        public RequestExecutiveInformation TaskInfo { get; set; }

        /// <summary>
        /// 通知执行委托
        /// </summary>
        public EventWaitHandle WaitHandle { get; set; }

        /// <summary>
        /// 任务对象
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="taskInfo">传输对象</param>
        /// <param name="state">状态</param>
        /// <param name="expirationTime">超时时间</param>
        /// <param name="socket">连接对象</param>
        public RemoteCallEntrity(Guid id, RequestExecutiveInformation taskInfo, ReceiveMessageState state, DateTime expirationTime, FRPCEasyClient socket) {
            this.ID = id;
            this.TaskInfo = taskInfo;
            this.State = state;
            this.ExpirationTime = expirationTime;
            this.ClientSocket = socket;

            WaitHandle = new AutoResetEvent(false);
        }
        /// <summary>
        /// 修改结果并执行委托函数
        /// </summary>
        /// <param name="rpcResult"></param>
        public void ProcessingFuncInvoke(string rpcResult) {
            ReturnValue = rpcResult;
            WaitHandle.Set();

        }

        /// <summary>
        /// 修改结果并执行委托函数
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="rpcResult">结果</param>
        public void ProcessingFuncInvoke(ReceiveMessageState state,string rpcResult)
        {
            State = state;
            ReturnValue = rpcResult;
            WaitHandle.Set();

        }
    }
}
