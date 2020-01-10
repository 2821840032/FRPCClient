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
    /// 执行查询信息命令
    /// </summary>
    [Verb("Select", HelpText = "查询信息")]
   public class SelectOptions
    {
        /// <summary>
        /// 
        /// </summary>
        [Option('n', "name", Required = true, HelpText = "查询的对象名称 Task 任务列表\tRPCServer 远程执行函数列表\t Log 日志")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Option('o', "order", Required = false, HelpText = "排序名称")]
        public string OrderName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Option('p', "orderDesc", Required = false, HelpText = "是否升序")]
        public bool OrderDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandExecutionRPC"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public int Run(List<Type> commandExecutionRPC, FRPCEasyClient client)
        {
         
            DataTable table = new DataTable();
          
            switch (Name)
            {
                case "Task":
                    table.Columns.Add("Key");
                    table.Columns.Add("State");
                    table.Columns.Add("ExpirationTime");
                    table.Columns.Add("RetryCount");
                    table.Columns.Add("ReturnValue");
                    var list = client.RemoteCallQueue.MethodCallQueues.Select(d=>d.Value).ToList();
                    if (!string.IsNullOrWhiteSpace(OrderName))
                    {
                        list = OrderBy<RemoteCallEntrity>(list.AsQueryable(), OrderName, OrderDesc).ToList();
                    }
                    foreach (var item in list)
                    {
                        table.Rows.Add(item.ID, item.State, item.ExpirationTime, item.RetryCount, item.ReturnValue);
                    }
                    PrintTable(table);
                    break;
                case "RPCServer":
                    table.Columns.Add("FullName");
                    table.Columns.Add("Method");
                    table.Columns.Add("Para(Count)");
               
                    foreach (var item in commandExecutionRPC)
                    {
                        foreach (var method in item.GetMethods())
                        {
                            table.Rows.Add(item.FullName, method.Name, string.Join(",", method.GetParameters().Select(d => d.ParameterType.Name)) + $"({method.GetParameters().Count()})");
                        }
                    }
                    PrintTable(table);
                    break;
                case "Log":
                    table.Columns.Add("LoggerType");
                    table.Columns.Add("Time");
                    table.Columns.Add("Message");
                    var logList = LoggerAssembly.LoggerList.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(OrderName))
                    {
                        logList = OrderBy<LoggerInfo>(logList, OrderName, OrderDesc);
                    }
                    foreach (var item in logList)
                    {
                        table.Rows.Add(item.LoggerType, item.Time, item.Message);
                    }
                    PrintTable(table);
                    break;
                default:
                    Console.WriteLine("name 参数错误 因为Task 任务列表\tRPCServer 远程执行函数列表");
                    return 1;
            }
            Console.WriteLine("数量：" + table.Rows.Count);
            return 0;

        }

        /// <summary>
        /// 根据指定属性名称对序列进行排序
        /// </summary>
        /// <typeparam name="TSource">source中的元素的类型</typeparam>
        /// <param name="source">一个要排序的值序列</param>
        /// <param name="property">属性名称</param>
        /// <param name="descending">是否降序</param>
        /// <returns></returns>
        private IQueryable<TSource> OrderBy<TSource>(IQueryable<TSource> source, string property, bool descending) where TSource : class
        {
            ParameterExpression param = Expression.Parameter(typeof(TSource), "c");
            PropertyInfo pi = typeof(TSource).GetProperty(property);
            MemberExpression selector = Expression.MakeMemberAccess(param, pi);
            LambdaExpression le = Expression.Lambda(selector, param);
            string methodName = (descending) ? "OrderByDescending" : "OrderBy";
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(TSource), pi.PropertyType }, source.Expression, le);
            return source.Provider.CreateQuery<TSource>(resultExp);
        }

        /// <summary>
        /// Print a DataTable on to the console
        /// </summary>
        /// <param name="table">the table to be printed</param>
        public void PrintTable(DataTable table)
        {
            // print head
            PrintLine(24 * table.Columns.Count);
            foreach (DataColumn col in table.Columns)
            {
                Console.Write(string.Format("{0,24}", col.Caption));
            }
            Console.Write("\n");
            PrintLine(24 * table.Columns.Count);

            // print rows
            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    Console.Write(string.Format("{0,24}", table.Rows[i][j].ToString()));
                }
                Console.Write("\n");
            }
            PrintLine(24 * table.Columns.Count, "-");
        }

        /// <summary>
        /// Print a line with specific char on to the console
        /// </summary>
        /// <param name="length">count of the char to be printed</param>
        /// <param name="lineChar">the char to be printed, default is "="</param>
        private static void PrintLine(int length, string lineChar = "=")
        {
            string line = string.Empty;
            for (int i = 0; i < length; i++)
            {
                line += lineChar;
            }
            Console.WriteLine(line);
        }
    }
}
