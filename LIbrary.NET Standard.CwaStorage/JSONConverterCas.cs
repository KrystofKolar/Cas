using Cwa;
using CwaNotesTypes;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CasStorage
{
    public class ConverterPackage : JsonConverter<Package>
    {
        protected JsonSerializerOptions _options;

        enum TypeDiscr //class type discriminator
        {
            Default = 0,
            Extended = 1,
        }

        public ConverterPackage()
        {
            InitOptions();
        }

        public virtual void InitOptions()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new CwaIsolatedStorage.ConverterBoolean());
            _options.Converters.Add(new CwaIsolatedStorage.ConverterDateTime());
            _options.Converters.Add(new CwaIsolatedStorage.ConverterEnum<CasBase.Scenario>());
            _options.Converters.Add(new CwaIsolatedStorage.ConverterEnum<AudioManager.VolumeLevels>());
            _options.Converters.Add(new CwaIsolatedStorage.ConverterEnum<CasBase.casId>());
            _options.Converters.Add(new CwaIsolatedStorage.ConverterEnum<CasBase.casTextureVariant>());

            _options.Converters.Add(new ConverterGeoCoordinate());
            _options.Converters.Add(new ConverterBackgroundItem());

            _options.WriteIndented = true;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            // todo
            // typeof(Package).IsAssignableFrom(typeToConvert); // inherited types
            return true;
        }

        public override Package Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Package pack = new Package();
            pack.Clear();

            string name;
            string value;

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            #region property configuration 
            // Configuration name
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            name = reader.GetString();
            if (name != Package.Keys[0])
            {
                throw new JsonException();
            }
            // Configuration value
            reader.Read();
            value = JsonSerializer.Deserialize<string>(ref reader, _options);
            if (value != Package.ConfigurationValues[0] &&
                value != Package.ConfigurationValues[1])
            {
                throw new JsonException();
            }
            pack.Configuration = value;
            #endregion

            #region property PublicKey
            // PublicKey name
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            name = reader.GetString();
            if (name != Package.Keys[1])
            {
                throw new JsonException();
            }
            // PublicKey value
            reader.Read();
            pack.UsePublicKey = JsonSerializer.Deserialize<bool>(ref reader, _options);
            #endregion

            #region property DateTimeCenter
            // datetimecenter name
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            name = reader.GetString();
            if (name != Package.Keys[2])
            {
                throw new JsonException();
            }
            // datetimecenter value
            reader.Read();
            pack.DateTimeCenter = JsonSerializer.Deserialize<DateTime>(ref reader, _options);
            #endregion

             // dictionary propertyname
            if (reader.Read()  &&
                reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            if (reader.Read() &&
                reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        #region keyvalue pair
                        if (reader.Read() &&
                            reader.TokenType == JsonTokenType.PropertyName)
                        {
                            // iterator each KeyValuePair and create object and save it in dictionary
                            // each pair looks like this:
                            // ("key", { "object type name", "object as json string" }
                            // ("BackgroundItemWall", { "BackgroundItem", "json string of item .."

                            string key = reader.GetString(); // "key"
                            if (reader.Read()) // consume token: "object begin"
                            {
                                if (reader.Read())
                                {
                                    string stype = reader.GetString();  // "object type name"
                                    var type = Type.GetType(stype);
                                    if (type == null)
                                    {
                                        // try another assembly
                                        type = CwaIsolatedStorage.ConvertCommon.GetType(stype);

                                        if (type == null)
                                        {
                                            // unknown type in any of the loaded assemblies
                                            throw new JsonException();
                                        }
                                    }

                                    if (reader.Read())
                                    {
                                        if (type != null)
                                        {
                                            // "object as json string"
                                            var result = JsonSerializer.Deserialize(
                                                ref reader,
                                                type,
                                                _options);

                                            pack.Properties.Add(key, result);
                                        }
                                        else
                                        {

                                        }
                                    }

                                    reader.Read(); // consume token: "object end"
                                }
                            }
                        }
                        #endregion
                    }
                }
            }

            if (reader.Read() &&
                reader.TokenType == JsonTokenType.EndObject)
            {
                return pack;
            }

            throw new JsonException();
        }

        protected virtual bool IsCwaNoteBase(object obj) =>
            obj as CwaNoteBase != null;

        // Projection
        private string GetJSONPropertyName(string member)
        {
            MemberInfo[] mbInfos = typeof(Package).GetMembers();

            foreach (MemberInfo element in mbInfos)
            {
                if (element.Name == member)
                {
                    Type tAttr = typeof(JsonPropertyNameAttribute);

                    JsonPropertyNameAttribute attr = (JsonPropertyNameAttribute)
                        Attribute.GetCustomAttribute(element, tAttr, false);

                    return attr != null ? attr.Name : member;
                }
            }

            return member;
        }

        public override void Write(Utf8JsonWriter writer, Package package, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
                //todo GetJSONPropertyName("get_Configuration")
                writer.WritePropertyName(Package.Keys[0]);
                JsonSerializer.Serialize(writer, package.Configuration, _options);

                writer.WritePropertyName(Package.Keys[1]);
                JsonSerializer.Serialize(writer, package.UsePublicKey, _options);

                writer.WritePropertyName(Package.Keys[2]);
                JsonSerializer.Serialize(writer, package.DateTimeCenter, _options);

                writer.WritePropertyName("Properties"); // key
                    writer.WriteStartArray(); // value

                    foreach (var kvp in package.Properties)
                    {
                        if (IsCwaNoteBase(kvp.Value))
                            continue;

                        writer.WriteStartObject();
                            writer.WritePropertyName(kvp.Key.ToString()); // key

                                writer.WriteStartObject(); // value
                                    writer.WritePropertyName((kvp.Value).GetType().FullName); // key

                                    JsonSerializer.Serialize(writer, kvp.Value, _options); // value
                                writer.WriteEndObject();

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }

    public class ConverterBackgroundItem : JsonConverter<CommonSb.BackgroundItemClickable>
    {
        public override CommonSb.BackgroundItemClickable Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            // placeholder

            CommonSb.BackgroundItemClickable bItem = 
                JsonSerializer.Deserialize<CommonSb.BackgroundItemClickable>(ref reader, options);
            
            return bItem;
        }

        public override void Write(
            Utf8JsonWriter writer,
            CommonSb.BackgroundItemClickable bitem,
            JsonSerializerOptions options)
        {
            // placeholder

            string s = JsonSerializer.Serialize<CommonSb.BackgroundItemClickable>(bitem, options);
            writer.WriteStringValue(s);
        }
    }

    public class ConverterGeoCoordinate : JsonConverter<NETStandardLocation.GeoCoordinate>
    {
        public override NETStandardLocation.GeoCoordinate Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            NETStandardLocation.GeoCoordinate geo = new NETStandardLocation.GeoCoordinate();

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
            NETStandardLocation.GeoCoordinate geo,
            JsonSerializerOptions options)
        {
            IFormatProvider ifp = CultureInfo.InvariantCulture.NumberFormat;
            string g = "[" + geo.Longitude.ToString(ifp) + "," +
                             geo.Latitude.ToString(ifp) + "," +
                             geo.Altitude.ToString(ifp) + "]";
            writer.WriteStringValue(g);
        }
    }
}
