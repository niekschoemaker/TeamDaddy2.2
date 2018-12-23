#define CONCAT
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    Console.WriteLine($"Program has been running for: {TimeSinceStartup:dd\\.hh\\:mm\\:ss}.");
                }

                if (consoleLine == "exit")
                {
                    Environment.Exit(1);
                }
            }
        }

        public DateTime TimeStarted;
        public TimeSpan TimeSinceStartup => (DateTime.UtcNow - TimeStarted);

        // Trackers to keep track of the current state of the program.
        /// <summary> Active Threads (either queued or currently processing xml data) </summary>
        public int ActiveParsers = 0;
        /// <summary> Sockets currently open (# of established connections with WeatherStations) </summary>
        public int OpenSockets = 0;

        public StringBuilder SqlStringBuilder = new StringBuilder("INSERT INTO measurements (StationNumber, DateTime, Temperature, Dewpoint, StationPressure, SeaLevelPressure, Visibility, WindSpeed, Precipitation, Snowfall, Events, CloudCover, WindDirection)\nVALUES");
        public int SqlQueueCount = 0;

        public Dictionary<uint, WeatherStation> WeatherStations = new Dictionary<uint, WeatherStation>();
        public Listener Listener;
        public SqlHandler SqlHandler;
        public Parser Parser;
        public static Controller Instance;
    }

    public class WeatherStation
    {
        #region Fields

        private static readonly Controller Controller = Controller.Instance;
        public uint StationNumber;
        public string Name;
        public string Country;
        public double Latitude;
        public double Longitude;
        public double Elevation;

        public float TemperatureTotal;
        public float DewpointTotal;
        public float StationPressureTotal;
        public float SeaLevelPressureTotal;
        public float VisibilityTotal;
        public float WindSpeedTotal;
        public double PrecipitationTotal;
        public float SnowfallTotal;
        public float CloudCoverTotal;
        public ushort WindDirectionTotal;
        #endregion Fields

        public static Random Rnd = new Random();
        // Since a little delay of 30 seconds in the database shouldn't really matter choose to split the queues
        // Makes processing easier, just put elements in sqlQueue if count is bigger than 30.
        public Queue<MeasurementData> MeasurementDatas = new Queue<MeasurementData>(30);

        public WeatherStation(uint stationNumber, string name, string country, double latitude, double longitude, double elevation)
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

            // Tested both string.Format and concat, string.Format seems quickest, no difference on PC (CPU) but difference is massive on PI, probably due to memory usage.
            // There's also sb.AppendFormat but this way you can create the string, then wait for the lock, instead of waiting for the lock and make the string while in the lock.
#if CONCAT
            var str = "(" + measurement.StationNumber + ", '" + measurement.DateTime + "', " + measurement.Temperature + ", " + measurement.Dewpoint + ", " + measurement.StationPressure + ", " + measurement.SeaLevelPressure + ", " + measurement.Visibility + ", " + measurement.WindSpeed + ", " + measurement.Precipitation + ", " + measurement.Snowfall + ", " + measurement.Events + ", " + measurement.CloudCover + ", " + measurement.WindDirection + "),\n";
#else
            var str = string.Format(CultureInfo.InvariantCulture,
                "({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}),\n", measurement.StationNumber,
                measurement.DateTime, measurement.Temperature, measurement.Dewpoint, measurement.StationPressure,
                measurement.SeaLevelPressure, measurement.Visibility,
                measurement.WindSpeed, measurement.Precipitation, measurement.Snowfall, measurement.Events,
                measurement.CloudCover, measurement.WindDirection);
#endif

            lock (Controller.SqlStringBuilder)
            {
                Controller.SqlStringBuilder.Append(str);
            }

            Interlocked.Increment(ref Controller.SqlQueueCount);


            // Get a random number and recalculate average if number "hits".
            // Removes inaccuracy from averages and number can be tweaked to tune performance hit.
            //if (Rnd.Next(240) == 1 && MeasurementDatas.Count > 28)
                //recalculateAverages();
        }

        public MeasurementData Dequeue()
        {
            return MeasurementDatas.Dequeue();
        }

        private void AddTotals(MeasurementData measurement)
        {
            TemperatureTotal += measurement.Temperature;
            DewpointTotal += measurement.Dewpoint;
            StationPressureTotal += measurement.StationPressure;
            SeaLevelPressureTotal += measurement.SeaLevelPressure;
            VisibilityTotal += measurement.Visibility;
            WindSpeedTotal += measurement.WindSpeed;
            PrecipitationTotal += measurement.Precipitation;
            SnowfallTotal += measurement.Snowfall;
            CloudCoverTotal += measurement.CloudCover;
            WindDirectionTotal += measurement.WindDirection;
        }

        private void SubtractTotals(MeasurementData measurement)
        {
            TemperatureTotal -= measurement.Temperature;
            DewpointTotal -= measurement.Dewpoint;
            StationPressureTotal -= measurement.StationPressure;
            SeaLevelPressureTotal -= measurement.SeaLevelPressure;
            VisibilityTotal -= measurement.Visibility;
            WindSpeedTotal -= measurement.WindSpeed;
            PrecipitationTotal -= measurement.Precipitation;
            SnowfallTotal -= measurement.Snowfall;
            CloudCoverTotal -= measurement.CloudCover;
            WindDirectionTotal -= measurement.WindDirection;
        }

        public void recalculateSums()
        {
            TemperatureTotal = MeasurementDatas.Sum(p => p.Temperature);
            PrecipitationTotal = MeasurementDatas.Sum(p => p.Precipitation);
            StationPressureTotal = MeasurementDatas.Sum(p => p.StationPressure);
            SeaLevelPressureTotal = MeasurementDatas.Sum(p => p.SeaLevelPressure);
            VisibilityTotal = MeasurementDatas.Sum(p => p.Visibility);
            WindSpeedTotal = MeasurementDatas.Sum(p => p.WindSpeed);
            PrecipitationTotal = MeasurementDatas.Sum(p => p.Precipitation);
            SnowfallTotal = MeasurementDatas.Sum(p => p.Snowfall);
            CloudCoverTotal = MeasurementDatas.Sum(p => p.CloudCover);
            WindDirectionTotal = (ushort)MeasurementDatas.Sum(p => p.WindDirection);
        }
    }
}
