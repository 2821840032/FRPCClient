using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPCClient
{
    /// <summary>
    /// 接受信息最基础的类
    /// </summary>
    public class RequestBaseInfo : IPackageInfo
    {
        /// <summary>
        /// 传输值
        /// </summary>
        public byte[] body { get; set; }
        /// <summary>
        /// 传输状态最基础的类 除开头部length
        /// </summary>
        /// <param name="body"></param>
        public RequestBaseInfo(byte[] body)
        {
            this.body = body;
        }
        /// <summary>
        /// Utf-8解析body
        /// </summary>
        public string bodyMeg => Encoding.UTF8.GetString(body);
    }
}
