using System;
using System.ComponentModel;

namespace Gearset.Components {
    [Serializable]
    public class GearConfig : INotifyPropertyChanged {
        bool _enabled;
        bool _visible;

        public GearConfig() {
            Enabled = true;
            Visible = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Gear"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [InspectorIgnore]
        public bool Enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                if (EnabledChanged != null)
                    EnabledChanged(this, new BooleanChangedEventArgs(value));
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Gear"/> is visible.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [InspectorIgnore]
        public bool Visible {
            get { return _visible; }
            set {
                _visible = value;
                if (VisibleChanged != null)
                    VisibleChanged(this, new BooleanChangedEventArgs(_visible));
                OnPropertyChanged("Visible");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized]
        public event EventHandler<BooleanChangedEventArgs> EnabledChanged;

        [field: NonSerialized]
        public event EventHandler<BooleanChangedEventArgs> VisibleChanged;

        /// <summary>
        /// Call this method when a data-bound property changes so the UI gets notified.
        /// </summary>
        protected void OnPropertyChanged(String propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BooleanChangedEventArgs : EventArgs {
        public BooleanChangedEventArgs(bool newValue) {
            NewValue = newValue;
        }

        public bool NewValue { get; set; }
    }
}
