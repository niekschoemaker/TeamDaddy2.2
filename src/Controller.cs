using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using ProjectTeamDaddy2._2;

namespace XMLReader
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
            // Make sure you get exceptions in English. Can't quite Google something if it's Dutch.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            Listener = new Listener(this);
            SqlHandler = new SqlHandler(this);
            Parser = new Parser(this);

            SqlHandler.AddWeatherStations();

            Listener.StartListening();

            //TODO: Add some actual data handling, without ReadLine program just closes since nothing runs on main thread whatsoever.
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        public ConcurrentBag<MeasurementData> SqlQueue = new ConcurrentBag<MeasurementData>();
        public ConcurrentDictionary<int, WeatherStation> WeatherStations = new ConcurrentDictionary<int, WeatherStation>();
        public Listener Listener;
        public SqlHandler SqlHandler;
        public Parser Parser;
    }

    public class WeatherStation
    {
        #region Fields
        public int StationNumber;
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
        public List<MeasurementData> SqlQueue = new List<MeasurementData>();

        public WeatherStation(int stationNumber)
        {
            StationNumber = stationNumber;
        }

        public WeatherStation(int stationNumber, string name, string country, double latitude, double longitude, double elevation)
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
                var toAdd = MeasurementDatas.Dequeue();
                SubtractTotals(toAdd);
                lock (SqlQueue)
                    SqlQueue.Add(toAdd);
                SqlDequeue();
            }

            AddTotals(measurement);
            MeasurementDatas.Enqueue(measurement);

            // Get a random number and recalculate average if number "hits".
            // Removes inaccuracy from averages and number can be tweaked to tune performance hit.
            if (Rnd.Next(17) == 1 && MeasurementDatas.Count > 28)
                recalculateAverages();
        }

        public MeasurementData Dequeue()
        {
            return MeasurementDatas.Dequeue();
        }

        /// <summary>
        /// Sends back the values which are queued to be added to the database.
        /// </summary>
        /// <returns></returns>
        public List<MeasurementData> SqlDequeue()
        {
            // Use ToList here to copy the List instead of making a reference since it is cleared right after.
            List<MeasurementData> sqlDatas;
            lock (SqlQueue)
            {
                sqlDatas = SqlQueue.ToList();
                SqlQueue.Clear();
            }

            return sqlDatas;
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
