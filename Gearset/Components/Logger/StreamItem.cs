using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;

namespace Gearset.Components.Logger {
    public class StreamItem : IComparable<StreamItem>, INotifyPropertyChanged {
        Boolean _enabled = true;
        public String Name { get; set; }

        public Boolean Enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public Brush Color { get; set; }

        public int CompareTo(StreamItem other) {
            return String.Compare(Name, other.Name, true, CultureInfo.InvariantCulture);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string p) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }
    }
}