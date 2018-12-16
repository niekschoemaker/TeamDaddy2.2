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
    }
}
