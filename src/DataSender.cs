using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using unwdmi.Protobuf;

namespace unwdmi.Parser
{
    class DataSender
    {
        private Controller _controller;

        public DataSender(Controller controller)
        {
            _controller = controller;
        }

        byte[] buffer = new byte[320000];

        public void SendData(IPAddress ip, int port, ICollection<Measurement> measurements)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect(ipEndPoint);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine(client.Connected);
                using (var stream = client.GetStream())
                {
                    MemoryStream byteStream = new MemoryStream(buffer); 
                    foreach (var measurement in measurements)
                    {
                        measurement.WriteDelimitedTo(byteStream);
                    }
                    stream.Write(buffer, 0, (int)byteStream.Position);
                    byteStream.Dispose();
                }
            }
        }
    }
}
