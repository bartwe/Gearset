using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    public abstract class MethodCaller : INotifyPropertyChanged {
        protected MethodInfo MethodInfo;
        ItemsControl _uiContainer;

        protected MethodCaller(MethodInfo methodInfo) {
            MethodInfo = methodInfo;
            Parameters = new ObservableCollection<MethodParamContainer>();
            foreach (var par in methodInfo.GetParameters()) {
                var parameter = new MethodParamContainer { ParameterInfo = par };
                parameter.PropertyChanged += parameter_PropertyChanged;
                Parameters.Add(parameter);
            }
        }

        public String Name { get; set; }
        public bool CallAutomatically { get; set; }

        /// <summary>
        /// The TreeViewNode which holds this node.
        /// </summary>
        public ItemsControl UiContainer {
            get { return _uiContainer; }
            set {
                if (value != null) {
                    value.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
                    value.MouseDoubleClick += value_MouseDoubleClick;
                    _uiContainer = value;
                }
            }
        }

        /// <summary>
        /// Determines if all parameters have been set so the method
        /// can be invoked.
        /// </summary>
        public bool IsReady {
            get {
                var isReady = true;
                foreach (var par in Parameters)
                    if (par.Parameter == null)
                        isReady = false;
                return isReady;
            }
        }

        /// <summary>
        /// Parameters for the invocation of the method. On a especific
        /// implementation of a Method caller, the CallMethod method
        /// should use a delegate to speed things up, and of course
        /// not use this list to pass parameters but still use it to expose
        /// it to the UI.
        /// </summary>
        public ObservableCollection<MethodParamContainer> Parameters { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void value_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (IsReady)
                CallMethod();
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e) {
            foreach (var v in Parameters) {
                if (v.UiContainer == null) {
                    v.UiContainer = (ItemsControl)UiContainer.ItemContainerGenerator.ContainerFromItem(v);
                }
            }
        }

        void parameter_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "ParameterName") {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsReady"));
            }
        }

        /// <summary>
        /// Calls the method 
        /// </summary>
        public abstract void CallMethod();

        /// <summary>
        /// If the methodCaller should be called every
        /// frame, this can be done in this 
        /// </summary>
        public virtual void Update() {
            if (CallAutomatically)
                CallMethod();
        }
    }
}
