using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XMLReader
{
    class Listener
    {
        public Socket Handler;
        private Socket _listener;

        private ConcurrentBag<MeasurementData> measurementList = new ConcurrentBag<MeasurementData>();
        byte[] bytes = new byte[256];

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
            Handler = handler;

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

            int read = s.EndReceive(ar);

            if (read > 0)
            {
                string str = Encoding.ASCII.GetString(so.buffer, 0, read);
                so.sb.Append(str);
                if (str.IndexOf("</WEATHERDATA>", str.Length - 20) > -1)
                {
                    string strContent = so.sb.ToString();
                    if(so.CurrentTask.IsCompleted)
                        so.CurrentTask = Task.Run(() => { ParseXML(strContent, so); });
                    so.sb.Clear();
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

        void ParseXML(string XML, StateObject so)
        {
            var XMLDeclerationIndex = XML.IndexOf("<?xml");
            if (XMLDeclerationIndex != 0)
            {
                so.sb.Clear();
                return;
            }
            
            // Is inside a using Statement because XmlReader and StringReader are ignored by GC
            // Forgetting GC in such cases causes quite significant memory leaks.
            using (var reader = XmlReader.Create(new StringReader(XML)))
            {
                try
                {
                    reader.MoveToContent();
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "MEASUREMENT")
                            measurementList.Add(ParseMeasurement(reader));
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                }
            }
        }

        MeasurementData ParseMeasurement(XmlReader reader)
        {
            MeasurementData measurement = new MeasurementData();
            var element = string.Empty;
            var value = string.Empty;
            var date = string.Empty;
            
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "MEASUREMENT")
                {
                    break;
                }

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        element = reader.Name;
                        continue;

                    case XmlNodeType.EndElement:
                        element = string.Empty;
                        continue;

                    case XmlNodeType.Text:
                        switch (element)
                        {
                            case "STN":
                                measurement.STN = reader.ReadContentAsInt();
                                continue;

                            case "DATE":
                                date = reader.Value;
                                continue;

                            case "TIME":
                                if (DateTime.TryParse(date + " " + reader.Value, out DateTime dateTime))
                                {
                                    measurement.dateTime = dateTime;
                                }
                                continue;

                            case "TEMP":
                                measurement.TEMP = reader.ReadContentAsFloat();
                                continue;

                            case "DEWP":
                                measurement.DEWP = reader.ReadContentAsFloat();
                                continue;

                            case "STP":
                                measurement.STP = reader.ReadContentAsFloat();
                                continue;

                            case "SLP":
                                measurement.SLP = reader.ReadContentAsFloat();
                                continue;

                            case "VISIB":
                                measurement.VISIB = reader.ReadContentAsFloat();
                                continue;

                            case "WDSP":
                                measurement.WDSP = reader.ReadContentAsFloat();
                                continue;

                            case "PRCP":
                                measurement.PRCP = reader.ReadContentAsFloat();
                                continue;

                            case "SNDP":
                                measurement.SNDP = reader.ReadContentAsFloat();
                                continue;

                            case "FRSHTT":
                                measurement.FRSHTT = reader.ReadContentAsFloat();
                                continue;

                            case "CLDC":
                                
                                measurement.CLDC = reader.ReadContentAsFloat();
                                continue;

                            case "WNDDIR":
                                measurement.WNDDIR = reader.ReadContentAsFloat();
                                continue;

                        }
                        
                        continue;


                }
            }
            return measurement;
        }
    }

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BUFFER_SIZE = 1024;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
        public bool Exception;
        public Task CurrentTask = Task.Run(() => { });
    }

    public struct MeasurementData
    {
        /// <summary> DateTime of recording </summary>
        public DateTime dateTime;
        /// <summary> Station ID </summary>
        public int STN;

        /// <summary> Temperature </summary>
        public float TEMP;

        /// <summary> Dewpoint </summary>
        public float DEWP;
        public float STP;
        public float SLP;
        public float VISIB;
        public float WDSP;
        public float PRCP;
        public float SNDP;
        public float FRSHTT;
        public float CLDC;
        public float WNDDIR;

        public void CheckValues()
        {
        }
    }
}
