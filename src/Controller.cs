using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

namespace unwdmi.Parser
{
    class Controller
    {
        static void Main(string[] args)
        {
            //If debug is enabled add a delay before starting, just to give the debugger some time to start up.
#if DEBUG
            Thread.Sleep(2500);
#endif
            // Make instance of self so we can send the Controller to the other objects.
            Controller controller = new Controller();
        }

        public Controller()
        {
            Instance = this;
            TimeStarted = DateTime.UtcNow;
            // Make sure you get exceptions in English. Can't quite Google something if it's Dutch.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            Listener = new Listener(this);
            SqlHandler = new SqlHandler(this);
            Parser = new Parser(this);

            SqlHandler.AddWeatherStations();

            Listener.StartListening();
            
            // Loop to keep checking the Sql Data.
            Task.Run(() =>
            {
                SqlHandler.CheckSqlQueue();
            });

            //TODO: Add some actual data handling, without ReadLine program just closes since nothing runs on main thread whatsoever.
            while (true)
            {
                var consoleLine = Console.ReadLine();
                if (consoleLine == "s")
                {
                    Console.WriteLine($"{DataAdded} measurements have been added to the database in {TimeSinceStartup:dd\\.hh\\:mm\\:ss}.");
                }

                if (consoleLine == "r")
                {
                    DataAdded = 0;
                }

                if (consoleLine == "exit")
                {
                    Environment.Exit(1);
                }
            }
        }

        public int DataAdded = 0;
        public DateTime TimeStarted;
        public TimeSpan TimeSinceStartup => (DateTime.UtcNow - TimeStarted);

        // Trackers to keep track of the current state of the program.
        /// <summary> Active Threads (either queued or currently processing xml data) </summary>
        public int ActiveParsers = 0;
        /// <summary> Sockets currently open (# of established connections with WeatherStations) </summary>
        public int OpenSockets = 0;

        public StringBuilder SqlStringBuilder = new StringBuilder("INSERT INTO measurements (StationNumber, DateTime, Temperature, Dewpoint, StationPressure, SeaLevelPressure, Visibility, WindSpeed, Precipitation, Snowfall, Events, CloudCover, WindDirection)\nVALUES");
        public int SqlQueueCount = 0;
        public List<MeasurementData> SqlQueue = new List<MeasurementData>();
        public ConcurrentDictionary<string, WeatherStation> WeatherStations = new ConcurrentDictionary<string, WeatherStation>();
        public Listener Listener;
        public SqlHandler SqlHandler;
        public Parser Parser;
        public Task SqlTask = Task.CompletedTask;
        public static Controller Instance;
    }

    public class WeatherStation
    {
        #region Fields

        private static readonly Controller Controller = Controller.Instance;
        public string StationNumber;
        public string Name;
        public string Country;
        public double Latitude;
        public double Longitude;
        public double Elevation;

        public float TemperatureAvg;
        public float DewpointAvg;
        public float StationPressureAvg;
        public float SeaLevelPressureAvg;
        public float VisibilityAvg;
        public float WindSpeedAvg;
        public double PrecipitationAvg;
        public float SnowfallAvg;
        public float CloudCoverAvg;
        public int WindDirectionAvg;
        #endregion Fields

        public static Random Rnd = new Random();
        // Since a little delay of 30 seconds in the database shouldn't really matter choose to split the queues
        // Makes processing easier, just put elements in sqlQueue if count is bigger than 30.
        public Queue<MeasurementData> MeasurementDatas = new Queue<MeasurementData>(30);

        public WeatherStation(string stationNumber)
        {
            StationNumber = stationNumber;
        }

        public WeatherStation(string stationNumber, string name, string country, double latitude, double longitude, double elevation)
        {
            StationNumber = stationNumber;
            Name = name;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
            Elevation = elevation;
        }

        public void Enqueue(MeasurementData measurement)
        {

            // Check if count equals 30, and if so move element to the SQL queue
            while (MeasurementDatas.Count >= 30)
            {
                var dequeueMeasurement = MeasurementDatas.Dequeue();
                SubtractTotals(dequeueMeasurement);
            }

            AddTotals(measurement);
            MeasurementDatas.Enqueue(measurement);
            
            lock (Controller.SqlQueue)
            {
                Controller.SqlQueue.Add(measurement);
            }

            lock (Controller.SqlStringBuilder)
            {
                Controller.SqlStringBuilder.AppendFormat(
                    "({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}),\n", measurement.StationNumber,
                    measurement.DateTime, measurement.Temperature, measurement.Dewpoint, measurement.StationPressure, measurement.SeaLevelPressure, measurement.Visibility,
                    measurement.WindSpeed, measurement.Precipitation, measurement.Snowfall, measurement.Events, measurement.CloudCover, measurement.WindDirection);
            }

            Interlocked.Increment(ref Controller.SqlQueueCount);


            // Get a random number and recalculate average if number "hits".
            // Removes inaccuracy from averages and number can be tweaked to tune performance hit.
            if (Rnd.Next(240) == 1 && MeasurementDatas.Count > 28)
                recalculateAverages();
        }

        public MeasurementData Dequeue()
        {
            return MeasurementDatas.Dequeue();
        }

        private void AddTotals(MeasurementData measurement)
        {
            TemperatureAvg += measurement.Temperature / 30;
            DewpointAvg += measurement.Dewpoint / 30;
            StationPressureAvg += measurement.StationPressure / 30;
            SeaLevelPressureAvg += measurement.SeaLevelPressure / 30;
            VisibilityAvg += measurement.Visibility / 30;
            WindSpeedAvg += measurement.WindSpeed / 30;
            PrecipitationAvg += measurement.Precipitation / 30;
            SnowfallAvg += measurement.Snowfall / 30;
            CloudCoverAvg += measurement.CloudCover / 30;
            WindDirectionAvg += measurement.WindDirection / 30;
        }

        private void SubtractTotals(MeasurementData measurement)
        {
            TemperatureAvg -= measurement.Temperature / 30;
            DewpointAvg -= measurement.Dewpoint / 30;
            StationPressureAvg -= measurement.StationPressure / 30;
            SeaLevelPressureAvg -= measurement.SeaLevelPressure / 30;
            VisibilityAvg -= measurement.Visibility / 30;
            WindSpeedAvg -= measurement.WindSpeed / 30;
            PrecipitationAvg -= measurement.Precipitation / 30;
            SnowfallAvg -= measurement.Snowfall / 30;
            CloudCoverAvg -= measurement.CloudCover / 30;
            WindDirectionAvg -= measurement.WindDirection / 30;
        }

        public void recalculateAverages()
        {
            TemperatureAvg = MeasurementDatas.Average(p => p.Temperature);
            PrecipitationAvg = MeasurementDatas.Average(p => p.Precipitation);
            StationPressureAvg = MeasurementDatas.Average(p => p.StationPressure);
            SeaLevelPressureAvg = MeasurementDatas.Average(p => p.SeaLevelPressure);
            VisibilityAvg = MeasurementDatas.Average(p => p.Visibility);
            WindSpeedAvg = MeasurementDatas.Average(p => p.WindSpeed);
            PrecipitationAvg = MeasurementDatas.Average(p => p.Precipitation);
            SnowfallAvg = MeasurementDatas.Average(p => p.Snowfall);
            CloudCoverAvg = MeasurementDatas.Average(p => p.CloudCover);
            WindDirectionAvg = (int)MeasurementDatas.Average(p => p.WindDirection);
        }
    }
}
