using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;

namespace unwdmi.Parser
{
    class Listener
    {
        public Listener(Controller instance)
        {
            _controller = instance;
        }

        private readonly Controller _controller;

        private Socket _listener;

        private Task _onParsersFinishedTask = Task.CompletedTask;

        public void StartListening()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 7789);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Blocking = false;
            _listener.Bind(ip);
            _listener.Listen(200);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }


        void AcceptCallBack(IAsyncResult ar)
        {
            // Program can act weird if all 800 connections are opened at once.
            // In real-life applications this shouldn't be a problem.
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0, ReceiveCallback, stateObject);
            Interlocked.Increment(ref _controller.OpenSockets);
        }

        void ReceiveCallback(IAsyncResult ar)
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
                                if (_controller.ActiveParsers == 0)
                                {
                                    if(_onParsersFinishedTask.IsCompleted)
                                        _onParsersFinishedTask = Task.Run(async () =>
                                        {
                                            await Task.Delay(100);
                                            while (_controller.ActiveReceivers != 0 && _controller.ActiveParsers != 0)
                                            {
                                                await Task.Delay(50);
                                            }
                                            _controller.OnParsersFinished();
                                        });
                                }
                            }
                        });
                        so.sb.Clear();
                    }

                    workSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), so);
                }
                else
                {
                    if (so.sb.Length > 1)
                    {
                        so.sb.Clear();
                    }

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
        public Socket workSocket;
        // 4096 fits all the XML files, so the checks don't have to be done as often, saves a bit of CPU, costs a bit more ram.
        public const int BUFFER_SIZE = 4096;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
    }
}
