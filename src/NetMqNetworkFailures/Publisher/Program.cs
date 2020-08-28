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
            var pub = new ZeroMqBindCarrelliPublisher("tcp://0.0.0.0:2020");
            pub.Start();

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    pub.Enqueue(new MyObject { Tick = DateTime.Now } );
                    await Task.Delay(1000);
                }
            }, TaskCreationOptions.LongRunning);

            Console.WriteLine("Publisher started");
            var mre = new ManualResetEvent(false);
            mre.WaitOne();
        }

    }

}
