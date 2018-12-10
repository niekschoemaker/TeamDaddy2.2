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

namespace XMLReader
{
    class Listener
    {
        private Socket _listener;

        public ConcurrentDictionary<int, WeatherStation> WeatherStationsDictionary = new ConcurrentDictionary<int, WeatherStation>();

        public Listener()
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
                        Task.Run(() => { ParseXML(strContent, so); });
                        so.sb.Clear();
                    }

                    // if stringbuilder is longer than a XML file clear it.
                    if (so.sb.Length > 4000)
                    {
                        var strContent = so.sb.ToString();
                        // Check if the stringbuilder contains a xml definition and if so substring it to start there.
                        if (strContent.Contains("<?xml"))
                        {
                            strContent = strContent.Substring(strContent.IndexOf("<?xml", StringComparison.Ordinal));
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
                    measurement.StationNumber = reader.ReadElementContentAsInt();
                    // reader.Skip skips one node (Skips to next start element in this XML file)
                    // Doesn't validate the XML so is quicker than calling .read multiple times
                    reader.Skip();

                    if (reader.Name.Equals("DATE"))
                    {
                        date = reader.ReadElementContentAsString();
                        reader.Skip();
                    }

                    if (reader.Name.Equals("TIME"))
                    {
                        
                        measurement.dateTime = date + " " + reader.ReadElementContentAsString();
                        
                        reader.Skip();
                    }

                    if (reader.Name.Equals("TEMP"))
                    {
                        if (!float.TryParse(reader.ReadElementContentAsString(), out measurement.Temperature))
                        {
                        }
                        reader.Skip();
                    }

                    if (reader.Name.Equals("DEWP"))
                    {
                        if (!float.TryParse(reader.ReadElementContentAsString(), out measurement.Dewpoint))
                        {
                        }

                        reader.Skip();
                    }

                    if (reader.Name.Equals("STP"))
                    {
                        if (!float.TryParse(reader.ReadElementContentAsString(), out measurement.STP))
                        {
                        }
                        reader.Skip();
                    }

                    if (reader.Name.Equals("SLP"))
                    {
                        if (!float.TryParse(reader.ReadElementContentAsString(), out measurement.SLP))
                        {
                            
                        }
                        reader.Skip();
                    }

                    if (reader.Name.Equals("VISIB"))
                    {
                        float.TryParse(reader.ReadElementContentAsString(), out measurement.VISIB);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("WDSP"))
                    {
                        float.TryParse(reader.ReadElementContentAsString(), out measurement.WDSP);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("PRCP"))
                    {
                        float.TryParse(reader.ReadElementContentAsString(), out measurement.PRCP);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("SNDP"))
                    {
                        float.TryParse(reader.ReadElementContentAsString(), out measurement.SNDP);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("FRSHTT"))
                    {
                        byte.TryParse(reader.ReadElementContentAsString(), out measurement.FRSHTT);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("CLDC"))
                    {
                        float.TryParse(reader.ReadElementContentAsString(), out measurement.CLDC);
                        reader.Skip();
                    }

                    if (reader.Name.Equals("WNDDIR"))
                    {
                        int.TryParse(reader.ReadElementContentAsString(), out measurement.WNDDIR);
                        reader.Skip();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
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
        public Dictionary<int, WeatherStation> WeatherStations = new Dictionary<int, WeatherStation>(10);
        public Socket workSocket = null;
        public const int BUFFER_SIZE = 1024;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
        public bool Exception;
        public Task CurrentTask = Task.Run(() => { });
    }

    public class MeasurementData
    {
        /// <summary> DateTime of recording </summary>
        public string dateTime;
        /// <summary> Station ID </summary>
        public int StationNumber;

        /// <summary> Temperature </summary>
        public float Temperature;

        /// <summary> Dewpoint </summary>
        public float Dewpoint;
        public float STP;
        public float SLP;
        public float VISIB;
        public float WDSP;
        public float PRCP;
        public float SNDP;
        public byte FRSHTT;
        public float CLDC;
        public int WNDDIR;

        /// <summary>
        /// Wether the data has already been added to database.
        /// to avoid duplicates and still be able to keep history of 30 values.
        /// </summary>
        public bool AddedToDatabase;

        public void CheckValues()
        {
        }
    }

    public class WeatherStation
    {
        public int StationNumber;
        // Since a little delay of 30 seconds in the database shouldn't really matter choose to split the queues
        // Makes processing easier, just put elements in sqlQueue if count is bigger than 30.
        public Queue<MeasurementData> MeasurementDatas = new Queue<MeasurementData>(30);
        public List<MeasurementData> SqlQueue = new List<MeasurementData>();

        public WeatherStation(int stationNumber)
        {
            StationNumber = stationNumber;
        }

        public void Enqueue(MeasurementData measurement)
        {
            // Check if count equals 30, and if so move element to the SQL queue
            while (MeasurementDatas.Count >= 30)
            {
                var toAdd = MeasurementDatas.Dequeue();
                lock(SqlQueue)
                    SqlQueue.Add(toAdd);
                SqlDequeue();
            }

            MeasurementDatas.Enqueue(measurement);
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

    }
}
