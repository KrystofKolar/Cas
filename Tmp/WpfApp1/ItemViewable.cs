using System;
using System.Collections.Generic;
using System.Text;

namespace GfxItem
{
    public class ItemViewable : Item
    {
        private bool visible;

        public bool Visible 
        { 
            get { return visible; }

            set
            {
                visible = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Visible"));
            }
        }

    }
}
