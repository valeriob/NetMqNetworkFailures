using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading.Tasks;

namespace OnRtls.ZeroMq
{
    public abstract class ZeroMqSubscriber
    {

        SubscriberSocket _subSocket;
        string _topic;
        bool _running;

        public ZeroMqSubscriber(string topic)
        {
            _topic = topic;
            _subSocket = new SubscriberSocket();
        }


        public void Start()
        {
            _subSocket.Subscribe(_topic);
            OnStart(_subSocket);

            _running = true;
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        protected abstract void OnStart(SubscriberSocket socket);


        void Run()
        {
            while (_running)
            {
                try
                {
                    string topic;
                    if (_subSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out topic))
                    {
                        var eventName = _subSocket.ReceiveFrameString();
                        var messageReceived = _subSocket.ReceiveFrameBytes();
                        try
                        {
                            MessaggioRicevuto(eventName, messageReceived);
                        }
                        catch
                        {
                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Console.WriteLine(this.GetType().Name + " : " + ex.Message);
                    }
                }
            }
        }


        public virtual void Stop()
        {
            if (_running)
            {
                _running = false;
                OnStop(_subSocket);
                _subSocket.Close();
            }
        }

        protected abstract void OnStop(SubscriberSocket socket);

        protected virtual void MessaggioRicevuto(string eventName, byte[] message)
        {
            _messaggioRicevuto?.Invoke(eventName, message);
        }

        Action<string, byte[]> _messaggioRicevuto;
        public void Subscribe(Action<string, byte[]> messaggioRicevuto)
        {
            _messaggioRicevuto = messaggioRicevuto;
        }
    }


    public class ZeroMqConnectSubscriber : ZeroMqSubscriber
    {
        string _address;
        public ZeroMqConnectSubscriber(string topic, string address) : base(topic)
        {
            _address = address;
        }

        protected override void OnStart(SubscriberSocket socket)
        {
            socket.Connect(_address);
        }

        protected override void OnStop(SubscriberSocket socket)
        {
            socket.Disconnect(_address);
        }

    }

    public class ZeroMqBindSubscriber : ZeroMqSubscriber
    {
        string _address;
        public ZeroMqBindSubscriber(string topic, string address) : base(topic)
        {
            _address = address;
        }

        protected override void OnStart(SubscriberSocket socket)
        {
            socket.Bind(_address);
        }

        protected override void OnStop(SubscriberSocket socket)
        {
            socket.Unbind(_address);
        }
    }
}
