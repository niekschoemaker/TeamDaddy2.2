using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public static X509Certificate2 serverCertificate = null;

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

            serverCertificate = new X509Certificate2("client.p12", "DaddyCool");



            Task.Run(() => ListenerParser.StartListening());

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            if (!File.Exists("./Data/Daddy.pb"))
            {
                File.Create("./Data/Daddy.pb");
            }

        }

        public void Save(ConcurrentDictionary<uint, WeatherStation> weatherStations)
        {
            var count = DateTime.UtcNow;

            if (!File.Exists($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb"))
            {
                File.Create($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb").Dispose();
            }

            List<KeyValuePair<uint, float>> humidities = new List<KeyValuePair<uint, float>>();
            using (FileStream output = File.Open($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb", FileMode.Append))
            {
                Console.WriteLine(weatherStations.Count);
                foreach (var weatherStation in weatherStations)
                {
                    var mCount = weatherStation.Value.Count;
                    var humidity = weatherStation.Value.HumidityTotal / mCount;
                    new Measurement
                    {
                        CloudCover = weatherStation.Value.CloudCoverTotal / mCount,
                        Humidity = humidity,
                        StationID = weatherStation.Key,
                        WindSpeed = weatherStation.Value.WindSpeedTotal / mCount
                    }.WriteDelimitedTo(output);
                    humidities.Add(new KeyValuePair<uint, float>(weatherStation.Key, (float)humidity));
                }
            }

            if (ListenerParser.HumidityTopTen30.Count == 30)
            {
                ListenerParser.HumidityTopTen30.Dequeue();
            }
            ListenerParser.HumidityTopTen30.Enqueue(humidities.OrderByDescending(p => p.Value).Take(10).ToList());


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

            var humidityTopTen = humidityKeyValuePairs.OrderByDescending(p => p.Value).Take(10).ToArray();
            var topTen = new Protobuf.TopTen();
            for(int i = 0; i < humidityTopTen.Count(); i++)
            {
                // Was trying to figure out reflection, this should work pretty nicely, is a nice version of "Humidity1 = value", "Humidity2 = value", etc
                topTen.GetType().GetProperty("Humidity" + (i + 1)).SetValue(topTen, humidityTopTen[i].Value);

                topTen.GetType().GetProperty("WeatherStation" + (i + 1)).SetValue(topTen, humidityTopTen[i].Key);
            }

            // Save latest top 10 to disk.
            using(var file = File.OpenWrite("TopTen.pb"))
                topTen.WriteTo(file);
        }
    }
}
