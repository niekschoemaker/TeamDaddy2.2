using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using unwdmi.Protobuf;

/*
 *  Автор: Sean Visser
 *  Версия: 1.0.0
 *  Определение: Контроллер для МВС-Модел память
 */

namespace unwdmi.Storage
{
    class Controller
    {
        static void Main(string[] args)
        {
            Controller controller = new Controller();

            while (true)
            {
                var _string = Console.ReadLine();
                if (_string.ToLower() == "suka")
                {
                    Console.WriteLine("BLYAT");
                }
            }


        }
        
        public ListenerParser ListenerParser;
        public ListenerWeb ListenerWeb;
        public static Controller Instance;

        public DateTime TimeStarted;
        public TimeSpan TimeSinceStartup => (DateTime.UtcNow - TimeStarted);

        public Controller()
        {
            Instance = this;
            ListenerParser = new ListenerParser(this);
            //ListenerWeb = new ListenerWeb(this);

            // Multi-threading
            TimeStarted = DateTime.UtcNow;
            ThreadPool.SetMinThreads(1, 0);
            ThreadPool.SetMaxThreads(20, 10);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            Task.Run(() => ListenerParser.StartListening());

            if(!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            if(!File.Exists("./Data/Daddy.pb"))
            {
                File.Create("./Data/Daddy.pb");
            }

        }

        public void Save()
        {
                var count = DateTime.UtcNow;
                List<Measurement> measurements;
            ConcurrentDictionary<uint, WeatherStation> weatherStations;
            lock (ListenerParser.weatherStations)
            {
                weatherStations = ListenerParser.weatherStations;
                ListenerParser.weatherStations = new ConcurrentDictionary<uint, WeatherStation>();
            }
                lock (ListenerParser.CacheMeasurements)
                {
                    measurements = ListenerParser.CacheMeasurements.ToList();
                    ListenerParser.CacheMeasurements.Clear();
                }

                if (!File.Exists($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb"))
                {
                    File.Create($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb").Dispose();
                }

            List<KeyValuePair<uint, float>> Humidities = new List<KeyValuePair<uint, float>>();
                using (FileStream output = File.Open($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb", FileMode.Append))
                {
                    Console.WriteLine(measurements.Count);
                    foreach (var weatherStation in weatherStations)
                    {
                        var humidity = weatherStation.Value.Measurements.Average(p => p.Humidity);
                        new Measurement
                        {
                            CloudCover = weatherStation.Value.Measurements.Average(p => p.CloudCover),
                            Humidity = humidity,
                            StationID = weatherStation.Key,
                            WindSpeed = weatherStation.Value.Measurements.Average(p => p.WindSpeed)
                        };
                        Humidities.Add(new KeyValuePair<uint, float>(weatherStation.Key, (float)humidity));
                    }
                }

            if (ListenerParser.HumidityTopTen30.Count == 30)
            {
                ListenerParser.HumidityTopTen30.Dequeue();
            }
            ListenerParser.HumidityTopTen30.Enqueue(Humidities.OrderByDescending(p => p.Value).Take(10).ToList());
            Dictionary<uint, float> humidityKeyValuePairs = new Dictionary<uint, float>();
            foreach (var a in ListenerParser.HumidityTopTen30)
            {
                foreach (var b in a)
                {
                    if (humidityKeyValuePairs.TryGetValue(b.Key, out var humidity))
                    {
                        if (humidity < b.Value)
                        {
                            humidityKeyValuePairs[b.Key] = b.Value;
                        }
                    }
                    else
                    {
                        humidityKeyValuePairs.Add(b.Key, b.Value);
                    }
                }
            }

            var humidityTopTen = humidityKeyValuePairs.OrderByDescending(p => p.Value).Take(10);
        }
    }
}
