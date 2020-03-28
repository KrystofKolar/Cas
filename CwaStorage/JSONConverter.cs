
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

// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to#support-dictionary-with-non-string-key

namespace CwaIsolatedStorage
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;


    public static class ConvertCommon
    {
        public static readonly JsonSerializerOptions jsOptionsSerialize = LoadJsonSerializerOptions(true);
        public static readonly JsonSerializerOptions jsOptionsSerializeDe = LoadJsonSerializerOptions(false);

        private static JsonSerializerOptions LoadJsonSerializerOptions(bool serialize)
        {
            JsonSerializerOptions serializeOptions = new JsonSerializerOptions();

            serializeOptions.Converters.Add(new ConverterDateTimeOffset());

            serializeOptions.Converters.Add(new ConverterBoolean());
            serializeOptions.Converters.Add(new ConverterVector2());

            serializeOptions.Converters.Add(new ConverterEnum<casTextureVariant>());
            serializeOptions.Converters.Add(new ConverterEnum<Scenario>());
            serializeOptions.Converters.Add(new ConverterEnum<Cwa.AudioManager.VolumeLevels>());
            serializeOptions.Converters.Add(new ConverterEnum<casId>());

            serializeOptions.Converters.Add(new ConverterDateTime());
            serializeOptions.Converters.Add(new ConverterGeoCoordinate());

            serializeOptions.Converters.Add(new ConverterDictDateTimeString());
            if (serialize == false )
                serializeOptions.Converters.Add(new ConverterDictObject());

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

    // serializeOptions.Converters.Add(new DictionaryTKeyEnumTValueConverter());

    public class DictionaryTKeyEnumTValueConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                return false;
            }

            return typeToConvert.GetGenericArguments()[0].IsEnum;
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(DictionaryEnumConverterInner<,>).MakeGenericType(
                    new Type[] { keyType, valueType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class DictionaryEnumConverterInner<TKey, TValue> :
            JsonConverter<Dictionary<TKey, TValue>> where TKey : struct, Enum
        {
            private readonly JsonConverter<TValue> _valueConverter;
            private Type _keyType;
            private Type _valueType;

            public DictionaryEnumConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _valueConverter = (JsonConverter<TValue>)options
                    .GetConverter(typeof(TValue));

                // Cache the key and value types.
                _keyType = typeof(TKey);
                _valueType = typeof(TValue);
            }

            public override Dictionary<TKey, TValue> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return dictionary;
                    }

                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string propertyName = reader.GetString();

                    // For performance, parse with ignoreCase:false first.
                    if (!Enum.TryParse(propertyName, ignoreCase: false, out TKey key) &&
                        !Enum.TryParse(propertyName, ignoreCase: true, out key))
                    {
                        throw new JsonException(
                            $"Unable to convert \"{propertyName}\" to Enum \"{_keyType}\".");
                    }

                    // Get the value.
                    TValue v;
                    if (_valueConverter != null)
                    {
                        reader.Read();
                        v = _valueConverter.Read(ref reader, _valueType, options);
                    }
                    else
                    {
                        v = JsonSerializer.Deserialize<TValue>(ref reader, options);
                    }

                    // Add to dictionary.
                    dictionary.Add(key, v);
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer,
                Dictionary<TKey, TValue> dictionary,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                {
                    writer.WritePropertyName(kvp.Key.ToString());

                    if (_valueConverter != null)
                    {
                        _valueConverter.Write(writer, kvp.Value, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, kvp.Value, options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
    public class ConverterDictObject : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            string s = reader.
            foreach (char c in s)
            {
                dict.Add(c.ToString(), s);
            }
            //string[] ss = s.Split(',');

            //foreach (var item in ss)
            //{
            //    Vector2 v = Vector2.Zero;

            //    //Match match = Regex.Match(item, @".*""([a-zA-Z0-9_\-.]+)"":""([a-zA-Z0-9_\-.]+)"".*");
            //    string rex = ".*?";
            //    Match match = Regex.Match(item, $@"{rex}""({rex})"":""({rex})""{rex}");

            //    if (match.Groups.Count == 3)
            //    {
            //        if (match.Success)
            //        {
            //            DateTime dt =
            //                DateTime.ParseExact(match.Groups[1].Value,
            //                                    parse, CultureInfo.InvariantCulture);

            //            string iso = match.Groups[2].Value;

            //            dict.Add(dt, iso);
            //        }
            //    }
            //}

            return dict;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, object> dict,
            JsonSerializerOptions options)
        {
            string s = "";

            foreach (var item in dict)
            {
                string key = JsonSerializer.Serialize(item.Key, ConvertCommon.jsOptionsSerialize);
                string val = JsonSerializer.Serialize(item.Value, ConvertCommon.jsOptionsSerialize);

                s += $"{key}:{val},\n";
            }

            s = s.Remove(s.Length - 1, 1);

            writer.WriteStringValue(s);
            //string s = "{\n";

            //foreach (var item in dict)
            //{
            //    string dt = item.Key.ToString(parse, CultureInfo.InvariantCulture);
            //    s += String.Format("\"{0}\":\"{1}\",\n", dt, item.Value);
            //}
            //s = s.Remove(s.Length - 2, 2); // newline and colon
            //s += "\n}";

            //writer.WriteStringValue(s);
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
            => ConvertCommon.Vector2Reader(reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            Vector2 v,
            JsonSerializerOptions options)
            => writer.WriteStringValue(ConvertCommon.Vector2Writer(v));
    }

    public class ConverterBoolean : JsonConverter<bool>
    {
        public override bool Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            bool b =  reader.GetBoolean();
            return b;
        }
        public override void Write(
            Utf8JsonWriter writer,
            bool b,
            JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(b);
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
