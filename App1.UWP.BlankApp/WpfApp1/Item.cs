using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GfxItem
{
    public class Item : INotifyPropertyChanged
    {
        private int id = -1;
        public int Id 
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Id"));
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
