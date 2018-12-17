using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using XMLReader;

namespace ProjectTeamDaddy2._2
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
        }

        private Controller controller;
        private readonly string databaseHost;
        private readonly string databaseUserId;
        private readonly string databasePassword;
        private readonly string databaseDb;
        private readonly string databasePort;

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
