using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using unwdmi.Protobuf;

namespace unwdmi.Parser
{
    internal class Listener
    {
        private readonly Controller _controller;

        private Socket _listener;

        private Task _onParsersFinishedTask = Task.CompletedTask;
        private DateTime lastRun = DateTime.UtcNow;

        public Listener(Controller instance)
        {
            _controller = instance;
        }

        public void StartListening()
        {
            var ip = new IPEndPoint(IPAddress.Any, 7789);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Blocking = false;
            _listener.Bind(ip);
            _listener.Listen(200);
            _listener.BeginAccept(AcceptCallBack, _listener);
        }


        private void AcceptCallBack(IAsyncResult ar)
        {
            // Program can act weird if all 800 connections are opened at once.
            // In real-life applications this shouldn't be a problem.
            _listener.BeginAccept(AcceptCallBack, _listener);

            var listener = (Socket) ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var stateObject = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0, ReceiveCallback, stateObject);
            Interlocked.Increment(ref _controller.OpenSockets);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Interlocked.Increment(ref _controller.ActiveReceivers);
            var so = (StateObject) ar.AsyncState;
            var workSocket = so.workSocket;
            try
            {
                var read = workSocket.EndReceive(ar);

                if (read > 0)
                {
                    var str = Encoding.ASCII.GetString(so.buffer, 0, read);
                    so.sb.Append(str);
                    if (read < StateObject.BUFFER_SIZE && str.IndexOf("ATA>", str.Length - 6 >= 0 ? str.Length - 6 : 0,
                            StringComparison.Ordinal) > -1)
                    {
                        var strContent = so.sb.ToString();
                        Interlocked.Increment(ref _controller.ActiveParsers);
                        Task.Run(() =>
                        {
                            try
                            {
                                _controller.Parser.ParseXML(strContent);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _controller.ActiveParsers);
                                if (_controller.ActiveParsers == 0 && _onParsersFinishedTask.IsCompleted)
                                {
                                    List<Measurement> measurements;
                                    lock (_controller.MeasurementQueue)
                                    {
                                        measurements = _controller.MeasurementQueue.ToList();
                                        _controller.MeasurementQueue = new ConcurrentBag<Measurement>();
                                    }

                                    lastRun = DateTime.UtcNow;
                                    _onParsersFinishedTask = Task.Run(() =>
                                    {
                                        _controller.OnParsersFinished(measurements);
                                    });
                                }
                            }
                        });
                        so.sb.Clear();
                    }

                    workSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, ReceiveCallback, so);
                }
                else
                {
                    if (so.sb.Length > 1) so.sb.Clear();

                    workSocket.Close();

                    Interlocked.Decrement(ref _controller.OpenSockets);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Interlocked.Decrement(ref _controller.ActiveReceivers);
        }
    }

    public class StateObject
    {
        // 4096 fits all the XML files, so the checks don't have to be done as often, saves a bit of CPU, costs a bit more ram.
        public const int BUFFER_SIZE = 4096;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
        public Socket workSocket;
    }
}