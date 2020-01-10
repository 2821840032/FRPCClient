using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCClient.Entity
{
    /// <summary>
    /// 接收信息的状态
    /// </summary>
    public enum ReceiveMessageState
    {
        /// <summary>
        /// 等待
        /// </summary>
        Wait=0,
        /// <summary>
        /// 成功
        /// </summary>
        Success=1,
        /// <summary>
        /// 超时
        /// </summary>
        Overtime=2,
        /// <summary>
        /// 错误
        /// </summary>
        Error =3,
        /// <summary>
        /// 其他
        /// </summary>
        Other=4,
    }
}
