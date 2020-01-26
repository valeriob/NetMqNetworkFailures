using MessagePack;
using NetMQ;
using NetMQ.Sockets;
using System;

namespace OnRtls.ZeroMq
{
    public abstract class ZeroMqPublisher
    {
        PublisherSocket _pubSocket;
        NetMQQueue<object> _queue;
        NetMQPoller _poller;

        public ZeroMqPublisher()
        {
            _pubSocket = new PublisherSocket();
            _pubSocket.Options.SendHighWatermark = 10;
            _pubSocket.Options.DelayAttachOnConnect = true;

            _queue = new NetMQQueue<object>(10);

            _queue.ReceiveReady += (sender, args) => Publish(_queue.Dequeue());
            _poller = new NetMQPoller { _queue };
        }

        public void Start()
        {
            OnStart(_pubSocket);
            _poller.RunAsync();
        }

        protected abstract void OnStart(PublisherSocket socket);



        public void Enqueue(object evnt)
        {
            _queue.Enqueue(evnt);
        }

        void Publish(object evnt)
        {
            try
            {
                var body = MessagePackSerializer.Typeless.Serialize(evnt);
                var name = evnt.GetType().Name;
                _pubSocket.SendMoreFrame("").SendMoreFrame(name).SendFrame(body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Stop()
        {
            _poller.Stop();
            OnStop(_pubSocket);
        }

        protected abstract void OnStop(PublisherSocket socket);

    }

    public class ZeroMqBindCarrelliPublisher : ZeroMqPublisher
    {
        string _bindingAddress;

        public ZeroMqBindCarrelliPublisher(string bindingAddress)
        {
            _bindingAddress = bindingAddress;
        }

        protected override void OnStart(PublisherSocket socket)
        {
            socket.Bind(_bindingAddress);
        }
        protected override void OnStop(PublisherSocket socket)
        {
            socket.Unbind(_bindingAddress);
        }

    }

    public class ZeroMqConnectCarrelliPublisher : ZeroMqPublisher
    {
        string _connectAddress;
        public ZeroMqConnectCarrelliPublisher(string connectAddress)
        {
            _connectAddress = connectAddress;
        }

        protected override void OnStart(PublisherSocket socket)
        {
            socket.Connect(_connectAddress);
        }

        protected override void OnStop(PublisherSocket socket)
        {
            socket.Disconnect(_connectAddress);
        }

    }
}
