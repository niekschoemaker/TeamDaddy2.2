using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace unwdmi.Parser
{
    class SqlHandler
    {
        public SqlHandler(Controller controller)
        {
            this.controller = controller;
            CheckConfig();
            //Load Config variables.
            databaseHost = ConfigurationManager.AppSettings["DatabaseHost"];
            databaseUserId = ConfigurationManager.AppSettings["DatabaseUserId"];
            databasePassword = ConfigurationManager.AppSettings["DatabasePassword"];
            databasePort = ConfigurationManager.AppSettings["DatabasePort"];
            databaseDb = ConfigurationManager.AppSettings["DatabaseDB"];
            connection = new MySqlConnection($"server={databaseHost};PORT={databasePort};database={databaseDb};user id={databaseUserId};password={databasePassword}");
        }

        private readonly MySqlConnection connection;
        private readonly Controller controller;
        private readonly string databaseHost;
        private readonly string databaseUserId;
        private readonly string databasePassword;
        private readonly string databaseDb;
        private readonly string databasePort;
        
        private Task _sqlTask = Task.CompletedTask;

        public void ExecuteNonQuery(StringBuilder sb)
        {
                var task = _sqlTask;
                _sqlTask = Task.Run(async () =>
                {
                    await task;
                    try
                    {
                        connection.Open();
                        MySqlCommand command = new MySqlCommand(sb.ToString(), connection);
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    connection.Close();
                });
        }

        public void AddData()
        {
            string query;
            lock (controller.SqlStringBuilder)
            {
                controller.SqlStringBuilder[controller.SqlStringBuilder.Length - 2] = ';';
                query = controller.SqlStringBuilder.ToString();
                controller.SqlStringBuilder.Clear();
                controller.SqlStringBuilder.AppendFormat(
                    "INSERT INTO measurements (StationNumber, DateTime, Temperature, Dewpoint, WindSpeed, CloudCover)\nVALUES");
                controller.SqlQueueCount = 0;
            }
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            connection.Close();
        }

        public void CheckSqlQueue()
        {
            while (true)
            {
                if(controller.ActiveParsers > 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (controller.OpenSockets == 0 || controller.SqlQueueCount == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // Check if every parser has added their data for that second and if so add the data to the Database
                Console.WriteLine(controller.SqlQueueCount);
                //AddData();

            }
        }

        public void AddWeatherStations()
        {
            using (MySqlConnection connection = new MySqlConnection($"server={databaseHost};PORT={databasePort};database={databaseDb};user id={databaseUserId};password={databasePassword}"))
            {
                MySqlCommand command = new MySqlCommand("SELECT * FROM unwdmi.stations", connection);
                command.Connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var STN = reader.GetUInt32("stn");
                    var name = reader.GetString("name");
                    var country = reader.GetString("country");
                    var latitude = reader.GetDouble("latitude");
                    var longitude = reader.GetDouble("longitude");
                    var elevation = reader.GetDouble("elevation");
                    controller.WeatherStations.Add(STN,
                        new WeatherStation(STN, name, country, latitude, longitude, elevation));
                }
            }
        }

        public void CheckConfig()
        {
            Configuration configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = configManager.AppSettings.Settings;

            var allKeys = confCollection.AllKeys;
            bool changed = false;
            //Check if any of the keys are missing and if they are substitute them with the default value.
            if (!allKeys.Contains("DatabaseHost"))
            {
                confCollection.Add("DatabaseHost", "192.168.178.87");
                changed = true;
            }

            if (!allKeys.Contains("DatabaseUserId"))
            {
                confCollection.Add("DatabaseUserId", "unwdmi");
                changed = true;
            }

            if (!allKeys.Contains("DatabasePassword"))
            {
                confCollection.Add("DatabasePassword", "DaddyCool");
                changed = true;
            }

            if (!allKeys.Contains("DatabasePort"))
            {
                confCollection.Add("DatabasePort", "3307");
                changed = true;
            }

            if (!allKeys.Contains("DatabaseDB"))
            {
                confCollection.Add("DatabaseDB", "unwdmi");
                changed = true;
            }

            if (changed)
            {
                configManager.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection(configManager.AppSettings.SectionInformation.Name);
            }
        }
    }
}
