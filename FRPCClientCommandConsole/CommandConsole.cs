using FRPCClientCommandConsole.CommandOptionsMap;
using CommandLine;
using FRPCClient;
using FRPCClientAOPContainer;
using System;
using System.Collections.Generic;

namespace FRPCClientCommandConsole
{
    /// <summary>
    /// 命令控制台
    /// </summary>
    public class CommandConsole
    {
        /// <summary>
        /// 可以被命令执行的远程调用
        /// </summary>
        List<Type> CommandExecutionRPC { get; set; }

        /// <summary>
        /// 程序启动时间
        /// </summary>
        DateTime StartDateTime { get; set; }

        FRPCEasyClient socketClient;
        AOPContainer aOPContainer;
        /// <summary>
        /// 初始化
        /// </summary>
        public CommandConsole(FRPCEasyClient socketClient, AOPContainer aOPContaine) {
            this.socketClient = socketClient;
            this.aOPContainer = aOPContaine;
            StartDateTime = DateTime.Now;
            CommandExecutionRPC = new List<Type>();
        }
        /// <summary>
        /// 启动监听命令
        /// </summary>
        public void MonitorCommand() {
                Monitor();
        }
        int Monitor()
        {
            var args = Console.ReadLine().Split(' ');

            var exitCode = Parser.Default.ParseArguments<SelectOptions, RealizationServerOptions,InfoOptions>(args)
                .MapResult(
                     (InfoOptions o) => o.Run(StartDateTime,CommandExecutionRPC, socketClient),
                     (SelectOptions o) => o.Run(CommandExecutionRPC,socketClient),
                     (RealizationServerOptions o) => o.Run(CommandExecutionRPC,aOPContainer, socketClient),
                    error => 1);
            return exitCode;
        }

        /// <summary>
        /// 添加一个可以用命令访问的接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ADDCommandExecutionRPC<T>()
        {
            CommandExecutionRPC.Add(typeof(T));

        }

    }
}
