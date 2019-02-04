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
using Newtonsoft.Json;

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
        private const string ConfigFile = "config.json";
        public Config _config;

        public Controller()
        {
            Log("Starting unwdmi Storage.");
            Instance = this;
            ListenerParser = new ListenerParser(this);
            //ListenerWeb = new ListenerWeb(this);

            // Multi-threading
            TimeStarted = DateTime.UtcNow;
            ThreadPool.SetMinThreads(1, 0);
            ThreadPool.SetMaxThreads(20, 10);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            InitFiles();
        }

        public void Save(ConcurrentDictionary<uint, WeatherStation> weatherStations)
        {
            var count = DateTime.UtcNow;

            if (!File.Exists($"./Data/Daddy-{count:yyyy-MM-dd-HH-mm}.pb"))
            {
                File.Create($"./Data/Daddy-{count:yyyy-MM-dd-HH-mm}.pb").Dispose();
            }

            List<KeyValuePair<uint, float>> humidities = new List<KeyValuePair<uint, float>>();
            using (FileStream output = File.Open($"./Data/Daddy-{count:yyyy-M-d-HH-m}.pb", FileMode.Append))
            {
                Console.WriteLine(weatherStations.Count);
                Log($"Saving data for {weatherStations.Count} weatherstations.");
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

        public void InitFiles()
        {
            if (!File.Exists("config.json"))
            {
                Log("Config file not found, generating a new one.");
                _config = new Config();
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(_config));
            }

            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            try
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile));
                Log("Succesfully loaded config file.", ErrorLevel.Info);
            }
            catch (Exception e)
            {
                Log("Something went wrong while loading the config file.\n" +
                                  "\tRemoving the config file should fix this issue.", ErrorLevel.Error);
                Log(e.ToString(), ErrorLevel.Debug);
            }

            bool changed = false;
            ReadFile:
            if (!File.Exists(_config.CertificateFilePath))
            {
                Log($"Certificate file not found at {_config.CertificateFilePath}", ErrorLevel.Fatal);
                Log("Please add the certificate file or give the correct name of the file:", ErrorLevel.Fatal);
                Log("Leave the following line blank to exit the program.", ErrorLevel.Fatal);
                var readLine = Console.ReadLine();
                if (string.IsNullOrEmpty(readLine))
                {
                    Environment.Exit(1);
                }
                else
                {
                    _config.CertificateFilePath = readLine;
                    changed = true;
                    goto ReadFile;
                }
            }
            serverCertificate = new X509Certificate2(_config.CertificateFilePath, "DaddyCool");
            
            SaveConfig();

            Log("Succesfully loaded certificate.");

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

        public class Config
        {
            [JsonProperty("Certificate File Path")]
            public string CertificateFilePath = "server.p12";

            [JsonProperty("Minimum log level for log file (1 = Debug, 2 = Info, 3 = Error, 4 = Fatal)")]
            public int LogLevel = (int)ErrorLevel.Error;

            [JsonProperty("Minimum log level for console (1 = Debug, 2 = Info, 3 = Error, 4 = Fatal)")]
            public int ConsoleLogLevel = (int)ErrorLevel.Info;
        }

        public enum ErrorLevel
        {
            Debug = 1,
            Info = 2,
            Error = 3,
            Fatal = 4
        }

        public void Log(string message, ErrorLevel level = ErrorLevel.Info)
        {
            switch (level)
            {
                case ErrorLevel.Debug:
                    message = "[DEBUG] " + message;
                    break;
                case ErrorLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.Red;
                    message = "[FATAL] " + message;
                    break;
                case ErrorLevel.Error:
                    message = "[ERROR] " + message;
                    break;
                case ErrorLevel.Info:
                    message = "[Info] " + message;
                    break;
            }

            message = DateTime.Now.ToString("hh:mm:ss") + " " +  message;
            if((int)level >= _config?.LogLevel)
                File.AppendAllText("./logs/" + DateTime.Today.ToString("yyyy-MM-dd") + ".txt", message + Environment.NewLine);
            if ((int)level >= _config?.ConsoleLogLevel)
                Console.WriteLine(message);

            Console.ResetColor();
        }

        public void SaveConfig()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(_config, Formatting.Indented));
        }
    }
}
