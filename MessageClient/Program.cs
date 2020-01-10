using FRPCClient;
using FRPCClientAOPContainer;
using ITestServer;
using System;
using TestServer;

namespace MessageClient
{
    class Program
    {
        static void Main(string[] args)
        {
            FRPCEasyClient client = new FRPCEasyClient("127.0.0.1", 2012,null, 1500, 0);
            AOPContainer Container = new AOPContainer();

            client.AddProvideServices<IMessage, Message>();

            string ll = Console.ReadLine();
            while ("q" != ll)
            {
                Container.GetServices<IMessage>(client).ShowMessage("Hello ");

                ll = Console.ReadLine();
            }
            Console.WriteLine("Over");
        }
    }
}
