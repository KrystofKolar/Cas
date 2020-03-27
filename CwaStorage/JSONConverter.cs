using CasBase;
using CommonSb;
using Microsoft.Xna.Framework;
using NETStandardLocation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CwaIsolatedStorage
{
    public static class ConvertCommon
    {
        public static readonly JsonSerializerOptions jsOptions = LoadJsonSerializerOptions();

        private static JsonSerializerOptions LoadJsonSerializerOptions()
        {
            JsonSerializerOptions serializeOptions = new JsonSerializerOptions();

            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterDateTimeOffset());

            //bool no converter
            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterVector2());

            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterEnum<casTextureVariant>());
            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterEnum<Scenario>());
            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterEnum<Cwa.AudioManager.VolumeLevels>());
            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterEnum<casId>());

            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterDateTime());
            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterGeoCoordinate());

            serializeOptions.Converters.Add(new CwaIsolatedStorage.ConverterDictDateTimeString());

            serializeOptions.WriteIndented = true;

            return serializeOptions;
        }

        public static Vector2 Vector2Reader(string s)
        {
            Vector2 v = Vector2.Zero;

            Match match = Regex.Match(s, @"\[(\d+),(\d+)\]");

            if (match.Groups.Count == 3)
            {
                if (match.Success)
                {
                    v.X = float.Parse(match.Groups[1].Value);

                    v.Y = float.Parse(match.Groups[2].Value);
                }
            }

            return v;
        }

        public static string Vector2Writer(Vector2 v)
        {
            string s = $"[{v.X},{v.Y}]";

            return s;
        }
    }

    public class ConverterDictDateTimeString : JsonConverter<Dictionary<DateTime, string>>
    {
        private readonly string parse = "yyyy.MM.dd hh.mm.ss dddd";

        public override Dictionary<DateTime, string> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Dictionary<DateTime, string> dict = new Dictionary<DateTime, string>();

            string s = reader.GetString();

            string[] ss = s.Split(',');

            foreach (var item in ss)
            {
                Vector2 v = Vector2.Zero;

                //Match match = Regex.Match(item, @".*""([a-zA-Z0-9_\-.]+)"":""([a-zA-Z0-9_\-.]+)"".*");
                string rex = ".*?";
                Match match = Regex.Match(item, $@"{rex}""({rex})"":""({rex})""{rex}");

                if (match.Groups.Count == 3)
                {
                    if (match.Success)
                    {
                        DateTime dt =
                            DateTime.ParseExact(match.Groups[1].Value,
                                                parse, CultureInfo.InvariantCulture);

                        string iso = match.Groups[2].Value;

                        dict.Add(dt, iso);
                    }
                }
            }

            return dict;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<DateTime, string> dict,
            JsonSerializerOptions options)
        {
            string s = "{\n";

            foreach (var item in dict)
            {
                string dt = item.Key.ToString(parse, CultureInfo.InvariantCulture);
                s += String.Format("\"{0}\":\"{1}\",\n", dt, item.Value);
            }
            s = s.Remove(s.Length - 2, 2); // newline and colon
            s += "\n}";

            writer.WriteStringValue(s);
        }
    }

    public class ConverterGeoCoordinate : JsonConverter<GeoCoordinate>
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

    public class ConverterDateTime : JsonConverter<DateTime>
    {
        ////    FullDateTimePattern:              dddd, MMMM dd, yyyy h:mm:ss tt
        //                                         Example: Monday, May 28, 2012 11:35:00 AM
        private readonly string parse = "yyyy.MM.dd hh.mm.ss dddd";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                DateTime.ParseExact(reader.GetString(),parse, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString(parse, CultureInfo.InvariantCulture));
    }

    public class ConverterDateTimeOffset : JsonConverter<DateTimeOffset>
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

    public class ConverterVector2 : JsonConverter<Vector2>
    {
        public override Vector2 Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ConvertCommon.Vector2Reader(reader.GetString());
        }

        public override void Write(
            Utf8JsonWriter writer,
            Vector2 v,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(ConvertCommon.Vector2Writer(v));
        }
    }

    public class ConverterEnum<T> : JsonConverter<T> where T : Enum
    {
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return (T)Enum.Parse(typeof(T), reader.GetString());
        }

        public override void Write(
            Utf8JsonWriter writer,
            T variant,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(Enum.GetName(typeof(T), variant));
        }
    }
}
