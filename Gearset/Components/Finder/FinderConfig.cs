using System;

namespace Gearset.Components {
    [Serializable]
    public sealed class FinderConfig : GearConfig {
        String _searchText;
        [NonSerialized] SearchFunction _searchFunction;

        public FinderConfig() {
            // Defaults
            Width = 400;
            Height = 430;
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
                _searchText = value;
                OnPropertyChanged("SearchText");
                if (SearchTextChanged != null) SearchTextChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The function that will be called everytime the query string changes.
        /// </summary>
        public SearchFunction SearchFunction {
            get { return _searchFunction; }
            set {
                _searchFunction = value;
                OnPropertyChanged("SearchFunction");
            }
        }

        [field: NonSerialized]
        public event EventHandler SearchTextChanged;
    }
}
