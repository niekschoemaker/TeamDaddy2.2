using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using unwdmi.Protobuf;

/*
 *  Автор: Sean Visser
 *  Версия: 1.0.0
 *  Определение: Контроллер для МВС-Модел память
 */

namespace unwdmi.Storage
{
    internal class ListenerParser
    {
        private const int minecraft = 25566;

        //Is a concurrent Dictionary since it is used in an async function, shouldn't be necessary, but just in case since it doens't really cost extra performance.
        public static ConcurrentDictionary<uint, WeatherStation> weatherStations =
            new ConcurrentDictionary<uint, WeatherStation>();

        private readonly Controller _controller;

        public List<Measurement> CacheMeasurements = new List<Measurement>();

        /// <summary>
        ///     Contains HumidityTopTens from the last 30 minutes (stores top ten of each minute)
        /// </summary>
        public Queue<List<KeyValuePair<uint, float>>> HumidityTopTen30 = new Queue<List<KeyValuePair<uint, float>>>(30);

        public int Minute = DateTime.UtcNow.Minute;

        public ListenerParser(Controller controller)
        {
            _controller = controller;
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            var so = (StateObject) ar.AsyncState;
            var server = so.server;
            server.BeginAcceptTcpClient(ReceiveCallback, so);

            using (var client = server.AcceptTcpClient())
            using (var stream = client.GetStream())
                //using (var sslStream = new SslStream(stream, false, validate))
            {
                _controller.Log($"Accepted a connection from {client.Client.RemoteEndPoint}",
                    Controller.ErrorLevel.Debug);
                try
                {
                    //sslStream.AuthenticateAsServer(Controller.serverCertificate);
                }
                catch (Exception e)
                {
                    _controller.Log(e.Message);
                    _controller.Log(e.ToString(), Controller.ErrorLevel.Debug);
                }

                while (true)
                    try
                    {
                        var measurement = Measurement.Parser.ParseDelimitedFrom(stream);
                        if (measurement == null) break;

                        if (!weatherStations.TryGetValue(measurement.StationID, out var weatherStation))
                        {
                            weatherStation = new WeatherStation
                            {
                                StationID = measurement.StationID
                            };
                            weatherStations.TryAdd(weatherStation.StationID, weatherStation);
                        }

                        weatherStation.WindSpeedTotal += measurement.WindSpeed;
                        weatherStation.HumidityTotal += (float) measurement.Humidity;
                        weatherStation.CloudCoverTotal += measurement.WindSpeed;
                        weatherStation.Count++;
                    }
                    catch
                    {
                        break;
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
            var server = new TcpListener(IPAddress.Any, minecraft);
            server.Start();
            var so = new StateObject
            {
                server = server
            };
            server.BeginAcceptTcpClient(ReceiveCallback, so);
        }
    }

    internal class ListenerWeb
    {
    }

    public class StateObject
    {
        public TcpListener server;
    }

    public class WeatherStation
    {
        public float CloudCoverTotal;

        public int Count;

        public float HumidityTotal;
        public uint StationID;
        public float WindSpeedTotal;
        public float HumidityAverage => HumidityTotal / 30;
        public float WindSpeedAverage => WindSpeedTotal / 30;
        public float CloudCoverAverage => CloudCoverTotal / 30;
    }
}