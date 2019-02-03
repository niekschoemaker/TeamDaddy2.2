using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Google.Protobuf;
using unwdmi.Protobuf;

namespace unwdmi.Parser
{
    internal class DataSender
    {
        private Controller _controller;

        private int _retryCount = 0;
        private readonly byte[] buffer = new byte[320000];

        public DataSender(Controller controller)
        {
            _controller = controller;
        }

        public void SendData(IPAddress ip, int port, ICollection<Measurement> measurements)
        {
            using (var client = new TcpClient())
            {
                Connect:
                try
                {
                    client.Connect(ip, port);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _retryCount++;
                    if (_retryCount > 5)
                    {
                        Console.WriteLine("Failed to connect 5 times, cancelling data sending.");
                        return;
                    }
                    goto Connect;
                }

                _retryCount = 0;
                using (var stream = client.GetStream())
                using (var sslStream = new SslStream(stream, true, ValidateServerCertificate, null))
                {
                    try
                    {
                        sslStream.AuthenticateAsClient("unwdmi.Parser");
                    }
                    catch (AuthenticationException e)
                    {
                        Console.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                            Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        Console.WriteLine("Authentication failed - closing the connection.");
                        client.Close();
                        return;
                    }

                    foreach (var measurement in measurements) measurement.WriteDelimitedTo(sslStream);

                    //stream.Write(buffer, 0, (int) byteStream.Position);
                }
            }
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        // Copied from Microsoft docs.
        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}