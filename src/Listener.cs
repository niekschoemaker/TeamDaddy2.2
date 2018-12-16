using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Console = System.Console;
using Random = System.Random;

namespace XMLReader
{
    class Listener
    {
        public Listener(Controller Instance)
        {
            controller = Instance;
            WeatherStationsDictionary = controller.WeatherStations;
        }

        private Controller controller;

        private Socket _listener;

        public ConcurrentDictionary<int, WeatherStation> WeatherStationsDictionary;

        public void StartListening()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 7789);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Blocking = false;
            _listener.Bind(ip);
            _listener.Listen(100);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }


        void AcceptCallBack(IAsyncResult ar)
        {
            //Program can act weird if all 800 connections are opened at once.
            //In real-life applications this shouldn't be a problem.
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject stateObject = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), stateObject);
            _listener.BeginAccept(new AsyncCallback(AcceptCallBack), _listener);
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject) ar.AsyncState;
            Socket s = so.workSocket;

            try
            {
                int read = s.EndReceive(ar);

                if (read > 0)
                {
                    string str = Encoding.ASCII.GetString(so.buffer, 0, read);
                    so.sb.Append(str);
                    if (str.IndexOf("</WEATHERDATA>", str.Length - 20 >= 0 ? str.Length - 20 : 0,
                            StringComparison.Ordinal) > -1)
                    {
                        var strContent = so.sb.ToString();
                        Task task = so.CurrentTask;
                        so.CurrentTask = Task.Run(() =>
                        {
                            StateObject.currentlyActiveTasks++;
                            Task.WaitAll(task);
                            ParseXML(strContent, so);
                            StateObject.currentlyActiveTasks--;
                        });
                        so.sb.Clear();
                    }

                    // if stringbuilder is longer than a XML file clear it.
                    if (so.sb.Length > 4000)
                    {
                        var strContent = so.sb.ToString();
                        // Check if the stringbuilder contains a xml definition and if so substring it to start there.
                        if (strContent.Contains("<?xml"))
                        {
                            strContent = strContent.Substring(strContent.LastIndexOf("<?xml", strContent.Length - 3900, StringComparison.Ordinal));
                            so.sb.Clear();
                            so.sb.Append(strContent);
                        }
                        else
                        {
                            so.sb.Clear();
                        }
                    }

                    s.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), so);
                }
                else
                {
                    if (so.sb.Length > 1)
                    {
                        so.sb.Clear();
                    }

                    s.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void ParseXML(string XML, StateObject so)
        {
            if (!XML.StartsWith("<?x") || !XML.EndsWith("TA>\n"))
            {
                XML = XML.Substring(XML.IndexOf("<?xml", StringComparison.Ordinal),
                    XML.IndexOf("</WEATHERDATA>", StringComparison.Ordinal) + 15);
            }

            // Is inside a using Statement because XmlReader and StringReader are ignored by GC
            // Forgetting GC in such cases causes quite significant memory leaks.
            using (var reader = XmlReader.Create(new StringReader(XML)))
            {
                try
                {
                    var count = 0;
                    while (count != 10)
                    {
                        reader.ReadToFollowing("MEASUREMENT");

                        // Parse the measurement and add it to the history queue.
                        var measurement = ParseMeasurement(reader);
                        if (measurement.StationNumber == 0)
                        {
                            Console.WriteLine("Uhoh");
                        }
                        count++;

                        if (!WeatherStationsDictionary.TryGetValue(measurement.StationNumber, out var weatherStation))
                        {
                            weatherStation = new WeatherStation(measurement.StationNumber);
                            WeatherStationsDictionary.TryAdd(measurement.StationNumber, weatherStation);
                        }
                        weatherStation.Enqueue(measurement);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        MeasurementData ParseMeasurement(XmlReader reader)
        {
            MeasurementData measurement = new MeasurementData();

            var count = 0;
            var date = string.Empty;


            if (reader.ReadToFollowing("STN"))
            {
                // The code now following. Plis ignore, lot of repetitive code, since using objects for performance.
                try
                {
                    if (!int.TryParse(reader.ReadElementString(), NumberStyles.None, null,
                        out measurement.StationNumber))
                    {
                        Console.WriteLine("Skipped measurement.");
                        return null;
                    }
                    // reader.Skip skips one node (Skips to next start element in this XML file)
                    // Doesn't validate the XML so is quicker than calling .read multiple times
                    reader.Skip();

                    if (reader.Name.Equals("DATE"))
                    {
                        date = reader.ReadElementString();
                        reader.Skip();
                    }

                    if (reader.Name.Equals("TIME"))
                    {
                        measurement.dateTime = date + " " + reader.ReadElementString();
                        reader.Skip();
                    }

                    var numberStyleNegative = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                    var numberStylePositive = NumberStyles.AllowDecimalPoint;

                    var culture = NumberFormatInfo.InvariantInfo;
                    if (reader.Name.Equals("TEMP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Temperature))
                        {
                            Console.WriteLine(NumberFormatInfo.InvariantInfo.NumberDecimalSeparator);
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("DEWP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Dewpoint))
                        {
                        }

                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("STP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.StationPressure))
                        {
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("SLP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.SeaLevelPressure))
                        {
                            
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("VISIB"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.Visibility))
                        {

                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("WDSP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.WindSpeed))
                        {

                        }
                        reader.Skip();
                    }

                    if (reader.Name.Equals("PRCP"))
                    {
                        if (!double.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.Precipitation))
                        {

                        }
                        reader.Skip();
                    }

                    if (reader.Name.Equals("SNDP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Snowfall))
                        {

                        }
                        reader.Skip();
                    }

                    if (reader.Name.Equals("FRSHTT"))
                    {
                        var frshtt = reader.ReadElementString().ToCharArray();
                        
                        byte total = 0;
                        if (frshtt.Length != 0)
                        {
                            for (int i = 0; i < frshtt.Length; i++)
                            {
                                total += frshtt[i] == '0' ? (byte)0 : (byte)Math.Pow(2, 5 - i);
                            }
                        }
                        measurement.Events = total;

                        reader.Skip();
                    }

                    if (reader.Name.Equals("CLDC"))
                    {
                        float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.CloudCover);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("WNDDIR"))
                    {
                        int.TryParse(reader.ReadElementString(), NumberStyles.None, NumberFormatInfo.InvariantInfo, out measurement.WindDirection);
                        reader.Skip();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            /*
            if (count != 14)
            {
                //Simulate the correction of data, should take around 100 ms, not more I think.
                Stopwatch stopwatch = Stopwatch.StartNew();
                //Add some blyats to a string to express your frustration on missing values.
                var blyats = "";
                while (stopwatch.ElapsedMilliseconds < 100)
                {
                    // Simulate being busy
                    blyats += "ayy blyat";
                }
            }*/
            // FYI: We're gonna need to keep the time the correction
            // takes below 100 milliseconds, happy funtime.

            return measurement;
        }
    }

    public class StateObject
    {
        public static int currentlyActiveTasks = 0;
        public Socket workSocket = null;
        public const int BUFFER_SIZE = 1024;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
        public bool Exception;
        public Task CurrentTask = Task.CompletedTask;
    }

    public class MeasurementData
    {
        /// <summary> DateTime of recording </summary>
        public string dateTime;
        /// <summary> Station ID </summary>
        public int StationNumber;


        /// <summary>
        /// Temperature in degrees Celsius. Valid Values range from -9999.9 till 9999.9 with one decimal point precision.
        /// </summary>
        public float Temperature;

        /// <summary>
        /// Dewpoint in degrees Celsius. Valid values range from -9999.9 till 9999.9 with 1 decimal point precision.
        /// </summary>
        public float Dewpoint;

        /// <summary>
        /// Air pressure at the station's level in mBar. valid values range from 0.0 till 9999.9 with 1 decimal point precision.
        /// </summary>
        public float StationPressure;

        /// <summary>
        /// Air pressure at sea level in mBar. Valid values range from 0.0 till 9999.9 with 1 decimal point precision.
        /// </summary>
        public float SeaLevelPressure;

        /// <summary>
        /// Visibility in KM. Valid values range from 0.0 till 999.9, with 1 decimal point precision.
        /// </summary>
        public float Visibility;

        /// <summary>
        /// Windspeed in KM/h. Valid values range from 0.0 till 999.9, with 1 decimal point precision.
        /// </summary>
        public float WindSpeed;

        /// <summary>
        /// Precipitation in cm. Valid values range from 0.00 till 999.99, with 2 decimal points precision.
        /// </summary>
        public double Precipitation;

        /// <summary>
        /// Snowfall in cm. Valid values range from -9999.9 till 9999.9, with 1 decimal point precision.
        /// </summary>
        public float Snowfall;

        /// <summary>
        /// Flag variable containing events on this day.
        /// see enum <see cref="EventFlags"/> for possible flags.
        /// </summary>
        public byte Events;

        /// <summary>
        /// Cloud cover in percentage. Valid values range from 0.0 till 99.9 with 1 decimal point precision.
        /// </summary>
        public float CloudCover;
        /// <summary>
        /// Wind direction in degrees. Valid values range from 0 till 359. Only integers.
        /// </summary>
        public int WindDirection;

        public enum EventFlags
        {
            Tornado = 1,
            Thunder = 2,
            Hail = 4,
            Snow = 8,
            Rain = 16,
            Freezing = 32
        }
    }

    public class WeatherStation
    {
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
        /// <param name="sqlDatas">A list which is supposed to contain the measurements</param>
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

        private void recalculateAverages()
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
            WindDirectionAvg = (int) MeasurementDatas.Average(p => p.WindDirection);
        }
    }
}
