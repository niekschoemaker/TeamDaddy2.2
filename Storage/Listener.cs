using System;
using System.Collections.Concurrent;
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
        public static ConcurrentDictionary<uint, WeatherStation> weatherStations = new ConcurrentDictionary<uint, WeatherStation>();
        public int Minute = DateTime.UtcNow.Minute;
        private const int minecraft = 25565;
        public Queue<List<KeyValuePair<uint, float>>> HumidityTopTen30 = new Queue<List<KeyValuePair<uint, float>>>(30);
        private List<KeyValuePair<uint, float>> HumdityTopTen = new List<KeyValuePair<uint, float>>();

        public void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("Accepted TcpClient");
            var so = (StateObject) ar.AsyncState;
            var server = so.server;
            server.BeginAcceptTcpClient(new AsyncCallback(ReceiveCallback), so);

            using (TcpClient client = server.AcceptTcpClient())
            using (NetworkStream stream = client.GetStream())
            {
                while (client.Connected)
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
                            if (!weatherStations.TryGetValue(measurement.StationID, out var weatherStation))
                            {
                                weatherStation = new WeatherStation
                                {
                                    StationID = measurement.StationID
                                };
                                weatherStations.TryAdd(weatherStation.StationID, weatherStation);
                            }
                            weatherStation.Measurements.Add(measurement);
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
                var a = Math.Floor((float)(Minute / 30));
                Task.Run(() => _controller.Save());
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

    public class WeatherStation
    {
        public uint StationID;
        public List<Measurement> Measurements = new List<Measurement>(60);
        public float HumidityAverage;
        public float WindSpeedAverage;
        public float CloudCoverAverage;
    }
}
