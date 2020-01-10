using System;

namespace ITestServer
{
   public interface IMessage
    {
        /// <summary>
        /// 显示文字
        /// </summary>
        /// <param name="msg">内容</param>
        void ShowMessage(string msg);
    }
}
