using System;
using System.ComponentModel;

namespace Gearset.Components.InspectorWPF {
    public sealed class NoticeViewModel : INotifyPropertyChanged {
        String _noticeText;
        String _noticeHyperlinkText;
        String _noticeHyperlinkUrl;

        public String NoticeText {
            get { return _noticeText; }
            set {
                _noticeText = value;
                OnPropertyChanged("NoticeText");
            }
        }

        public String NoticeHyperlinkText {
            get { return _noticeHyperlinkText; }
            set {
                _noticeHyperlinkText = value;
                OnPropertyChanged("NoticeHyperlinkText");
            }
        }

        public String NoticeHyperlinkUrl {
            get { return _noticeHyperlinkUrl; }
            set {
                _noticeHyperlinkUrl = value;
                OnPropertyChanged("NoticeHyperlinkUrl");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(String propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
