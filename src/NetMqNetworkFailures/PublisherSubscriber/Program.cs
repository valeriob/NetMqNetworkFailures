using MessagePack;
using OnRtls.ZeroMq;
using Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var sub = new ZeroMqConnectSubscriber("", "tcp://127.0.0.1:2020");
            sub.Subscribe(Ricevuto);
            sub.Start();

            var pub = new ZeroMqBindCarrelliPublisher("tcp://0.0.0.0:2020");
            pub.Start();

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    pub.Enqueue(new MyObject { Tick = DateTime.Now });
                    await Task.Delay(1000);
                }
            }, TaskCreationOptions.LongRunning);

            Console.WriteLine("Publisher started");

            string line = "";
            while ((line = Console.ReadLine()) != "exit")
            {
                pub.Stop();
                Thread.Sleep(1 * 1000);
                pub.Start();

                Console.WriteLine("Publisher Restarted");
            }
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
