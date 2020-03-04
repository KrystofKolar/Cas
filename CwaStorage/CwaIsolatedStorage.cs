using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;


// Get a new isolated store for this assembly and put it into an
// isolated store object.

//        IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

namespace CwaIsolatedStorage
{
    // http://developer.nokia.com/community/wiki/Introduction_and_best_practices_for_IsolatedStorageSettings

    public static class IsolatedStorageHelper
    {
        public static void SaveToIsoStore(String filenameResource)
        {
            Debug.WriteLine("TODO: !!! Disabled SaveToIsoStore, because at the time of porting not called");

            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();

            if (!isolatedStorageFile.FileExists(filenameResource))
            {
                //StreamResourceInfo resource = Application.GetResourceStream(new Uri(filenameResource, UriKind.Relative));

                using (IsolatedStorageFileStream isolatedStorageFileStream = isolatedStorageFile.CreateFile(filenameResource))
                {
                    //int chunkSize = 1024;
                    //byte[] bytes = new byte[chunkSize];
                    //int byteCount;

                    //while ((byteCount = resource.Stream.Read(bytes, 0, chunkSize)) > 0)
                    //{
                    //    isolatedStorageFileStream.Write(bytes, 0, byteCount);
                    //}
                }
            }
        }

        // create, overwrite file
        public static bool SaveTextureToIso(string fileNameIsoStore, Texture2D textureSaved)
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    //Debug.WriteLine("Delete then Save texture as jpg to Iso {0}", fileNameIsoStore);
                    RemoveFileFromIso(fileNameIsoStore);

                    using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(fileNameIsoStore, FileMode.Create, file))
                    {
                        // no alpha with jpeg
                        //textureSaved.SaveAsJpeg(fileStream, textureSaved.Width, textureSaved.Height);

                        textureSaved.SaveAsPng(fileStream, textureSaved.Width, textureSaved.Height);

                        fileStream.Close();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace.ToString());
                return false;
            }
        }

        public static long FileSize(string filenameIsoStore)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
#if (CWANETSTANDARD)            
                return file.UsedSize;
#else
                Debug.WriteLine("TODO: Size not available");
                return -1;
#endif
            }
        }

        public static bool FileExists(string filenameIsoStore)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
#if DEBUG
                bool exists = file.FileExists(filenameIsoStore);

                if (exists)
                {
                    return true;
                }
                else
                {
                    return false;
                }
#else
                return file.FileExists(filenameIsoStore);
#endif
            }
        }

        public static bool RemoveFileFromIso(string filenameIsoStore)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(filenameIsoStore))
                {
                    file.DeleteFile(filenameIsoStore);

                    return true;
                }
            }

            return false;
        }

        //todo how to get the texture dimensions
        public static Texture2D LoadTextureFromIso(GraphicsDevice gd, string fileNameIsoStore, Rectangle rec)
        {
            try
            {
                Texture2D tex = new Texture2D(gd, (int)rec.Width, (int)rec.Height);
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (file.FileExists(fileNameIsoStore))
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(fileNameIsoStore, FileMode.Open, file))
                        {
#if (CWANETSTANDARD)
                            tex = Texture2D.FromStream(gd, fileStream);
#else
                            tex = Texture2D.FromStream(gd, fileStream, (int)rec.Width, (int)rec.Height, false);
#endif
                        }

                        return tex;
                    }
                    else
                        return null;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                string s = e.StackTrace;
                Debug.WriteLine(s);
#endif
                return null;
            }
        }
    }

    /// <summary>
    /// Helper class is needed because IsolatedStorageProperty is generic and 
    /// can not provide singleton model for static content
    /// </summary>
    //internal 
    public static class IsolatedStoragePropertyHelper
    {
        /// <summary>
        /// We must use this object to lock saving settings
        /// </summary>
        public static readonly object ThreadLocker = new object();

        // Store are Key/Valuepairs
        public static readonly IsolatedStorageSettings Store = IsolatedStorageSettings.ApplicationSettings;
    }

    public class IsolatedStorageProperty<T>
    {
        readonly object _defaultValue;
        readonly string _name;
        readonly object _syncObject = new object();

        public IsolatedStorageProperty(string Propertyname, T PropertydefaultValue)
        {
            _name = Propertyname;
            _defaultValue = PropertydefaultValue;
        }

        public bool Exists
        {
            get
            {
                return IsolatedStoragePropertyHelper.Store.Contains(_name);
            }
        }

        public T Value
        {
            get
            {
                if (!Exists)//If property does not exist - initializing it using default value
                {
                    lock (_syncObject) //Initializing only once
                    {
                        if (!Exists)
                            SetDefault();
                    }
                }

                return (T)IsolatedStoragePropertyHelper.Store[_name];
            }

            set
            {
                IsolatedStoragePropertyHelper.Store[_name] = value;
                Save();
            }
        }

        private static void Save()
        {
            lock (IsolatedStoragePropertyHelper.ThreadLocker)
            {
                IsolatedStoragePropertyHelper.Store.Save();
            }
        }

        public void SetDefault()
        {
            Value = (T)_defaultValue;
        }

        public T GetDefault()
        {
            return (T)_defaultValue;
        }

        public bool Remove()
        {
            return IsolatedStoragePropertyHelper.Store.Remove(_name);
        }
    }

}