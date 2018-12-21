﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

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

        void ParseMeasurement(XmlReader reader)
        {
            MeasurementData measurement = new MeasurementData();

            if (!reader.ReadToFollowing("STN")) return;

            // The code now following. Plis ignore, lot of repetitive code, since using objects for performance.
            try
            {
                uint.TryParse(reader.ReadElementString(), out measurement.StationNumber);

                var weatherStation = _controller.WeatherStations[measurement.StationNumber];

                // reader.Skip skips one node (Skips to next start element in this XML file)
                // Doesn't validate the XML so is quicker than calling .read multiple times
                reader.Skip();

                var date = reader.ReadElementString();
                reader.Skip();
                measurement.DateTime = date + " " + reader.ReadElementString();
                reader.Skip();

                const NumberStyles numberStyleNegative = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                const NumberStyles numberStylePositive = NumberStyles.AllowDecimalPoint;

                var culture = NumberFormatInfo.InvariantInfo;

                if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Temperature))
                {
                    // If value fails to parse set the value to the average of last 30 seconds.
                    measurement.Temperature = weatherStation.TemperatureAvg;
                }

                // Check if the data is not a peak. The not equals check is to avoid buggy behaviour when the value and average is 0.
                if (measurement.Temperature != weatherStation.TemperatureAvg &&
                    measurement.Temperature <= weatherStation.TemperatureAvg * 1.2 &&
                    measurement.Temperature >= weatherStation.TemperatureAvg * 0.8)
                {
                    measurement.Temperature = weatherStation.TemperatureAvg;
                }

                reader.Skip();

                if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Dewpoint))
                {
                    measurement.Dewpoint = weatherStation.DewpointAvg;
                }

                if (measurement.Dewpoint != weatherStation.DewpointAvg &&
                    measurement.Dewpoint <= weatherStation.DewpointAvg * 1.2 &&
                    measurement.Dewpoint >= weatherStation.DewpointAvg * 0.8)
                {
                    measurement.Dewpoint = weatherStation.DewpointAvg;
                }

                reader.Skip();

                if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.StationPressure))
                {
                    measurement.StationPressure = weatherStation.StationPressureAvg;
                }

                if (measurement.StationPressure != weatherStation.StationPressureAvg &&
                    measurement.StationPressure <= weatherStation.StationPressureAvg * 1.2 &&
                    measurement.StationPressure >= weatherStation.StationPressureAvg * 0.8)
                {
                    measurement.StationPressure = weatherStation.StationPressureAvg;
                }

                reader.Skip();

                if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.SeaLevelPressure))
                {
                    measurement.SeaLevelPressure = weatherStation.SeaLevelPressureAvg;
                }

                if (measurement.SeaLevelPressure != weatherStation.SeaLevelPressureAvg &&
                    measurement.SeaLevelPressure <= weatherStation.SeaLevelPressureAvg * 1.2 &&
                    measurement.SeaLevelPressure >= weatherStation.SeaLevelPressureAvg * 0.8)
                {
                    measurement.SeaLevelPressure = weatherStation.SeaLevelPressureAvg;
                }

                reader.Skip();

                if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.Visibility))
                {
                    measurement.Visibility = weatherStation.VisibilityAvg;
                }

                if (measurement.Visibility != weatherStation.VisibilityAvg &&
                    measurement.Visibility <= weatherStation.VisibilityAvg * 1.2 &&
                    measurement.Visibility >= weatherStation.VisibilityAvg * 0.8)
                {
                    measurement.Visibility = weatherStation.VisibilityAvg;
                }

                reader.Skip();

                if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.WindSpeed))
                {
                    measurement.WindSpeed = weatherStation.WindSpeedAvg;
                }

                if (measurement.WindSpeed != weatherStation.WindSpeedAvg &&
                    measurement.WindSpeed <= weatherStation.WindSpeedAvg * 1.2 &&
                    measurement.WindSpeed >= weatherStation.WindSpeedAvg * 0.8)
                {
                    measurement.WindSpeed = weatherStation.WindSpeedAvg;
                }

                reader.Skip();

                if (!double.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.Precipitation))
                {
                    measurement.Precipitation = weatherStation.PrecipitationAvg;
                }

                if (measurement.Precipitation != weatherStation.PrecipitationAvg &&
                    measurement.Precipitation <= weatherStation.PrecipitationAvg * 1.2 &&
                    measurement.Precipitation >= weatherStation.PrecipitationAvg * 0.8)
                {
                    measurement.Precipitation = weatherStation.PrecipitationAvg;
                }

                reader.Skip();

                if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Snowfall))
                {
                    measurement.Snowfall = weatherStation.SnowfallAvg;
                }

                if (weatherStation.SnowfallAvg != measurement.Snowfall &&
                    measurement.Snowfall <= weatherStation.SnowfallAvg * 1.2 &&
                    measurement.Snowfall >= weatherStation.SnowfallAvg * 0.8)
                {
                    measurement.Snowfall = weatherStation.SnowfallAvg;
                }

                reader.Skip();

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

                if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture,
                    out measurement.CloudCover))
                {
                    measurement.CloudCover = weatherStation.CloudCoverAvg;
                }

                if (measurement.CloudCover != weatherStation.CloudCoverAvg &&
                    measurement.CloudCover <= weatherStation.CloudCoverAvg * 1.2 &&
                    measurement.CloudCover >= weatherStation.CloudCoverAvg * 0.8)
                {
                    measurement.CloudCover = weatherStation.CloudCoverAvg;
                }
                reader.Skip();

                if (!int.TryParse(reader.ReadElementString(), NumberStyles.None, NumberFormatInfo.InvariantInfo,
                    out measurement.WindDirection))
                {
                    measurement.WindDirection = weatherStation.WindDirectionAvg;
                }

                if (measurement.WindDirection != weatherStation.WindDirectionAvg &&
                    measurement.WindDirection <= weatherStation.WindDirectionAvg * 1.2 &&
                    measurement.WindDirection >= weatherStation.WindDirectionAvg * 0.8)
                {
                    measurement.WindDirection = weatherStation.WindDirectionAvg;
                }

                reader.Skip();

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
        public string DateTime;

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
}
