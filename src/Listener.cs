using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Console = System.Console;

namespace unwdmi.Parser
{
    class Listener
    {
        public Listener(Controller instance)
        {
            controller = instance;
            WeatherStationsDictionary = controller.WeatherStations;
        }

        private Controller controller;

        private Socket _listener;

        public ConcurrentDictionary<int, WeatherStation> WeatherStationsDictionary;

        public void StartListening()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 7789);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Blocking = false;
            _listener.Bind(ip);
            _listener.Listen(100);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }


        void AcceptCallBack(IAsyncResult ar)
        {
            //Program can act weird if all 800 connections are opened at once.
            //In real-life applications this shouldn't be a problem.
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), stateObject);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject) ar.AsyncState;
            Socket s = so.workSocket;

            try
            {
                int read = s.EndReceive(ar);

                if (read > 0)
                {
                    string str = Encoding.ASCII.GetString(so.buffer, 0, read);
                    so.sb.Append(str);
                    if (str.IndexOf("</WEATHERDATA>", str.Length - 20 >= 0 ? str.Length - 20 : 0,
                            StringComparison.Ordinal) > -1)
                    {
                        var strContent = so.sb.ToString();
                        Task task = so.CurrentTask;
                        so.CurrentTask = Task.Run(() =>
                        {
                            task.Wait();
                            controller.Parser.ParseXML(strContent);
                        });
                        so.sb.Clear();
                    }

                    s.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), so);
                }
                else
                {
                    if (so.sb.Length > 1)
                    {
                        so.sb.Clear();
                    }

                    s.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class StateObject
    {
        public static int CurrentlyActiveTasks = 0;
        public Socket workSocket = null;
        public const int BUFFER_SIZE = 1024;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
        public bool Exception;
        public Task CurrentTask = Task.CompletedTask;
    }
}
