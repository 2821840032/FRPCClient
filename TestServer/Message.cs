using ITestServer;
using System;
using FRPCClient.Entity;
namespace TestServer
{
    public class Message :BaseProvideServices,IMessage
    {
        public void ShowMessage(string msg)
        {
            Console.WriteLine($"{this.RequestClientSession.Value} 发来消息"+msg);
        }
    }
}
