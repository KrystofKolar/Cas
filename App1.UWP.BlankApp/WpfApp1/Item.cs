using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace GfxItem
{
    public class Item : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error
        {
            get {
                return null;  
            }
        }

        public string this[string propertyname]
        {
            get
            {
                if (propertyname == "Id")
                {
                    if (id == 123)
                    {
                        Debug.WriteLine($"Property IDataErrorInfo check id={id} ERROR.");
                        return "123 als id ist nicht erlaubt";
                    }
                }

                Debug.WriteLine($"Property IDataErrorInfo check id={id} success.");
                return null;
            }
        }

        private int id = -1;
        public int Id 
        {
            get
            {
                return id;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Negative id not allowed");
                }
                else
                {
                    id = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Id"));
                }
            }
        }

        private string name = string.Empty;
        public string Name 
        { 
            get
            {
                return name;
            }

            set
            {
                name = value;
                Debug.WriteLine($"Name changed to {name}");
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
