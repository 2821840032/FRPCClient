using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPCClient
{
    
    /// <summary>
    /// 二进制请求解析器
    /// </summary>
    public class FRPCReceiveFilter : SuperSocket.ProtoBase.FixedHeaderReceiveFilter<RequestBaseInfo>
    {
        /// <summary>
        /// 二进制解析器
        /// </summary>
        public FRPCReceiveFilter()
       : base(4)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferStream"></param>
        /// <returns></returns>
        public override RequestBaseInfo ResolvePackage(IBufferStream bufferStream)
        {
            var last = bufferStream.Buffers.Last();
            var body = last.Array.Skip(last.Offset).Take(last.Count).ToArray();
            return new RequestBaseInfo(body);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferStream"></param>
        /// <param name="length"></param>
        /// <returns></returns>

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            //前四位是 长度 后面是内容
            return BitConverter.ToInt32(bufferStream.Buffers.Last().Take(4).ToArray(), 0);
        }
    }
}
