using CommandLine;
using FRPCClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;
using FRPCClient.Entity;
using System.Data;

namespace FRPCClientCommandConsole.CommandOptionsMap
{
    /// <summary>
    /// 信息命令
    /// </summary>
    [Verb("info", HelpText = "信息")]
   public class InfoOptions
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="commandExecutionRPC"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public int Run(DateTime dateTime,List<Type> commandExecutionRPC, FRPCEasyClient client)
        {
            Console.WriteLine("开始时间："+dateTime);
            Console.WriteLine("远程地址："+client.RemoteEndpoint.ToString());
            Console.WriteLine("命令可调用对象数量："+commandExecutionRPC.Count);
            Console.WriteLine("任务数量："+client.RemoteCallQueue.MethodCallQueues.Count);
            Console.WriteLine("任务异常任务数量："+client.RemoteCallQueue.MethodCallQueues.Values.Where(d=>d.State== ReceiveMessageState.Error).Count());
            Console.WriteLine("运行异常数量："+ LoggerAssembly.LoggerList.Where(d=>d.LoggerType== LoggerType.Error).Count());
            return 0;
        }
    }
}
