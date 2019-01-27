using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;
using Google.Protobuf;
using unwdmi.Protobuf;

/*
 *  Автор: Sean Visser
 *  Версия: 1.0.0
 *  Определение: Контроллер для МВС-Модел память
 */

namespace unwdmi.Storage
{

    class ListenerParser
    {
        private Controller _controller;
        public ListenerParser(Controller controller)
        {
            _controller = controller;
        }

        public List<Measurement> CacheMeasurements = new List<Measurement>();

        public void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("Accepted TcpClient");
            var so = (StateObject) ar.AsyncState;
            var server = so.server;
            server.BeginAcceptTcpClient(new AsyncCallback(ReceiveCallback), so);
            using (TcpClient client = server.AcceptTcpClient())
            using (NetworkStream stream = client.GetStream())
            {
                while (true)
                {
                    try
                    {
                        var measurement = Measurement.Parser.ParseDelimitedFrom(stream);
                        if (measurement == null)
                        {
                            break;
                        }
                        else
                        {
                            lock(CacheMeasurements)
                                CacheMeasurements.Add(measurement);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }

        }

        public void StartListening()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 25565);
            server.Start();
            var so = new StateObject()
            {
                server = server
            };
            server.BeginAcceptTcpClient(new AsyncCallback(ReceiveCallback), so);
        }
    }

    class ListenerWeb
    {

    }

    public class StateObject
    {
        public TcpListener server;
        // 4096 fits all the XML files, so the checks don't have to be done as often, saves a bit of CPU, costs a bit more ram.
        public const int BUFFER_SIZE = 4096;
        public byte[] buffer = new byte[BUFFER_SIZE];
    }
}
