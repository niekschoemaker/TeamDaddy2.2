﻿using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Google.Protobuf;
using unwdmi.Protobuf;

namespace unwdmi.Parser
{
    class Parser
    {
        public Parser(Controller controller)
        {
            _controller = controller;
        }

        private readonly Controller _controller;

        public void ParseXML(string XML)
        {
            if (!XML.StartsWith("<?x") || !XML.EndsWith("TA>\n"))
            {
                XML = XML.Substring(XML.IndexOf("<?xml", StringComparison.Ordinal),
                    XML.IndexOf("TA>", StringComparison.Ordinal) + 4);
            }

            // Is inside a using Statement because XmlReader and StringReader are ignored by GC
            // Forgetting GC in such cases causes quite significant memory leaks.
            using (var reader = XmlReader.Create(new StringReader(XML)))
            {
                try
                {
                    var count = 0;
                    reader.ReadToFollowing("MEASUREMENT");
                    while (count != 10)
                    {
                        // Parse the measurement
                        ParseMeasurement(reader);

                        count++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Main parser, parses all measurement variables to correct types and adds them to the WeatherStation <see cref="WeatherStation"/>
        /// </summary>
        /// <param name="reader"> XMLReader positioned at WeatherData starting tag. </param>
        void ParseMeasurement(XmlReader reader)
        {
            if (!reader.ReadToFollowing("STN")) return;

            // The code now following. Plis ignore, lot of repetitive code, since using objects for performance.
            try
            {
                #region Identification (StationNumber and DateTime)

                uint stationNumber;
                uint.TryParse(reader.ReadElementString(), out stationNumber);

                if (!_controller.WeatherStations.ContainsKey(stationNumber))
                {
                    _controller.WeatherStations.Add(stationNumber, new WeatherStation(stationNumber, string.Empty, string.Empty, 0.0, 0.0, 0.0));
                }

                var weatherStation = _controller.WeatherStations[stationNumber];

                // reader.Skip skips one node (Skips to next start element in this XML file)
                // Doesn't validate the XML so is quicker than calling .read multiple times
                reader.Skip();

                var date = reader.ReadElementString();
                reader.Skip();
                DateTime dateTime;
                DateTime.TryParse(date + " " + reader.ReadElementString(), out dateTime);
                reader.Skip();
#endregion

                // Set up a few custom NumberStyles (defines the amount of things float.TryParse tries on a number, minor performance increase)
                const NumberStyles numberStyleNegative = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                const NumberStyles numberStylePositive = NumberStyles.AllowDecimalPoint;

                var culture = NumberFormatInfo.InvariantInfo;
                #region Temperature

                bool humidityMissing = false;
                // Cache value containing latest value read from XMLReader.
                float temperature = 0f;
                var currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStyleNegative, culture, out temperature))
                {
                    humidityMissing = true;
                }

                reader.Skip();
#endregion

                #region DewPoint

                float dewpoint = 0f;
                currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStyleNegative, culture, out dewpoint))
                {
                    humidityMissing = true;
                }

                reader.Skip();
#endregion

                reader.Skip();

                reader.Skip();

                reader.Skip();

                #region WindSpeed

                float windSpeed;

                currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStylePositive, culture, out windSpeed))
                {
                    windSpeed = weatherStation.WindSpeedTotal / 30;
                }

                else
                {
                    var windSpeedAvg = weatherStation.WindSpeedTotal / 30;
                    if (windSpeed != windSpeedAvg &&
                  windSpeed <= windSpeedAvg * 1.2 &&
                  windSpeed >= windSpeedAvg * 0.8)
                    {
                        windSpeed = windSpeedAvg;
                    }
                }

                reader.Skip();
#endregion

                reader.Skip();

                reader.Skip();

                reader.Skip();

                #region CloudCover

                float cloudCover;
                currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStylePositive, culture,
                    out cloudCover))
                {
                    cloudCover = weatherStation.CloudCoverTotal / 30;
                }
                else
                {
                    var cloudCoverAvg = weatherStation.CloudCoverTotal / 30;
                    if (cloudCover != cloudCoverAvg &&
                        cloudCover <= cloudCoverAvg * 1.2 &&
                        cloudCover >= cloudCoverAvg * 0.8)
                    {
                        cloudCover = cloudCoverAvg;
                    }
                }

                reader.Skip();
#endregion

                reader.Skip();
                double humidity;
                if (!humidityMissing)
                {
                    humidity = Math.Exp(17.625 * dewpoint / (243.03 + dewpoint)) / Math.Exp(17.625 * temperature / (243.04 + temperature));

                    double humidityAvg = weatherStation.HumidityTotal / 30;
                    if (humidity != humidityAvg &&
                        humidity <= humidityAvg * 1.2 &&
                        humidity >= humidityAvg * 0.8)
                    {
                        humidity = humidityAvg;
                    }
                }
                else
                {
                    humidity = weatherStation.HumidityTotal / 30;
                }

                Measurement measurement = new Measurement()
                {
                    CloudCover = cloudCover,
                    DateTime = dateTime.ToBinary(),
                    Humidity = humidity,
                    StationID = stationNumber,
                    WindSpeed = windSpeed
                };

                Request request = new Request()
                {
                    Command = Request.Types.Command.WeatherData
                };

                weatherStation.Enqueue(measurement);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class MeasurementData
    {
        /// <summary> Station ID </summary>
        public uint StationNumber;

        /// <summary> DateTime of recording </summary>
        public DateTime DateTime;

        /// <summary> Temperature in degrees Celsius. Valid Values range from -9999.9 till 9999.9 with one decimal point precision. </summary>
        public float Temperature;

        /// <summary> Dewpoint in degrees Celsius. Valid values range from -9999.9 till 9999.9 with 1 decimal point precision. </summary>
        public float Dewpoint;

        /// <summary> Air pressure at the station's level in mBar. valid values range from 0.0 till 9999.9 with 1 decimal point precision. </summary>
        public float StationPressure;

        /// <summary> Air pressure at sea level in mBar. Valid values range from 0.0 till 9999.9 with 1 decimal point precision. </summary>
        public float SeaLevelPressure;

        /// <summary> Visibility in KM. Valid values range from 0.0 till 999.9, with 1 decimal point precision.</summary>
        public float Visibility;

        /// <summary> Windspeed in KM/h. Valid values range from 0.0 till 999.9, with 1 decimal point precision.</summary>
        public float WindSpeed;

        /// <summary> Precipitation in cm. Valid values range from 0.00 till 999.99, with 2 decimal points precision.</summary>
        public double Precipitation;

        /// <summary> Snowfall in cm. Valid values range from -9999.9 till 9999.9, with 1 decimal point precision.</summary>
        public float Snowfall;

        /// <summary> Flag variable containing events on this day. see enum <see cref="EventFlags"/> for possible flags.</summary>
        public byte Events;

        /// <summary> Cloud cover in percentage. Valid values range from 0.0 till 99.9 with 1 decimal point precision.</summary>
        public float CloudCover;
        /// <summary> Wind direction in degrees. Valid values range from 0 till 359. Only integers.</summary>
        public ushort WindDirection;

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
}
