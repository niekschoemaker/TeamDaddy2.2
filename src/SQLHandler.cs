using System;
using System.Collections.Generic;
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
        }

        public void AddWeatherStations()
        {
            using (MySqlConnection connection = new MySqlConnection("server=localhost;PORT=3307;database=unwdmi;user id=root;password=DaddyCool"))
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

        private Controller controller;
    }
}
