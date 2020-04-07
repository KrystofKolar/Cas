
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



    /// Helper class is needed because IsolatedStorageProperty is generic and 
    /// cannot provide singleton  for static content
    public abstract class IsfProperties
    {
        public static readonly object ThreadLocker = new object();
 
        public static string fApp;

        public abstract object Get(string key);
        public abstract void Save<T>(string key, object val);
        public abstract bool Exists(string key);
        public abstract void Remove(string key);

        protected virtual string GetFileJson(string file)
        {
            lock (ThreadLocker)
            {
                file = Path.Combine(CwaSystemIO.IO.isf, file);

                return CwaSystemIO.IO.FileReadUTF8(file);
            }
        }

        protected virtual void SaveFileJson(string str)
        {
            lock (ThreadLocker)
            {
                string file = Path.Combine(CwaSystemIO.IO.isf, fApp);
                file = fApp;
                CwaSystemIO.IO.FileWriteUTF8(file, str);
            }
        }
    }

    public class IsfPropertyBase
    {
        // adapter
        public static IsfProperties props;
    }

    // abstraction for a dictionary
    public class IsfProperty<T> : IsfPropertyBase
    {
        private readonly string _key;
        private readonly object _def;

        public IsfProperty(string key, T def)
        {
            _key = key;
            _def = def;

            if (!props.Exists(key))
                props.Save<T>(_key, _def);
        }

        public bool Exists
        {
            get
            {
                return props.Exists(_key); ;
            }
        }

        public T Value
        {
            get
            {
                if (!Exists)
                {
                    lock (IsfProperties.ThreadLocker)
                    {
                        if (!Exists)
                            SetDefault();
                    }
                }

                return (T)props.Get(_key);
            }

            set
            {
                props.Save<T>(_key, value);
            }
        }

        public void SetDefault()
        {
            Value = (T)_def;
        }

        public T GetDefault()
        {
            return (T)_def;
        }

        public void Remove()
        {
            props.Remove(_key);
        }
    }
}