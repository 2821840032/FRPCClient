using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FRPCClient.Entity;
using System.Threading;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FRPCClient
{
    /// <summary>
    /// 任务队列管理
    /// </summary>
    public class RemoteCallQueue
    {
        /// <summary>
        /// 超时时间 秒
        /// </summary>
        public int OvertimeSecond { get; private set; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; private set; }

        private Timer HealthExaminationThread;

        private Timer ScheduledCleaningThread;

        /// <summary>
        /// 任务列表
        /// </summary>
        public ConcurrentDictionary<Guid, RemoteCallEntrity> MethodCallQueues
        {
            get;private set;
        }

        /// <summary>
        /// 开启队列管理
        /// </summary>
        /// <param name="second">任务超时时间</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        public RemoteCallQueue(int second,int maxRetryCount)
        {
            this.OvertimeSecond = second;
            MethodCallQueues = new ConcurrentDictionary<Guid, RemoteCallEntrity>();
            MaxRetryCount = maxRetryCount;
            Thread thread = new Thread(TimerInit);
            thread.Start();

        }

        private void TimerInit() {

            HealthExaminationThread = new Timer(HealthExaminationFunc, null, 1000 * 5, 1000 * 5);

            ScheduledCleaningThread = new Timer(ScheduledCleaningFunc, null, 60000, 60000);
        }

        /// <summary>
        /// 添加一个任务到队列
        /// </summary>
        /// <param name="info">信息</param>
        /// <param name="socket">远程连接</param>
        /// <returns></returns>
        public RemoteCallEntrity AddTaskQueue(RequestExecutiveInformation info, FRPCEasyClient socket) {
            var result = new RemoteCallEntrity(info.ID, info, ReceiveMessageState.Wait, DateTime.Now.AddSeconds(OvertimeSecond),socket);
            MethodCallQueues.TryAdd(info.ID, result);
            return result;
        }

        /// <summary>
        /// 进行远程调用
        /// </summary>
        /// <param name="info">通讯的信息</param>
        public async Task RemoteExecutionFuncAsync(RemoteCallEntrity info) {
            await Task.Yield();
            RemoteExecutionFunc(info);
        }

        /// <summary>
        /// 进行远程调用
        /// </summary>
        /// <param name="info">通讯的信息</param>
        public  void RemoteExecutionFunc(RemoteCallEntrity info)
        {
            var msg = JsonConvert.SerializeObject(info.TaskInfo);
            try
            {
                info.ClientSocket.SendMessage(msg);
            }
            catch (Exception e)
            {
                info.ClientSocket.Log("通讯出现异常 请求没有没发送成功" + e.Message, LoggerType.Error);
                info.ProcessingFuncInvoke(ReceiveMessageState.Error, e.Message);
            }
        }

        /// <summary>
        /// 监控检查函数
        /// </summary>
        private void HealthExaminationFunc(object source)
        {
            foreach (var item in MethodCallQueues.Where(d => d.Value != null && DateTime.Now > d.Value.ExpirationTime && d.Value.State == ReceiveMessageState.Wait).ToList())
            {
                if (item.Value.RetryCount<MaxRetryCount)
                {
                    item.Value.RetryCount++;
                    item.Value.ExpirationTime = DateTime.Now.AddSeconds(OvertimeSecond);
                    //重发
                    RemoteExecutionFuncAsync(item.Value);
                }
                else
                {
                    item.Value.ProcessingFuncInvoke(ReceiveMessageState.Overtime, $"Timeout to {OvertimeSecond} Second");
                }

            }
        }
        /// <summary>
        /// 定时清理函数
        /// </summary>
        private void ScheduledCleaningFunc(object source)
        {
            foreach (var item in MethodCallQueues.Where(d => DateTime.Now> d.Value.ExpirationTime.AddHours(60)).ToList())
            {
                MethodCallQueues.TryRemove(item.Key, out var value);
            }
        }

        /// <summary>
        /// 根据任务ID获取任务信息并修改状态为以完成
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="rpcResule">内容</param>
        /// <returns>true 找到并修改信息 false未找到</returns>
        public bool GetTaskIDAndSuccess(Guid id,string rpcResule) {
            if (MethodCallQueues.TryGetValue(id, out var value))
            {
                if (value.State == ReceiveMessageState.Wait)
                {
                    value.ProcessingFuncInvoke(ReceiveMessageState.Success,rpcResule);
                    return true;
                }
                else {
                    value.ClientSocket.Log($"任务状态已经被更改过一次 现在它又收到了一个结果 ID:{value.ID} Result:{rpcResule} State:{value.State}", LoggerType.Error);
                    return true;
                }
             
            }
            else {
                //没有找到这个任务的信息
                return false;
            }

        }
    }
}
