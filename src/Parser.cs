using System;
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
            this._controller = controller;
        }

        private Controller _controller;

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

                        // Parse the measurement and add it to the history queue.
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
                    if (!int.TryParse(reader.ReadElementString(), NumberStyles.None, null,
                        out measurement.StationNumber))
                    {
                        Console.WriteLine("Skipped measurement.");
                        return null;
                    }

                    if (!_controller.WeatherStations.TryGetValue(measurement.StationNumber, out var weatherStation))
                    {
                        weatherStation = new WeatherStation(measurement.StationNumber);
                        _controller.WeatherStations.TryAdd(measurement.StationNumber, weatherStation);
                    }
                    weatherStation.Enqueue(measurement);

                    // reader.Skip skips one node (Skips to next start element in this XML file)
                    // Doesn't validate the XML so is quicker than calling .read multiple times
                    reader.Skip();

                    if (reader.Name == "DATE")
                    {
                        date = reader.ReadElementString();
                        reader.Skip();
                    }

                    if (reader.Name == "TIME")
                    {
                        // Pretty expensive to parse DateTime but saves a bit of RAM, if CPU is overloaded change this to string.
                        if (!DateTime.TryParse(date + " " + reader.ReadElementString(), CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out measurement.DateTime))
                        {
                            Console.WriteLine("Datetime not parsed");
                        }
                        reader.Skip();
                    }

                    var numberStyleNegative = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                    var numberStylePositive = NumberStyles.AllowDecimalPoint;

                    var culture = NumberFormatInfo.InvariantInfo;
                    if (reader.Name == "TEMP")
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Temperature))
                        {
                            measurement.Temperature = weatherStation.MeasurementDatas.Average(p => p.Temperature);
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("DEWP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Dewpoint))
                        {
                            measurement.Dewpoint = weatherStation.MeasurementDatas.Average(p => p.Dewpoint);
                        }

                        reader.Skip();
                        count++;
                    }

                    if (reader.Name == "STP")
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.StationPressure))
                        {
                            measurement.StationPressure = weatherStation.MeasurementDatas.Average(p => p.StationPressure);
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name.Equals("SLP"))
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.SeaLevelPressure))
                        {
                            measurement.SeaLevelPressure = weatherStation.MeasurementDatas.Average(p => p.SeaLevelPressure);
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name == "VISIB")
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.Visibility))
                        {
                            measurement.Visibility = weatherStation.MeasurementDatas.Average(p => p.Visibility);
                        }
                        reader.Skip();
                        count++;
                    }

                    if (reader.Name == "WDSP")
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.WindSpeed))
                        {
                            measurement.WindSpeed = weatherStation.MeasurementDatas.Average(p => p.WindSpeed);
                        }
                        reader.Skip();
                    }

                    if (reader.Name == "PRCP")
                    {
                        if (!double.TryParse(reader.ReadElementString(), numberStylePositive, culture, out measurement.Precipitation))
                        {
                            measurement.Precipitation = weatherStation.MeasurementDatas.Average(p => p.Precipitation);
                        }
                        reader.Skip();
                    }

                    if (reader.Name == "SNDP")
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStyleNegative, culture, out measurement.Snowfall))
                        {
                            measurement.Snowfall = weatherStation.MeasurementDatas.Average(p => p.Snowfall);
                        }
                        reader.Skip();
                    }

                    if (reader.Name == "FRSHTT")
                    {
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
                    }

                    if (reader.Name == "CLDC")
                    {
                        if (!float.TryParse(reader.ReadElementString(), numberStylePositive, culture,
                            out measurement.CloudCover))
                        {
                            measurement.CloudCover = weatherStation.MeasurementDatas.Average(p => p.CloudCover);
                        }
                        reader.Skip();
                    }

                    if (reader.Name == "WNDDIR")
                    {
                        if (!int.TryParse(reader.ReadElementString(), NumberStyles.None, NumberFormatInfo.InvariantInfo,
                            out measurement.WindDirection))
                        {
                            measurement.CloudCover = (int)weatherStation.MeasurementDatas.Average(p => p.WindDirection);
                        }
                        reader.Skip();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return measurement;
        }
    }

    public class MeasurementData
    {
        /// <summary> DateTime of recording </summary>
        public DateTime DateTime;
        /// <summary> Station ID </summary>
        public int StationNumber;


        /// <summary>
        /// Temperature in degrees Celsius. Valid Values range from -9999.9 till 9999.9 with one decimal point precision.
        /// </summary>
        public float Temperature;

        /// <summary>
        /// Dewpoint in degrees Celsius. Valid values range from -9999.9 till 9999.9 with 1 decimal point precision.
        /// </summary>
        public float Dewpoint;

        /// <summary>
        /// Air pressure at the station's level in mBar. valid values range from 0.0 till 9999.9 with 1 decimal point precision.
        /// </summary>
        public float StationPressure;

        /// <summary>
        /// Air pressure at sea level in mBar. Valid values range from 0.0 till 9999.9 with 1 decimal point precision.
        /// </summary>
        public float SeaLevelPressure;

        /// <summary>
        /// Visibility in KM. Valid values range from 0.0 till 999.9, with 1 decimal point precision.
        /// </summary>
        public float Visibility;

        /// <summary>
        /// Windspeed in KM/h. Valid values range from 0.0 till 999.9, with 1 decimal point precision.
        /// </summary>
        public float WindSpeed;

        /// <summary>
        /// Precipitation in cm. Valid values range from 0.00 till 999.99, with 2 decimal points precision.
        /// </summary>
        public double Precipitation;

        /// <summary>
        /// Snowfall in cm. Valid values range from -9999.9 till 9999.9, with 1 decimal point precision.
        /// </summary>
        public float Snowfall;

        /// <summary>
        /// Flag variable containing events on this day.
        /// see enum <see cref="EventFlags"/> for possible flags.
        /// </summary>
        public byte Events;

        /// <summary>
        /// Cloud cover in percentage. Valid values range from 0.0 till 99.9 with 1 decimal point precision.
        /// </summary>
        public float CloudCover;
        /// <summary>
        /// Wind direction in degrees. Valid values range from 0 till 359. Only integers.
        /// </summary>
        public int WindDirection;

        public void GetFlags()
        {
        }

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
