using OnRtls.ZeroMq;
using System;
using System.Threading;

namespace NetMqTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var pub = new ZeroMqConnectSubscriber("", "tcp://localhost:2020");
            pub.Subscribe(Ricevuto);
            pub.Start();

            Console.WriteLine("Subscribed");
         
            var mre = new ManualResetEvent(false);
            mre.WaitOne();
        }

        static void Ricevuto(string evnt, byte[] msg)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay} Msg Received");
        }
    }
}
