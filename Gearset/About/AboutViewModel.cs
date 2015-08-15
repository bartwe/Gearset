using System;
using System.ComponentModel;

namespace Gearset.About {
    public sealed class AboutViewModel : INotifyPropertyChanged {
        public AboutViewModel(String productNameAndVersion, string copyrightNotice) {
            ProductNameAndVersion = productNameAndVersion;
            CopyrightNotice = copyrightNotice;
        }

        public string CopyrightNotice { get; set; }
        public string ProductNameAndVersion { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(String propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
