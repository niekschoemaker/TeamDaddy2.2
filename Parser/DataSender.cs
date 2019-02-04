using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using unwdmi.Protobuf;

namespace unwdmi.Parser
{
    internal class DataSender
    {
        private readonly byte[] buffer = new byte[320000];
        private Controller _controller;

        private int _retryCount;

        public DataSender(Controller controller)
        {
            _controller = controller;
        }

        public void SendData(IPAddress ip, int port, ICollection<Measurement> measurements)
        {
            var ipEndPoint = new IPEndPoint(ip, port);
            using (var client = new TcpClient())
            {
                Connect:
                try
                {
                    client.Connect(ipEndPoint);
                }
                catch (Exception e)
                {
                    _retryCount++;
                    if (_retryCount > 5)
                    {
                        Console.WriteLine("Failed to connect 5 times, cancelling data sending.");
                        _retryCount = 0;
                        return;
                    }

                    Console.WriteLine("Something went wrong while trying to connect, retrying.");
                    Debug.WriteLine(e);
                    goto Connect;
                }

                _retryCount = 0;

                var stream = client.GetStream();
                //using (var sslStream = new SslStream(stream, false, ValidateServerCertificate, null))
                try
                {
                    //sslStream.AuthenticateAsClient("unwdmi.Parser");
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

                var byteStream = new BufferedStream(stream, 4096);
                foreach (var measurement in measurements) measurement.WriteDelimitedTo(byteStream);

                byteStream.Flush();
                //stream.Write(buffer, 0, (int) byteStream.Position);
                client.Client.Shutdown(SocketShutdown.Both);
                client.Client.Disconnect(false);
                byteStream.Dispose();
                stream.Dispose();
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