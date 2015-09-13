using System;
using System.ComponentModel;
using Gearset.UI;
using Microsoft.Xna.Framework;

namespace Gearset.Components.Data {
    public sealed class Plot : Window
#if WINDOWS
        , INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(String name) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
#else
    {
#endif

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Plot" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible {
            get { return _visible; }
            set {
                _visible = value;
                if (VisibleChanged != null)
                    VisibleChanged(this, EventArgs.Empty);
#if WINDOWS
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Visible"));
#endif
            }
        }

        bool _visible;

        internal event EventHandler VisibleChanged;


        public DataSampler Sampler { get; private set; }
        internal String TitleLabelName;

        internal String MinLabelName;
        internal String MaxLabelName;

        internal float Min;
        internal float Max;

        public Plot(DataSampler sampler, Vector2 position, Vector2 size)
            : base(position, size) {
            TitleBarSize = 14;
            Sampler = sampler;
            Visible = true;
            TitleLabelName = "__Plot" + sampler.Name;
            MinLabelName = "lo" + TitleLabelName;
            MaxLabelName = "hi" + TitleLabelName;
        }
    }
}
