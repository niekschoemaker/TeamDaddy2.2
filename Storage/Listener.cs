using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
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
        //Is a concurrent Dictionary since it is used in an async function, shouldn't be necessary, but just in case since it doens't really cost extra performance.
        public static ConcurrentDictionary<uint, WeatherStation> weatherStations = new ConcurrentDictionary<uint, WeatherStation>();
        public int Minute = DateTime.UtcNow.Minute;
        private const int minecraft = 25566;
        /// <summary>
        /// Contains HumidityTopTens from the last 30 minutes (stores top ten of each minute)
        /// </summary>
        public Queue<List<KeyValuePair<uint, float>>> HumidityTopTen30 = new Queue<List<KeyValuePair<uint, float>>>(30);

        public void ReceiveCallback(IAsyncResult ar)
        {
            var so = (StateObject) ar.AsyncState;
            var server = so.server;
            server.BeginAcceptTcpClient(new AsyncCallback(ReceiveCallback), so);

            Console.WriteLine($"Receive call back");

            using (TcpClient client = server.AcceptTcpClient())
            using (var stream = client.GetStream())
            using (var sslStream = new SslStream(stream, true, ValidateServerCertificate, null))
            {
                _controller.Log($"Accepted a connection from {client.Client.RemoteEndPoint}", Controller.ErrorLevel.Debug);
                Console.WriteLine($"Accepted a connection from {client.Client.RemoteEndPoint}");
                try
                {
                    sslStream.AuthenticateAsServer(Controller.serverCertificate);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _controller.Log(e.Message);
                    _controller.Log(e.ToString(), Controller.ErrorLevel.Debug);
                }
                while (true)
                {
                    try
                    {
                        var measurement = Measurement.Parser.ParseDelimitedFrom(sslStream);
                        if (measurement == null)
                        {
                            break;
                        }
                        else
                        {
                            if (!weatherStations.TryGetValue(measurement.StationID, out var weatherStation))
                            {
                                weatherStation = new WeatherStation
                                {
                                    StationID = measurement.StationID
                                };
                                weatherStations.TryAdd(weatherStation.StationID, weatherStation);
                            }

                            weatherStation.WindSpeedTotal += measurement.WindSpeed;
                            weatherStation.HumidityTotal += (float)measurement.Humidity;
                            weatherStation.CloudCoverTotal += measurement.WindSpeed;
                            weatherStation.Count++;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            //Console.WriteLine(CacheMeasurements.Count() + "HACKER MAN <3");
            if (Minute != DateTime.UtcNow.Minute)
            {
                Minute = DateTime.UtcNow.Minute;
                ConcurrentDictionary<uint, WeatherStation> _weatherStations;
                lock (weatherStations)
                {
                    _weatherStations = weatherStations;
                    weatherStations = new ConcurrentDictionary<uint, WeatherStation>();
                }
                Task.Run(() => _controller.Save(_weatherStations));
            }

        }

        public void StartListening()
        {
            TcpListener server = new TcpListener(IPAddress.Any, minecraft);
            server.Start();
            var so = new StateObject()
            {
                server = server
            };
            server.BeginAcceptTcpClient(new AsyncCallback(ReceiveCallback), so);
        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    class ListenerWeb
    {

    }

    public class StateObject
    {
        public TcpListener server;
    }

    public class WeatherStation
    {
        public uint StationID;

        public int Count = 0;
        public float HumidityAverage => HumidityTotal / 30;
        public float WindSpeedAverage => WindSpeedTotal / 30;
        public float CloudCoverAverage => CloudCoverTotal / 30;

        public float HumidityTotal;
        public float WindSpeedTotal;
        public float CloudCoverTotal;
    }
}
