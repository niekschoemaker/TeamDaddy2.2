//#define CONCAT

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Google.Protobuf;
using unwdmi.Protobuf;

namespace unwdmi.Parser
{
    internal class Controller
    {
        private const string PathWeatherstations = "WeatherStations.dat";
        private const string PathCountries = "Countries.dat";
        public const int Port = 25566;
        public static Controller Instance;

        // Trackers to keep track of the current state of the program.
        /// <summary> Active Threads (either queued or currently processing xml data) </summary>
        public int ActiveParsers = 0;

        public int ActiveReceivers = 0;

        public Dictionary<string, Country> Countries = new Dictionary<string, Country>();
        public DataSender DataSender;
        public string Hostname = "127.0.0.1";
        public IPAddress IpAddress;
        public Listener Listener;
        public ConcurrentBag<Measurement> MeasurementQueue = new ConcurrentBag<Measurement>();

        /// <summary> Sockets currently open (# of established connections with WeatherStations) </summary>
        public int OpenSockets = 0;

        public Parser Parser;

        public StringBuilder SqlStringBuilder =
            new StringBuilder(
                "INSERT INTO measurements (StationNumber, DateTime, Temperature, Dewpoint, WindSpeed, CloudCover)\nVALUES");

        public DateTime TimeStarted;
        public Dictionary<uint, WeatherStation> WeatherStations = new Dictionary<uint, WeatherStation>();

        public Controller(string[] args)
        {
            Instance = this;
            TimeStarted = DateTime.UtcNow;
            ThreadPool.SetMinThreads(1, 0);
            ThreadPool.SetMaxThreads(20, 10);
            // Make sure you get exceptions in English. Can't quite Google something if it's Dutch.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            if (!IPAddress.TryParse(Hostname, out IpAddress))
            {
                var ipAddresses = Dns.GetHostAddresses(Hostname);
                if (ipAddresses.Length > 0) IpAddress = ipAddresses[0];
            }

            Console.WriteLine(IpAddress);

            Listener = new Listener(this);
            Parser = new Parser(this);
            DataSender = new DataSender(this);

            LoadCountries();
            LoadWeatherStations();

            Listener.StartListening();

            //TODO: Add some actual data handling, without ReadLine program just closes since nothing runs on main thread whatsoever.
            while (true)
            {
                var consoleLine = Console.ReadLine()?.Split(' ');
                if (consoleLine != null && consoleLine.Length > 0)
                    switch (consoleLine[0])
                    {
                        case "s":
                        case "stats":
                            Console.WriteLine($"Program has been running for: {TimeSinceStartup:dd\\.hh\\:mm\\:ss}.");
                            break;

                        case "ignore":
                            Console.WriteLine("Please provide a country name...");
                            var countryString = Console.ReadLine()?.ToUpper() ?? "";
                            Country country;
                            if (!Countries.TryGetValue(countryString, out country))
                            {
                                var countryArray = Countries.Where(p => p.Key.Contains(countryString))
                                    .Select(p => p.Value).ToArray();
                                if (countryArray == null || countryArray.Length == 0)
                                {
                                    Console.WriteLine("Country not found please specify an existing country.");
                                    break;
                                }

                                if (countryArray.Length > 1)
                                {
                                    Console.WriteLine(
                                        "Found multiple countries with given string please enter the number of the one you want to select:");
                                    var count = 0;
                                    foreach (var a in countryArray)
                                    {
                                        Console.WriteLine(count + ": " + a.Country_);
                                        count++;
                                    }

                                    Console.WriteLine(
                                        "Please specifiy a number to select the country you want to change...");

                                    var index = 0;
                                    if (!int.TryParse(Console.ReadLine(), out index) || index > countryArray.Length - 1)
                                    {
                                        Console.WriteLine("Invalid format. Please provide a valid number.");
                                        break;
                                    }

                                    country = countryArray[index];
                                }
                            }

                            Console.WriteLine(
                                "Please provide if you want to ignore the specified country with true/false");

                            bool ignore;
                            if (bool.TryParse(Console.ReadLine(), out ignore))
                            {
                                country.Ignore = ignore;
                                SaveCountryData();
                                UpdateWeatherStations();
                                Console.WriteLine("Datahandling will " + (country.Ignore ? "now" : "no longer") +
                                                  " ignore " + country.Country_);
                            }
                            else
                            {
                                Console.WriteLine("Invalid syntax, please only input true or false.");
                            }

                            break;

                        case "host":
                            if (consoleLine.Length == 2)
                            {
                                if (IPAddress.TryParse(consoleLine[1], out _))
                                {
                                    Hostname = consoleLine[1];
                                    Console.WriteLine($"Set IPAdress to {Hostname}");
                                }
                                else
                                {
                                    var ipAddresses = Dns.GetHostAddresses(consoleLine[1]);
                                    if (ipAddresses.Length > 0) Hostname = ipAddresses[0].ToString();
                                }
                            }

                            break;

                        case "exit":
                            Environment.Exit(1);
                            break;

                        default:
                            Console.WriteLine("Available commands are:\n\n" +
                                              "exit - exit the program.\n" +
                                              "ignore- add or remove a country from the ignored countries list.\n" +
                                              "stats - display the stats of the program.");
                            break;
                    }
            }
        }

        public TimeSpan TimeSinceStartup => DateTime.UtcNow - TimeStarted;

        private static void Main(string[] args)
        {
            //If debug is enabled add a delay before starting, just to give the debugger some time to start up.
#if DEBUG
            Thread.Sleep(2500);
#endif
            if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");

            // Make instance of self so we can send the Controller to the other objects.
            var controller = new Controller(args);
        }

        public void LoadCountries()
        {
            if (!File.Exists(PathCountries)) File.Create(PathCountries).Dispose();

            try
            {
                using (var fs = File.OpenRead(PathCountries))
                {
                    while (true)
                        try
                        {
                            var country = Country.Parser.ParseDelimitedFrom(fs);
                            Countries.Add(country.Country_, country);
                        }
                        catch
                        {
                            break;
                        }
                }

                SaveCountryData();
                Console.WriteLine("Succesfully loaded Country data.");
            }
            catch
#if DEBUG
                (Exception e)
#endif
            {
#if DEBUG
                Console.WriteLine(e);
#else
                Console.WriteLine("Something went wrong while loading Country data.");
#endif
            }
        }

        public void SaveCountryData()
        {
            if (File.Exists(PathCountries)) File.Delete(PathCountries);

            using (var fs = File.OpenWrite(PathCountries))
            {
                foreach (var country in Countries.Values) country.WriteTo(fs);
            }
        }

        public void LoadWeatherStations()
        {
            Console.WriteLine("Loading WeatherStation Data...\n");

            if (!File.Exists(PathWeatherstations))
            {
                Console.WriteLine($"{PathWeatherstations} file is missing, trying to update it from sql database...\n");

                if (!SaveWeatherStations())
                {
                    File.Create(PathWeatherstations).Dispose();

                    Console.WriteLine(
                        "Failed to get data from Database, generated empty (default) WeatherStation data.\n");
                }
                else
                {
                    Console.WriteLine("Succesfully fetched WeatherStation Data.");
                }
            }

            try
            {
                using (var fs = File.OpenRead(PathWeatherstations))
                {
                    while (true)
                        try
                        {
                            var weatherStationData = Protobuf.WeatherStation.Parser.ParseDelimitedFrom(fs);
                            if (weatherStationData == null) break;
                            var weatherStation = new WeatherStation(weatherStationData.StationNumber,
                                weatherStationData.Name, weatherStationData.Country,
                                weatherStationData.Latitude, weatherStationData.Longitude, weatherStationData.Elevation,
                                false);
                            weatherStation.IgnoreStation =
                                weatherStation.Latitude < 36 || weatherStation.Latitude > 72 ||
                                weatherStation.Longitude < -13 || weatherStation.Longitude > 41;

                            WeatherStations.Add(weatherStationData.StationNumber, weatherStation);
                        }
                        catch
                        {
                            break;
                        }
                }

                Console.WriteLine("Succesfully loaded WeatherStation data.");
            }
            catch
#if DEBUG
                (Exception e)
#endif
            {
#if DEBUG
                Console.WriteLine(e);
#else
                Console.WriteLine("Something went wrong while loading WeatherStation data.");
#endif
            }
        }

        public bool SaveWeatherStations()
        {
            if (!File.Exists(PathWeatherstations)) File.Create(PathWeatherstations);

            using (var fs = File.Open(PathWeatherstations, FileMode.Truncate, FileAccess.Write, FileShare.None))
            {
                foreach (var weatherStation in WeatherStations.Values)
                    new Protobuf.WeatherStation
                    {
                        StationNumber = weatherStation.StationNumber,
                        Name = weatherStation.Name,
                        Country = weatherStation.Country,
                        Latitude = weatherStation.Latitude,
                        Longitude = weatherStation.Longitude,
                        Elevation = weatherStation.Elevation
                    }.WriteDelimitedTo(fs);
            }

            return true;
        }

        public void UpdateWeatherStations()
        {
            foreach (var weatherStation in WeatherStations)
            {
                Country countryData;
                if (!Countries.TryGetValue(weatherStation.Value.Country, out countryData))
                {
                    countryData = new Country
                    {
                        Country_ = weatherStation.Value.Country,
                        Ignore = weatherStation.Value.IgnoreStation
                    };
                    Countries.Add(weatherStation.Value.Country, countryData);
                }
            }
        }

        /// <summary>
        ///     Hook called when all parsers are finished. DO NOT CALL THIS METHOD!
        /// </summary>
        // Add things which have to be executed right after all parsers are finished here.
        public void OnParsersFinished(List<Measurement> measurements)
        {
            DataSender.SendData(IpAddress, Port, measurements);
        }
    }

    public class WeatherStation
    {
        public static Random Rnd = new Random();
        public Queue<Measurement> MeasurementDatas = new Queue<Measurement>(30);

        public WeatherStation(uint stationNumber, string name, string country, double latitude, double longitude,
            double elevation, bool ignoreStation)
        {
            StationNumber = stationNumber;
            Name = name;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
            Elevation = elevation;
            IgnoreStation = ignoreStation;
        }

        public void Enqueue(Measurement measurement)
        {
            // Check if count equals 30, and if so move element to the SQL queue
            while (MeasurementDatas.Count >= 30)
            {
                var dequeueMeasurement = MeasurementDatas.Dequeue();
                SubtractTotals(dequeueMeasurement);
            }

            AddTotals(measurement);
            MeasurementDatas.Enqueue(measurement);

            Controller.MeasurementQueue.Add(measurement);

            // Get a random number and recalculate average if number "hits".
            // Removes inaccuracy from averages and number can be tweaked to tune performance hit.
            //if (Rnd.Next(240) == 1 && MeasurementDatas.Count > 28)
            //recalculateAverages();
        }

        public Measurement Dequeue()
        {
            return MeasurementDatas.Dequeue();
        }

        private void AddTotals(Measurement measurement)
        {
            WindSpeedTotal += measurement.WindSpeed;
            CloudCoverTotal += measurement.CloudCover;
            HumidityTotal += measurement.Humidity;
        }

        private void SubtractTotals(Measurement measurement)
        {
            WindSpeedTotal -= measurement.WindSpeed;
            CloudCoverTotal -= measurement.CloudCover;
            HumidityTotal -= measurement.Humidity;
        }

        public void recalculateSums()
        {
            WindSpeedTotal = MeasurementDatas.Sum(p => p.WindSpeed);
            CloudCoverTotal = MeasurementDatas.Sum(p => p.CloudCover);
            HumidityTotal = MeasurementDatas.Sum(p => p.Humidity);
        }

        #region Fields

        private static readonly Controller Controller = Controller.Instance;
        public uint StationNumber;
        public string Name;
        public string Country;
        public double Latitude;
        public double Longitude;
        public double Elevation;
        public bool IgnoreStation;

        public double HumidityTotal;
        public float WindSpeedTotal;
        public float CloudCoverTotal;

        #endregion Fields
    }
}