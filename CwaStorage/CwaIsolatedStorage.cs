
#if CWAISOLATEDSTORAGE_ISF
using System.IO.IsolatedStorage;
#endif

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using dict = System.Collections.Generic.Dictionary<string, object>;

namespace CwaIsolatedStorage
{
    // http://developer.nokia.com/community/wiki/Introduction_and_best_practices_for_IsolatedStorageSettings

    // https://docs.microsoft.com/en-us/dotnet/standard/io/isolated-storage


    public static class IsfCommon
    {
#if CWAISOLATEDSTORAGE_ISF
        //private static IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
        //                                         IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
        //                                                                      IsolatedStorageScope.Domain |
        //                                                                      IsolatedStorageScope.Assembly, null, null);
        //    IsolatedStorageFile.GetStore(IsolatedStorageScope.Application |
        //                                 IsolatedStorageScope.User, null);
#else
        public static string isf = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif

        public static void SaveTextureToIso(string file, Texture2D tex)
        {
#if CWAISOLATEDSTORAGE_ISF
            //if(RemoveFileFromIso(fileNameIsoStore))
            //{
            //    Debug.WriteLine($"Info: SaveTextureToIso removed {fileNameIsoStore}");
            //}

            //using (IsolatedStorageFileStream stream = 
            //    new IsolatedStorageFileStream(fileNameIsoStore, FileMode.Create, isf))
            //{
            //    //textureSaved.SaveAsJpeg(fileStream, textureSaved.Width, textureSaved.Height); // no alpha with jpeg
            //    textureSaved.SaveAsPng(stream, textureSaved.Width, textureSaved.Height);
            //}
#else
            file = Path.Combine(isf, file);

            using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                tex.SaveAsPng(stream, tex.Width, tex.Height);
            }
#endif
        }

        public static long FileSize(string file)
        {
            long size = -1L;
#if CWAISOLATEDSTORAGE_ISF
            //if (isf.FileExists(filenameIsoStore))
            //{
            //    // there is a reflection version, but this is shorter, most likly slower
            //    using (IsolatedStorageFileStream stream = 
            //        new IsolatedStorageFileStream(filenameIsoStore, FileMode.Open, FileAccess.Read, isf))
            //    {
            //        size = stream.Length;
            //    }
            //}

            //return size;
#else
            file = Path.Combine(isf, file);
            FileInfo fi = new FileInfo(file);
            if (fi.Exists)
            {
                size = fi.Length;
            }

            return size;
#endif
        }

#if CWAISOLATEDSTORAGE_ISF
        //public static bool FileExists(string filenameIsoStore)
        //    => isf.FileExists(filenameIsoStore);
#else
        public static bool FileExists(string file)
        {
            file = Path.Combine(isf, file);
            FileInfo fi = new FileInfo(file);

            return fi.Exists;
        }
#endif
        public static bool RemoveFileFromIso(string file)
        {
#if CWAISOLATEDSTORAGE_ISF
            //if (isf.FileExists(filenameIsoStore))
            //{
            //    isf.DeleteFile(filenameIsoStore);

            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
#else
            if (FileExists(file))
            {
                file = Path.Combine(isf, file);
                File.Delete(file);

                return true;
            }
            else
            {
                return false;
            }
#endif
        }

        public static Texture2D LoadTextureFromIso(GraphicsDevice gd, string file, Rectangle r)
        {
#if CWAISOLATEDSTORAGE_ISF
            //if (!isf.FileExists(fileNameIsoStore))
            //{
            //    //throw new ArgumentException();
            //    return null;
            //}

            //Texture2D tex = null;
            ////Texture2D tex = new Texture2D(gd, (int)rec.Width, (int)rec.Height);

            //using (IsolatedStorageFileStream fileStream = 
            //    new IsolatedStorageFileStream(fileNameIsoStore, FileMode.Open, isf))
            //{
            //    tex = Texture2D.FromStream(gd, fileStream);
            //}

            //return tex;
#else

            if (!FileExists(file))
            {
                return null;
            }

            file = Path.Combine(isf, file);

            Texture2D tex = new Texture2D(gd, (int)r.Width, (int)r.Height);

            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                tex = Texture2D.FromStream(gd, stream);
            }

            return tex;
#endif
        }
    }

    /// Helper class is needed because IsolatedStorageProperty is generic and 
    /// cannot provide singleton  for static content
    [Serializable]
    public static class IsfProperties
    {
        [NonSerialized]
        public static readonly object ThreadLocker = new object();
        [NonSerialized]
        private static readonly string fname = "App.bin";
        [NonSerialized]
        private static readonly string typNote = "Cwa.CwaNote"; // todo

        public static dict App;

        static IsfProperties()
        {
            if (!LoadTry<dict>())
                App = new dict();
        }

        public static object Get(string key) =>
            App.ContainsKey(key) ? App[key] : null;

        public static bool LoadTry<T>(string key = "")
        {
            object obj = null;
            bool isApp = IsApp<T>();

            lock (ThreadLocker)
            {
                string file = isApp ? fname : key;

                file = Path.Combine(IsfCommon.isf, file);

                if (IsfCommon.FileExists(file))
                {
                    string str = "";

                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);

                        int os = 0; // todo
                        while(true)
                        {
                            int bc = fs.Read(b, 0, b.Length);
                            if (bc < 1)
                                break;

                            str += (temp.GetString(b, os, bc));
                            os += bc;
                        }
                    }

                    obj = JsonSerializer.Deserialize(str, typeof(T), ConvertCommon.jsOptionsSerialize);

                    if (obj != null)
                    {
                        if (isApp)
                            App = (dict)obj;
                        else
                            App[key] = (T)obj;

                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsApp<T>()
            => typeof(T).FullName == typeof(dict).FullName;

        private static bool IsNote<T>()
            => typeof(T).FullName == typNote;

        public static void Save<T>(string key, object obj)
        {
            lock (ThreadLocker)
            {
                string file = fname;

                if (IsNote<T>())
                    file = key;
                else
                {
                    App[key] = (T)obj;

                }

                file = Path.Combine(IsfCommon.isf, file);

                string json = JsonSerializer.Serialize<dict>(App, ConvertCommon.jsOptionsSerialize); //Serialize(App, ConvertCommon.jsOptions);

                using (FileStream fs = File.Create(file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(json);
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        public static bool Remove(string key)
        {
            if (!App.ContainsKey(key))
                return false;

            lock (ThreadLocker)
            {
                if (App[key].GetType().FullName == typNote)
                {
                    string file = Path.Combine(IsfCommon.isf, key);
                    IsfCommon.RemoveFileFromIso(file);
                }

                return App.Remove(key);
            }
        }
    }

    public class IsfProperty<T>
    {
        private readonly string _key;
        private readonly object _valueDef;

        public IsfProperty(string key, T valueDef)
        {
            _key = key;
            _valueDef = valueDef;
        }

        public bool Exists
        {
            get
            {
                return IsfProperties.Get(_key) != null;
            }
        }

        public T Value
        {
            get
            {
                if (!Exists)
                    lock (IsfProperties.ThreadLocker)
                    {
                        if (!Exists)
                            SetDefault();
                    }

                return (T)IsfProperties.Get(_key);
            }

            set
            {
                IsfProperties.Save<T>(_key, value);
            }
        }

        public void SetDefault()
        {
            Value = (T)_valueDef;
        }

        public T GetDefault()
        {
            return (T)_valueDef;
        }

        public bool Remove()
        {
            return IsfProperties.Remove(_key);
        }
    }
}