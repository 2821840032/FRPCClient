using CommandLine;
using CommandLine.Text;
using FRPCClientAOPContainer;
using FRPCClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace FRPCClientCommandConsole.CommandOptionsMap
{
    /// <summary>
    /// 执行远程访问
    /// </summary>
    [Verb("IMP", HelpText = "执行远程访问")]
    public class RealizationServerOptions
    {
        /// <summary>
        /// 
        /// </summary>
        [Option('i', "id", Required = false, HelpText = "ID")]
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Option('f', "fullname", Required = false, HelpText = "执行的FullName")]
        public string Fullname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Option('n', "name", Required = false, HelpText = "函数名称")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Option('p', "para", Required = false, HelpText = "参数")]
        public IEnumerable<string> Para { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Option('i', "session", Required = false, HelpText = "强制指定某个client完成 用于被转发的请求")]
        public Guid? RRPCSessionID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandExecutionRPClist"></param>
        /// <param name="aOPContainer"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public int Run(List<Type> commandExecutionRPClist, AOPContainer aOPContainer,FRPCEasyClient client)
        {
            try
            {
                MethodInfo objMethod;
                var objType = commandExecutionRPClist.FirstOrDefault(d => d.FullName.Equals(Fullname));
                if (objType == null)
                {
                    Console.WriteLine($"没有找到{Fullname}的类型");
                    return 1;
                }


                objMethod = objType.GetMethods().FirstOrDefault(d => d.Name.Equals(Name));
                if (objMethod == null)
                {
                    Console.WriteLine($"没有找到{Name}函数");
                    return 1;
                }

                var paras = objMethod.GetParameters();
                if (Para.Count() != paras.Count())
                {
                    Console.WriteLine($"参数数量不对 应为{paras.Count()}");
                    return 1;
                }
                List<object> paraList = new List<object>();

                var paraToList = Para.ToList();
                try
                {
                    for (int i = 0; i < paraToList.Count(); i++)
                    {
                        paraList.Add(JsonConvert.DeserializeObject(paraToList[i], paras[i].ParameterType));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("格式化参数失败" + e.Message);
                }

                var obj = aOPContainer.GetServices(client, objType, RRPCSessionID);

                var result = objMethod.Invoke(obj, paraList.ToArray());

                Console.WriteLine("调用完成:" + JsonConvert.SerializeObject(result));
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("IMP ERRER" +e.Message);
                return 0;
            }
       
        }
    }
}
