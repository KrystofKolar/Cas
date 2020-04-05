
using Microsoft.Xna.Framework;
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

        // if type is another assembly, iterate over the loaded assemblies.
        // and search for the type.
        // There is still the possibility, that the assembly is not loaded
        // this case is not handled currently.
        //
        // The assembly is not loaded, if during compiletime, there is no
        // no reference/value type used.
        //
        // Created an own project for this case. "Test.ConsoleApp.Types"
        //
        public static Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

        public static Type GetType(string stype)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(stype);

                if (type == null)
                    continue;
                else
                    return type;
            }
            return null;
        }
    }

    public class ConverterDictionaryTKeyEnumTValue : JsonConverterFactory
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

            if (!typeToConvert.GetGenericArguments()[0].IsEnum)
            {
                return false;
            }

            return true;
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                type: typeof(DictionaryEnumConverterInner<,>)
                    .MakeGenericType(new Type[] { keyType, valueType }),
                bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                binder: null, // default
                args: new object[] { options },
                culture: CultureInfo.InvariantCulture);

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
                // used for parsing
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

                    // Enums keys are always strings
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
                        // try reuse existing Converter
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

                throw new JsonException(); // EndObject not reached
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
                        // try use our converter
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
            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, object> dict,
            JsonSerializerOptions options)
        {
            string s = "";

            foreach (var item in dict)
            {
                //string key = JsonSerializer.Serialize(item.Key, ConvertCommon.);
                //string val = JsonSerializer.Serialize(item.Value, ConvertCommon.jsOptionsSerialize);
                string key="", val="";
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

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        if (reader.Read() &&
                            reader.TokenType == JsonTokenType.PropertyName)
                        {
                            string date = reader.GetString(); // "key"
                            DateTime dt = DateTime.ParseExact(date,
                                                              parse,
                                                              CultureInfo.InvariantCulture);

                            if (reader.Read())
                            {
                                string file = reader.GetString();

                                dict.Add(dt, file);

                                reader.Read(); // consume token: "object end"
                            }
                        }
                    }
                }
            }

            return dict;

            /*


                        string s = reader.GetString();

                        string[] ss = s.Split(',');

                        foreach (var item in ss)
                        {
                            Vector2 v = Vector2.Zero;

                            //Match match = Regex.Match(item, @".*""([a-zA-Z0-9_\-.]+)"":""([a-zA-Z0-9_\-.]+)"".*");
                            string rex = ".*?";
                            Match match = Regex.Match(item, $@"{rex}""({rex})"":""({rex})""{rex}");

                            if (match.Success && match.Groups.Count == 3)
                            {
                                DateTime dt =
                                    DateTime.ParseExact(match.Groups[1].Value,
                                                        parse, CultureInfo.InvariantCulture);

                                string iso = match.Groups[2].Value;

                                dict.Add(dt, iso);
                            }
                            else
                            {
                                throw new JsonException();
                            }
                        }

                        return dict;
            */
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<DateTime, string> dict,
            JsonSerializerOptions options)
        {
            if (dict.Count < 1)
                return;

            writer.WriteStartArray();

                    foreach (var item in dict)
                    {
                        writer.WriteStartObject();

                                string dt = item.Key.ToString(parse, CultureInfo.InvariantCulture);
                                writer.WritePropertyName(dt);

                                writer.WriteStringValue(item.Value);

                        writer.WriteEndObject();
                    }

            writer.WriteEndArray();
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

    // debug only
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
