using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CasStorage
{
    public class Package
    {
        public static string[] Keys =
        {
            "Configuration",
            "UsePublicKey",
            "DateTimeCenter",
        };

        // SerializeValue Configuration
        public static string[] ConfigurationValues =
        {
            "Development",
            "Productive"
        };

        public Package()
        {
            Clear();
        }

        public void Clear()
        {
            Configuration = ConfigurationValues[0];
            UsePublicKey = false;
            DateTimeCenter = DateTime.UtcNow;

            Properties = new Dictionary<string, object>();
        }

        [JsonPropertyNameAttribute("Configuration")]
        public string Configuration { get; set; }

        [JsonPropertyName("UsePublicKey")]
        public bool UsePublicKey { get; set; }

        [JsonPropertyName("DateTimeCenter")]
        public DateTime DateTimeCenter { get; set; }

        [JsonPropertyName("Properties")]
        public Dictionary<string, object> Properties { get; set; }
    }
}
