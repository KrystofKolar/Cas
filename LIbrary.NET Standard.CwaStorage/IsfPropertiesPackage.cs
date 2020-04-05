using System;
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

        public IsfPropertiesPackage()
        {
            Init();
        }

        public virtual void Init()
        {
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
            if (CwaSystemIO.IO.FileExists(fApp))
                pack = JsonSerializer.Deserialize<Package>(GetFileJson(fApp), _optionsPackage);
            else
                pack = new Package();

            // load notes
            string[] Files = Directory.GetFiles(CwaSystemIO.IO.isf);

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
                    s = CwaSystemIO.IO.FileReadUTF8(name);

                    note = JsonSerializer.Deserialize<CwaNoteBase>(s, _optionsNoteBase);

                    pack.Properties.Add(name, note);
                }
                catch(Exception e)
                {
                    CwaSystemIO.IO.FileWriteUTF8(name + ".ERR", e.Message);
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
                    CwaSystemIO.IO.FileRemove(key);
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
                    CwaSystemIO.IO.FileWriteUTF8(key, // CwaNote as CwaNoteBase
                        JsonSerializer.Serialize<CwaNoteBase>((CwaNoteBase)obj, _optionsNoteBase));
                else
                    SavePackageAsFile();
            }
        }

        // save Package class, but without notes
        protected virtual void SavePackageAsFile() =>
            SaveFileJson(JsonSerializer.Serialize<Package>(pack, _optionsPackage));
    }
}
