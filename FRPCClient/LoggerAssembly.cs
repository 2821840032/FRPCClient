using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCClient
{
    /// <summary>
    /// 日志组件
    /// </summary>
    public static class LoggerAssembly
    {
        /// <summary>
        /// 日志列表
        /// </summary>
        public static List<LoggerInfo> LoggerList { get;private set; } = new List<LoggerInfo>();

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="socket">连接</param>
        /// <param name="loggerType">日志类型</param>
        /// <param name="message">信息</param>
        public static void Log(this FRPCEasyClient socket, string message, LoggerType loggerType = LoggerType.Info)
        {
            Console.WriteLine(DateTime.Now+":"+loggerType+":"+message);
            LoggerList.Add(new LoggerInfo() { LoggerType = loggerType, Message = message });
        }

    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LoggerType { 
        /// <summary>
        /// 异常
        /// </summary>
        Error=0,
        /// <summary>
        /// 警告
        /// </summary>
        Warning=1,
        /// <summary>
        /// 信息
        /// </summary>
        Info=2
    }
    /// <summary>
    /// 日志信息
    /// </summary>
    public class LoggerInfo {
    
        /// <summary>
        /// 日志类型
        /// </summary>
        public LoggerType LoggerType { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string  Message { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
