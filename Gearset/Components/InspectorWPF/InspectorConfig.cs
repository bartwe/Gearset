using System;

namespace Gearset.Components {
    [Serializable]
    public sealed class InspectorConfig : GearConfig {
        String _searchText;
        [NonSerialized] bool _modifiedOnly;

        public InspectorConfig() {
            // Defaults
            Width = 430;
            Height = 600;
            _searchText = String.Empty;
        }

        [InspectorIgnore]
        public double Top { get; internal set; }

        [InspectorIgnore]
        public double Left { get; internal set; }

        [InspectorIgnore]
        public double Width { get; internal set; }

        [InspectorIgnore]
        public double Height { get; internal set; }

        [InspectorIgnore]
        public String SearchText {
            get { return _searchText; }
            set {
                if (value == null)
                    return;
                _searchText = value;
                OnPropertyChanged("SearchText");
                if (SearchTextChanged != null)
                    SearchTextChanged(this, EventArgs.Empty);
            }
        }

        [InspectorIgnore]
        public bool ModifiedOnly {
            get { return _modifiedOnly; }
            set {
                _modifiedOnly = value;
                if (ModifiedOnlyChanged != null)
                    ModifiedOnlyChanged(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler ModifiedOnlyChanged;

        [field: NonSerialized]
        public event EventHandler SearchTextChanged;
    }
}
