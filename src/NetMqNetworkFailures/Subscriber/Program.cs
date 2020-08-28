using MessagePack;
using OnRtls.ZeroMq;
using Shared;
using System;
using System.Threading;

namespace NetMqTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var sub = new ZeroMqConnectSubscriber("", "tcp://127.0.0.1:2020");
            sub.Subscribe(Ricevuto);
            sub.Start();

            Console.WriteLine("Subscribed");
         
            var mre = new ManualResetEvent(false);
            mre.WaitOne();
        }

        static void Ricevuto(string evnt, byte[] msg)
        {
            var body = (MyObject)MessagePackSerializer.Typeless.Deserialize(msg);
            Console.WriteLine($"{DateTime.Now.TimeOfDay} Msg Received {body.Tick}");
        }
    }
}
