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

        private readonly byte[] buffer = new byte[320000];

        public DataSender(Controller controller)
        {
            _controller = controller;
        }

        public async void SendData(IPAddress ip, int port, ICollection<Measurement> measurements)
        {
            var ipEndPoint = new IPEndPoint(ip, port);
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
                using (var sslStream = new SslStream(stream, false, ValidateServerCertificate, null))
                {
                    Task task;
                    try
                    {
                        task = sslStream.AuthenticateAsClientAsync("unwdmi.Parser");
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

                    var byteStream = new MemoryStream(buffer);
                    foreach (var measurement in measurements) measurement.WriteDelimitedTo(byteStream);

                    await task;
                    task = stream.WriteAsync(buffer, 0, (int) byteStream.Position);
                    byteStream.Dispose();
                    await task;
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
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return true;
        }
    }
}