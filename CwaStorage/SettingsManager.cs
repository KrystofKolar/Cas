
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Test.ConsoleApp.NETCore
{
    public class UserSettings
    {
        public string Name { get; set; }
        public bool IsValid { get; set; }
    }

    public class SettingsManager<T> where T : class
    {
        private readonly string _filePath;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, fileName);
        }

        public T LoadSettings() =>
            File.Exists(_filePath) ?
            //JsonConvert.DeserializeObject<T>(File.ReadAllText(_filePath)) :
            JsonSerializer.Deserialize<T>(File.ReadAllText(_filePath)) :
            null;

        public void SaveSettings(T settings)
        {
            //string json = JsonConvert.SerializeObject(settings);
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_filePath, json);
        }
    }
}

