using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CwaIsolatedStorage;
using CwaNotesTypes;

namespace CasStorage
{
    public class IsfPropertiesPackage : IsfProperties
    {
        public static Package pack;
        protected static JsonSerializerOptions _optionsPackage;
        protected static JsonSerializerOptions _optionsNoteBase;

        public IsfPropertiesPackage(string sApp = "YourAppname.json")
        {
            IsfProperties.fApp = sApp;

            _optionsPackage = new JsonSerializerOptions();
            _optionsPackage.Converters.Add(new ConverterPackage());
            _optionsPackage.WriteIndented = true;

            _optionsNoteBase = new JsonSerializerOptions();
            _optionsNoteBase.Converters.Add(new ConverterDateTime());
            _optionsNoteBase.Converters.Add(new ConverterVector2());
            _optionsNoteBase.Converters.Add(new ConverterEnum<CasBase.casId>());
            _optionsNoteBase.Converters.Add(new ConverterDictDateTimeString());
            _optionsNoteBase.Converters.Add(new ConverterEnum<CasBase.casTextureVariant>());

            _optionsNoteBase.WriteIndented = true;

            Load();
        }

        protected virtual void Load()
        {
            // load none notes
            if (CwaSystemIO.IO.FileExistsRelative(fApp))
            {
                try
                {
                    pack = JsonSerializer.Deserialize<Package>(GetFileJson(fApp), _optionsPackage);
                }
                catch(Exception e) //todo
                {
                    Debug.WriteLine(e.Message);
                    pack = null;
                }
            }

            if (pack == null)
            {
                pack = new Package();
                SavePackageAsFile();
            }

            // load notes
            string[] Files = Directory.GetFiles(CwaSystemIO.IO.Relative);

            string name = string.Empty;
            string s = string.Empty;
            CwaNoteBase note = null;

            foreach(var item in Files)
            {
                name = Path.GetFileName(item);

                if (!name.StartsWith(CwaNotesBase.Root.FILENAMENOTE))
                    continue;

                try
                {
                    s = CwaSystemIO.IO.FileReadUTF8(item);

                    note = JsonSerializer.Deserialize<CwaNoteBase>(s, _optionsNoteBase);

                    pack.Properties.Add(name, note);
                }
                catch(Exception e)
                {
                    CwaSystemIO.IO.FileWriteUTF8(item + ".ERR", e.Message);
                }
            }
        }

        public override bool Exists(string key) =>
            pack.Properties.ContainsKey(key);

        protected virtual bool IsCwaNoteBase(object obj) =>
            obj as CwaNoteBase != null;

        public override object Get(string key) =>
            pack.Properties[key];

        public override void Remove(string key)
        {
            lock (ThreadLocker)
            {
                object obj = pack.Properties[key];
                pack.Properties.Remove(key);

                if (IsCwaNoteBase(obj))
                    CwaSystemIO.IO.FileRemoveRelative(key);
                else
                    SavePackageAsFile();
            }
        }

        public override void Save<T>(string key, object obj)
        {
            lock (ThreadLocker)
            {
                pack.Properties[key] = (T)obj;

                if (IsCwaNoteBase(obj))
                {
                    string file = Path.Combine(CwaSystemIO.IO.Relative, key);
                    CwaSystemIO.IO.FileWriteUTF8(file, // CwaNote as CwaNoteBase
                        JsonSerializer.Serialize<CwaNoteBase>((CwaNoteBase)obj, _optionsNoteBase));
                }
                else
                    SavePackageAsFile();
            }
        }

        // save Package class, but without notes
        protected virtual void SavePackageAsFile()
        {
            string s = JsonSerializer.Serialize<Package>(pack, _optionsPackage);
            SaveFileJson(s);
        }
    }
}
