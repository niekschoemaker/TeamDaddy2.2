using System;
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

                // Cache value containing latest value read from XMLReader.
                float temperature;
                var currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStyleNegative, culture, out temperature))
                {
                    // If value fails to parse set the value to the average of last 30 seconds.
                    temperature = weatherStation.TemperatureTotal / 30;
                }
                // Check if the data is not a peak. The not equals check is to avoid buggy behaviour when the value and average is 0.
                else
                {
                    var temperatureAvg = weatherStation.TemperatureTotal / 30;
                    if (temperature != temperatureAvg &&
                        temperature <= temperatureAvg * 1.2 &&
                        temperature >= temperatureAvg * 0.8)
                    {
                        temperature = temperatureAvg;
                    }

                }

                reader.Skip();
#endregion

                #region DewPoint

                float dewpoint;
                currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStyleNegative, culture, out dewpoint))
                {
                    dewpoint = weatherStation.DewpointTotal / 30;
                }
                else
                {
                    var dewPointAvg = weatherStation.DewpointTotal / 30;
                    if (dewpoint != dewPointAvg &&
                        dewpoint <= dewPointAvg * 1.2 &&
                        dewpoint >= dewPointAvg * 0.8)
                    {
                        dewpoint = dewPointAvg;
                    }
                }

                reader.Skip();
#endregion

                #region StationPressure

                /*currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStylePositive, culture, out measurement.StationPressure))
                {
                    measurement.StationPressure = weatherStation.StationPressureTotal / 30;
                }
                else
                {
                    var stationPressureAvg = weatherStation.StationPressureTotal / 30;
                    if (measurement.StationPressure != stationPressureAvg &&
                        measurement.StationPressure <= stationPressureAvg * 1.2 &&
                        measurement.StationPressure >= stationPressureAvg * 0.8)
                    {
                        measurement.StationPressure = stationPressureAvg;
                    }
                }*/

                reader.Skip();
#endregion

                #region SeaLevelPressure

                /*currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStylePositive, culture, out measurement.SeaLevelPressure))
                {
                    measurement.SeaLevelPressure = weatherStation.SeaLevelPressureTotal / 30;
                }

                else
                {
                    var seaLevelPressureAvg = weatherStation.SeaLevelPressureTotal / 30;
                    if (measurement.SeaLevelPressure != seaLevelPressureAvg &&
                        measurement.SeaLevelPressure <= seaLevelPressureAvg * 1.2 &&
                        measurement.SeaLevelPressure >= seaLevelPressureAvg * 0.8)
                    {
                        measurement.SeaLevelPressure = seaLevelPressureAvg;
                    }
                }*/

                reader.Skip();
#endregion
                
                #region Visibility

                /*currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStylePositive, culture, out measurement.Visibility))
                {
                    measurement.Visibility = weatherStation.VisibilityTotal / 30;
                }

                else
                {
                    var visibilityAverage = weatherStation.VisibilityTotal / 30;
                    if (measurement.Visibility != visibilityAverage &&
                    measurement.Visibility <= visibilityAverage * 1.2 &&
                    measurement.Visibility >= visibilityAverage * 0.8)
                    {
                        measurement.Visibility = visibilityAverage;
                    }
                }*/

                reader.Skip();
#endregion

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

                #region Precipitation

                /*currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !double.TryParse(currentValue, numberStylePositive, culture, out measurement.Precipitation))
                {
                    measurement.Precipitation = weatherStation.PrecipitationTotal / 30;
                }
                else
                {
                    var precipitationAvg = weatherStation.PrecipitationTotal / 30;
                    if (measurement.Precipitation != precipitationAvg &&
                    measurement.Precipitation <= precipitationAvg * 1.2 &&
                    measurement.Precipitation >= precipitationAvg * 0.8)
                    {
                        measurement.Precipitation = precipitationAvg;
                    }
                }*/

                reader.Skip();
#endregion

                #region Snowfall

                /*currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !float.TryParse(currentValue, numberStyleNegative, culture, out measurement.Snowfall))
                {
                    measurement.Snowfall = weatherStation.SnowfallTotal / 30;
                }
                else
                {
                    var snowFallAvg = weatherStation.SnowfallTotal / 30;
                    if (measurement.Snowfall != snowFallAvg &&
                    measurement.Snowfall <= snowFallAvg * 1.2 &&
                    measurement.Snowfall >= snowFallAvg * 0.8)
                    {
                        measurement.Snowfall = snowFallAvg;
                    }
                }*/

                reader.Skip();
#endregion
                #region Events

                /*currentValue = reader.ReadElementString();
                // If string IsNull skip frshtt calculation, used goto to reduce nesting.
                if (string.IsNullOrEmpty(currentValue))
                    goto CloudCover;

                var frshtt = currentValue.ToCharArray();

                byte total = 0;
                if (frshtt.Length != 0)
                {
                    for (int i = 0; i < frshtt.Length; i++)
                    {
                        total += frshtt[i] == '0' ? (byte)0 : (byte)Math.Pow(2, 5 - i);
                    }
                }

                measurement.Events = total;*/
                reader.Skip();
#endregion

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

                #region WindDirection

                /*currentValue = reader.ReadElementString();
                if (string.IsNullOrEmpty(currentValue) || !ushort.TryParse(currentValue, NumberStyles.None, NumberFormatInfo.InvariantInfo,
                    out measurement.WindDirection))
                {
                    measurement.WindDirection = (ushort)(weatherStation.WindDirectionTotal / 30);
                }
                else
                {
                    var windDirectionAvg = weatherStation.WindDirectionTotal / 30;
                    if (measurement.WindDirection != windDirectionAvg &&
                  measurement.WindDirection <= windDirectionAvg * 1.2 &&
                  measurement.WindDirection >= windDirectionAvg * 0.8)
                    {
                        measurement.WindDirection = (ushort)windDirectionAvg;
                    }
                }*/
#endregion

                Measurement measurement = new Measurement()
                {
                    CloudCover = cloudCover,
                    DateTime = dateTime.ToBinary(),
                    Dewpoint = dewpoint,
                    StationID = stationNumber,
                    Temperature = temperature,
                    WindSpeed = windSpeed
                };

                Request request = new Request()
                {
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
