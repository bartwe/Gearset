using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Gearset.Components.InspectorWPF {
    public sealed class MethodParamContainer : INotifyPropertyChanged {
        ItemsControl _uiContainer;
        InspectorNode _parameter;

        public ItemsControl UiContainer {
            get { return _uiContainer; }
            set {
                if (value != null) {
                    var node = value as TreeViewItem;
                    node.AllowDrop = true;
                    node.Drop += node_Drop;
                    _uiContainer = value;
                }
            }
        }

        public String ParameterName { get { return (Parameter != null) ? Parameter.Name : "(no parameter set)"; } }
        public String ParameterType { get { return ParameterInfo.ParameterType.Name; } }
        public ParameterInfo ParameterInfo { get; set; }

        public InspectorNode Parameter {
            get { return _parameter; }
            set {
                _parameter = value;
                OnPropertyChanged("ParameterName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(String propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        void node_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(ParameterInfo.ParameterType)) {
                Parameter = e.Data.GetData(ParameterInfo.ParameterType) as InspectorNode;
            }
        }
    }

    public sealed class MethodParamContainer<T> {
        public T RealParameter;
        public Object Parameter { get { return RealParameter; } set { RealParameter = (T)value; } }
    }
}
