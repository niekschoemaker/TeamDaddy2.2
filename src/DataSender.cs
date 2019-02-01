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

        public async void SendData(IPAddress ip, int port, ICollection<Measurement> measurements)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ip, 25565);
                Console.WriteLine(client.Connected);
                using (var stream = client.GetStream())
                {
                    foreach (var measurement in measurements)
                    {
                        measurement.WriteDelimitedTo(stream);
                    }
                }
                client.Close();
            }
        }
    }
}
