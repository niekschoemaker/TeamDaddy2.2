using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
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

        private Task<int> sqlTask = Task.Run(() => 1);
        private readonly MySqlConnection connection;
        private readonly Controller controller;
        private readonly string databaseHost;
        private readonly string databaseUserId;
        private readonly string databasePassword;
        private readonly string databaseDb;
        private readonly string databasePort;

        public void ExecuteNonQuery(string query)
        {
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(query, connection);
                sqlTask.Wait();
                command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            connection.Close();
        }

        public void AddData(List<MeasurementData> measurementDatas)
        {
            if (measurementDatas.Count >= controller.WeatherStations.Count)
            {

                StringBuilder sb = new StringBuilder();

                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                int count = 0;
                foreach (var m in measurementDatas)
                {
                    if (count == 0)
                    {
                        sb.Append($"INSERT INTO measurements ({nameof(m.StationNumber)}, {nameof(m.DateTime)}, {nameof(m.Temperature)}, {nameof(m.Events)}, {nameof(m.SeaLevelPressure)}, {nameof(m.Snowfall)}, {nameof(m.StationPressure)}, {nameof(m.Visibility)}, {nameof(m.WindDirection)}, {nameof(m.CloudCover)}, {nameof(m.Dewpoint)}, {nameof(m.Precipitation)}, {nameof(m.WindSpeed)})\n" +
                            $"VALUES");
                    }

                    if (count == measurementDatas.Count - 1)
                    {
                        sb.Append($"({m.StationNumber}, '{m.DateTime:yyyy-MM-dd HH:mm:ss}', {m.Temperature}, {m.Events}, {m.SeaLevelPressure}, {m.Snowfall}, {m.StationPressure}, {m.Visibility}, {m.WindDirection}, {m.CloudCover}, {m.Dewpoint}, {m.Precipitation}, {m.WindSpeed});");
                        count++;
                        continue;
                    }
                    sb.Append($"({m.StationNumber}, '{m.DateTime:yyyy-MM-dd HH:mm:ss}', {m.Temperature}, {m.Events}, {m.SeaLevelPressure}, {m.Snowfall}, {m.StationPressure}, {m.Visibility}, {m.WindDirection}, {m.CloudCover}, {m.Dewpoint}, {m.Precipitation}, {m.WindSpeed}),\n");
                    count++;
                }
                //sb.Append("END;\n");

                ExecuteNonQuery(sb.ToString());
                sb.Clear();
                controller.DataAdded += (ulong)measurementDatas.Count;
                CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-us");
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
                    var STN = reader.GetInt32("stn");
                    var name = reader.GetString("name");
                    var country = reader.GetString("country");
                    var latitude = reader.GetDouble("latitude");
                    var longitude = reader.GetDouble("longitude");
                    var elevation = reader.GetDouble("elevation");
                    controller.WeatherStations.TryAdd(STN,
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
                confCollection.Add("DatabaseHost", "localhost");
                changed = true;
            }

            if (!allKeys.Contains("DatabaseUserId"))
            {
                confCollection.Add("DatabaseUserId", "root");
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
