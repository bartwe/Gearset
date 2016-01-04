using System;
using System.Windows;
using System.Windows.Forms.Integration;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    ///     Lets the user search for a object in its game.
    /// </summary>
    sealed class Finder : Gear {
        float _searchDelay;
        bool _locationJustChanged;

        public Finder()
            : base(GearsetSettings.Instance.FinderConfig) {
            Config = GearsetSettings.Instance.FinderConfig;

            Window = new FinderWindow();
            ElementHost.EnableModelessKeyboardInterop(Window);
            Window.SizeChanged += Window_SizeChanged;
            Window.IsVisibleChanged += Window_IsVisibleChanged;
            Window.LocationChanged += Window_LocationChanged;
            Window.DataContext = this;
            Window.Top = Config.Top;
            Window.Left = Config.Left;
            Window.Width = Config.Width;
            Window.Height = Config.Height;

            WindowHelper.EnsureOnScreen(Window);

            if (Config.Visible)
                Window.Show();

            Config.SearchTextChanged += Config_SearchTextChanged;
            _searchDelay = .25f;
        }

        /// <summary>
        ///     WPF window instance.
        /// </summary>
        internal FinderWindow Window { get; private set; }

        public FinderConfig Config { get; private set; }

        void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Config.Visible = Window.IsVisible;
        }

        protected override void OnVisibleChanged() {
            if (Window != null) {
                Window.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
                Window.WasHiddenByGameMinimize = false;
            }
        }

        void Window_LocationChanged(object sender, EventArgs e) {
            _locationJustChanged = true;
        }

        void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            _locationJustChanged = true;
        }

        void Config_SearchTextChanged(object sender, EventArgs e) {
            _searchDelay = .25f;
        }

        public override void Update(GameTime gameTime) {
            if (_searchDelay > 0) {
                var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _searchDelay -= dt;
                if (_searchDelay <= 0 && Config.SearchText != null) {
                    _searchDelay = 0;
                    if (Config.SearchFunction != null)
                        Window.ResultsListBox.ItemsSource = Config.SearchFunction(Config.SearchText);
                    else
                        Window.ResultsListBox.ItemsSource = DefaultSearchFunction(Config.SearchText);

                    if (Window.ResultsListBox.Items.Count > 0)
                        Window.ResultsListBox.SelectedIndex = 0;
                }
            }

            if (_locationJustChanged) {
                _locationJustChanged = false;
                Config.Top = Window.Top;
                Config.Left = Window.Left;
                Config.Width = Window.Width;
                Config.Height = Window.Height;
            }
            base.Update(gameTime);
        }

        /// <summary>
        ///     The default search function. It will search through the GameComponentCollection
        ///     of the Game.
        /// </summary>
        static FinderResult DefaultSearchFunction(String queryString) {
            var result = new FinderResult();
            var searchParams = queryString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Split the query
            if (searchParams.Length == 0) {
                searchParams = new[] { String.Empty };
            }
            else {
                // Ignore case.
                for (var i = 0; i < searchParams.Length; i++)
                    searchParams[i] = searchParams[i].ToUpper();
            }

            foreach (var component in GearsetResources.Game.Components) {
                var matches = true;
                var type = component.GetType().ToString();

                if (component is GearsetComponentBase)
                    continue;

                // Check if it matches all search params.
                for (var i = 0; i < searchParams.Length; i++) {
                    if (!(component.ToString().ToUpper().Contains(searchParams[i]) || type.ToUpper().Contains(searchParams[i]))) {
                        matches = false;
                        break;
                    }
                }
                if (matches)
                    result.Add(new ObjectDescription(component, type));
            }
            return result;
        }
    }
}
