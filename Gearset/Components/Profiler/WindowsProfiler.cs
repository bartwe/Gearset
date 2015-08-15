#if WINDOWS
using System;
using System.Windows;
using Microsoft.Xna.Framework;

namespace Gearset.Components.Profiler {
    sealed class WindowsProfiler : Profiler {
        //Circle buffer for WPF window summary - flip flop between two lists to break item source binding and force refresh
        bool _prifilerWindowLocationChanged;

        internal WindowsProfiler() {
            CreateProfilerWindow();
        }

        internal ProfilerWindow Window { get; private set; }

        void CreateProfilerWindow() {
            Window = new ProfilerWindow {
                Top = Config.Top,
                Left = Config.Left,
                Width = Config.Width,
                Height = Config.Height
            };

            Window.IsVisibleChanged += ProfilerIsVisibleChanged;

            WindowHelper.EnsureOnScreen(Window);

            if (Config.Visible)
                Window.Show();

            Window.LocationChanged += ProfilerLocationChanged;
            Window.SizeChanged += ProfilerSizeChanged;

            Window.TrLevelsListBox.DataContext = TimeRuler.Levels;
            Window.PgLevelsListBox.DataContext = PerformanceGraph.Levels;
            Window.PsLevelsListBox.DataContext = ProfilerSummary.Levels;
        }

        void ProfilerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Config.Visible = Window.IsVisible;
        }

        protected override void OnVisibleChanged() {
            if (Window != null)
                Window.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
        }

        void ProfilerSizeChanged(object sender, SizeChangedEventArgs e) {
            _prifilerWindowLocationChanged = true;
        }

        void ProfilerLocationChanged(object sender, EventArgs e) {
            _prifilerWindowLocationChanged = true;
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (_prifilerWindowLocationChanged) {
                _prifilerWindowLocationChanged = false;
                Config.Top = Window.Top;
                Config.Left = Window.Left;
                Config.Width = Window.Width;
                Config.Height = Window.Height;
            }
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            // Just to make sure we're only doing this one per frame.
            if (GearsetResources.CurrentRenderPass != RenderPass.BasicEffectPass)
                return;

            //Creates lots of garbage
            //RefreshTimingSummary();
        }
    }
}

#endif
