using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCClientAOPContainer
{
    /// <summary>
    /// AOP过滤器实体
    /// </summary>
    public class AOPFilterEntity
    {
        /// <summary>
        /// 检查类型FullName
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 是否立即返回做异步处理
        /// </summary>
        public bool IsReturn { get; set; }

        /// <summary>
        /// IsReturn = false 的情况下 完成远程请求后替换结果
        /// </summary>
        public bool IsReplaceResult { get; set; }

        /// <summary>
        /// 返货结果
        /// </summary>
        public object Result { get; set; }
    }
}
