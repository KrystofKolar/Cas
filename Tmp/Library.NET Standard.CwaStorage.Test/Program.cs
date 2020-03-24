using CasBase;
using Cwa;
using CwaCommon;
using CwaIsolatedStorage;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Text.RegularExpressions;

namespace Library.NET_Standard.CwaStorage.Test
{
    public class GeoCoordinateConverter : JsonConverter<GeoCoordinate>
    {
        public override GeoCoordinate Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            GeoCoordinate geo = new GeoCoordinate();
 
            string s = reader.GetString();
            // matching a float: ([+-]?([0-9]*[.])?[0-9]+)
            // [float,float,float]
            Match match = Regex.Match(s, @"\[([+-]?([0-9]*[.])?[0-9]+),([+-]?([0-9]*[.])?[0-9]+),([+-]?([0-9]*[.])?[0-9]+)\]");

            if (match.Groups.Count == 4)
            {
                if (match.Success)
                {
                    geo.Longitude = float.Parse(match.Groups[1].Value);

                    geo.Latitude = float.Parse(match.Groups[2].Value);

                    geo.Altitude = float.Parse(match.Groups[3].Value);
                }
            }

            return geo;
        }
        public override void Write(
            Utf8JsonWriter writer,
            GeoCoordinate geo,
            JsonSerializerOptions options)
        {
            IFormatProvider ifp = CultureInfo.InvariantCulture.NumberFormat;
            string g = "[" + geo.Longitude.ToString(ifp) + "," + 
                             geo.Latitude.ToString(ifp) + "," + 
                             geo.Altitude.ToString(ifp) + "]";
            writer.WriteStringValue(g);
        }
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        ////    FullDateTimePattern:              dddd, MMMM dd, yyyy h:mm:ss tt
        //                                         Example: Monday, May 28, 2012 11:35:00 AM
        private readonly string parse = "yyyy.MM.dd hh.mm.ss dddd";
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                DateTime.ParseExact(reader.GetString(),
                    parse, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString(
                    parse, CultureInfo.InvariantCulture));
    }

    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset> 
    {
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                DateTimeOffset.ParseExact(reader.GetString(),
                    "MM/dd/yyyy", CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString(
                    "MM/dd/yyyy", CultureInfo.InvariantCulture));
    }

    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Vector2 v = Vector2.Zero;

            string s = reader.GetString();
            Match match = Regex.Match(s, @"\[(\d+),(\d+)\]");

            if (match.Groups.Count == 3)
            {
                if (match.Success)
                {
                    v.X = float.Parse(match.Groups[1].Value);

                    v.Y = float.Parse(match.Groups[2].Value);
                }
            }

            //if (match.Success)
            //{
            //    v.X = float.Parse(match.Value);
            //}

            //match = match.NextMatch();
            //if (match.Success)
            //{
            //    v.X = float.Parse(match.Value);
            //}

            return v;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Vector2 v2,
            JsonSerializerOptions options)
        {
            //writer.writestringvalue(datetimevalue.tostring(
            //    "mm/dd/yyyy", cultureinfo.invariantculture));

            string s = $"[{v2.X},{v2.Y}]";
            
            writer.WriteStringValue(s);
        }
    }

    public class EnumConverter<T> : JsonConverter<T> where T : Enum
    {
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string s = reader.GetString();
            T variant = (T) Enum.Parse(typeof(T), s);

            return variant;
        }

        public override void Write(
            Utf8JsonWriter writer,
            T variant,
            JsonSerializerOptions options)
        {
            string s = Enum.GetName(typeof(T), variant);

            writer.WriteStringValue(s);
        }
    }

    public class WeatherForecast
    {
        public DateTimeOffset Date { get; set; }
        public int TemperatureCelsius { get; set; }
        public string Summary { get; set; }
        public Vector2 Direction { get; set; }


        public bool isValid { get; set; }
        public casTextureVariant MyTextureVariant { get; set; }
        public Scenario MyScenario { get; set; }
        public AudioManager.VolumeLevels MyVolumeLeves { get; set; }
        public casId MyId { get; set; }
        public DateTime MyDateTime { get; set; }

        public GeoCoordinate MyGeo { get; set; }
    }



    class Program
    {
        void JSONTester()
        {
            WeatherForecast weatherForecast = new WeatherForecast()
            {
                Date = DateTime.Now,
                Summary = "nice",
                TemperatureCelsius = 15,
                Direction = new Vector2(123, 456),

                // Cas related
                isValid = true,
                MyTextureVariant = casTextureVariant.Grey,
                MyScenario = Scenario.GreyDepth,
                MyVolumeLeves = AudioManager.VolumeLevels.LittleMore,
                MyId = casId.boghole,
                MyDateTime = new DateTime(1975, 12, 21, 22, 33, 44),

                MyGeo = new GeoCoordinate() { Longitude=123.4567, Latitude=456.789, Altitude=2345.678}

            };

            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new DateTimeOffsetConverter());


            //bool no converter
            serializeOptions.Converters.Add(new Vector2Converter());

            serializeOptions.Converters.Add(new EnumConverter<casTextureVariant>());
            serializeOptions.Converters.Add(new EnumConverter<Scenario>());
            serializeOptions.Converters.Add(new EnumConverter<AudioManager.VolumeLevels>());
            serializeOptions.Converters.Add(new EnumConverter<casId>());

            serializeOptions.Converters.Add(new DateTimeConverter());
            serializeOptions.Converters.Add(new GeoCoordinateConverter());

            serializeOptions.WriteIndented = true;
            string jsonString = JsonSerializer.Serialize(weatherForecast, serializeOptions);

            const string fname = "weather.bin";
            object ThreadLocker = new object();

            IFormatter formatter = new BinaryFormatter();
            string ffname = Path.Combine(IsfCommon.isf, fname);

            lock (ThreadLocker)
            {
                using (Stream stream = new FileStream(ffname, FileMode.Create))
                {
                    try
                    {
                        formatter.Serialize(stream, jsonString);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"serialize error here {e.Message}");
                    }
                }
            }






            jsonString = "";

            lock (ThreadLocker)
            {
                using (Stream stream = new FileStream(ffname, FileMode.Open))
                {
                    try
                    {
                        jsonString = (string)formatter.Deserialize(stream);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"deserialize error here {e.Message}");
                    }
                }
            }


            var deserializeOptions = new JsonSerializerOptions();
            deserializeOptions.Converters.Add(new DateTimeOffsetConverter());
            deserializeOptions.Converters.Add(new Vector2Converter());

            WeatherForecast weatherForecastDe = 
                JsonSerializer.Deserialize<WeatherForecast>(jsonString, serializeOptions);

        }

        static void Main(string[] args)
        {
            casTextureVariant variant = casTextureVariant.Deprecated;

            variant = (casTextureVariant)Enum.Parse(typeof(casTextureVariant), "Hidden");

            string hid = variant.ToString();

            Program prog = new Program();
            prog.JSONTester();

            return;


            IsfProperty<Vector2> tsv = new IsfProperty<Vector2>("PositionKey", new Vector2(100,200));
            IsfProperty<string> ts = new IsfProperty<string>("KeyProperty", "defaultValue");

            ts.Value = "xyz";

            if (ts.Exists)
            {
                int x = 2;
            }

            string s = ts.GetDefault();

            //Console.ReadKey();

        }
    }
}
