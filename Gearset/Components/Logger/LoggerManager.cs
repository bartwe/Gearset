using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Color = System.Windows.Media.Color;

namespace Gearset.Components.Logger {
    public sealed class LoggerManager : Gear {
        readonly StreamItem _defaultStream;

        readonly SolidColorBrush[] _colors = {
            new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
            new SolidColorBrush(Color.FromArgb(255, 128, 200, 200)),
            new SolidColorBrush(Color.FromArgb(255, 200, 200, 128)),
            new SolidColorBrush(Color.FromArgb(255, 200, 128, 200)),
            new SolidColorBrush(Color.FromArgb(255, 128, 128, 200)),
            new SolidColorBrush(Color.FromArgb(255, 128, 200, 128)),
            new SolidColorBrush(Color.FromArgb(255, 200, 128, 128)),
            new SolidColorBrush(Color.FromArgb(255, 150, 110, 110)),
            new SolidColorBrush(Color.FromArgb(255, 110, 150, 110)),
            new SolidColorBrush(Color.FromArgb(255, 110, 110, 150)),
            new SolidColorBrush(Color.FromArgb(255, 150, 150, 110)),
            new SolidColorBrush(Color.FromArgb(255, 150, 110, 150)),
            new SolidColorBrush(Color.FromArgb(255, 110, 150, 150))
        };

        readonly ScrollViewer _scrollViewer;
        KeyboardState _prevKbs;
        int _currentColor;
        internal ObservableCollection<StreamItem> Streams;
        internal FixedLengthQueue<LogItem> LogItems;
        internal FixedLengthQueue<LogItem> LogItemsOld;
        internal ICollectionView FilteredView;
        bool _locationJustChanged;

        public LoggerManager()
            : base(GearsetSettings.Instance.LoggerConfig) {
            // Create the back-end collections
            Streams = new ObservableCollection<StreamItem>();
            LogItems = new FixedLengthQueue<LogItem>(500);
            LogItemsOld = new FixedLengthQueue<LogItem>(10000);
            //LogItems.DequeueTarget = LogItemsOld;
            Streams.Add(_defaultStream = new StreamItem { Name = "Default", Enabled = true, Color = _colors[_currentColor++] });

            // Create the window.
            Window = new LoggerWindow();

            Window.Top = Config.Top;
            Window.Left = Config.Left;
            Window.Width = Config.Width;
            Window.Height = Config.Height;
            Window.IsVisibleChanged += logger_IsVisibleChanged;
            Window.SoloRequested += logger_SoloRequested;

            WindowHelper.EnsureOnScreen(Window);

            if (Config.Visible)
                Window.Show();

            FilteredView = CollectionViewSource.GetDefaultView(LogItems);
            FilteredView.Filter = a => ((LogItem)a).Stream.Enabled;
            FilteredView.GroupDescriptions.Add(new PropertyGroupDescription("UpdateNumber"));
            Window.LogListBox.DataContext = FilteredView;
            Window.StreamListBox.DataContext = Streams;

            Window.LocationChanged += logger_LocationChanged;
            Window.SizeChanged += logger_SizeChanged;

            _scrollViewer = GetDescendantByType(Window.LogListBox, typeof(ScrollViewer)) as ScrollViewer;
        }

        internal LoggerWindow Window { get; private set; }
        //private Brush[] BackBrushes = new Brush[2];

        public LoggerConfig Config { get { return GearsetSettings.Instance.LoggerConfig; } }

        void logger_SoloRequested(object sender, SoloRequestedEventArgs e) {
            foreach (var stream in Streams) {
                if (stream != e.StreamItem)
                    stream.Enabled = false;
                else
                    stream.Enabled = true;
            }
        }

        void logger_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Config.Visible = Window.IsVisible;
        }

        protected override void OnVisibleChanged() {
            if (Window != null)
                Window.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
        }

        void logger_SizeChanged(object sender, SizeChangedEventArgs e) {
            _locationJustChanged = true;
        }

        void logger_LocationChanged(object sender, EventArgs e) {
            _locationJustChanged = true;
        }

        public override void Update(GameTime gameTime) {
            var kbs = Keyboard.GetState();

            if (_locationJustChanged) {
                _locationJustChanged = false;
                Config.Top = Window.Top;
                Config.Left = Window.Left;
                Config.Width = Window.Width;
                Config.Height = Window.Height;
            }

            if (kbs.IsKeyDown(Keys.LeftControl) && kbs.IsKeyDown(Keys.F7) && _prevKbs.IsKeyUp(Keys.F7)) {
                if (Window.IsVisible)
                    Window.Hide();
                else {
                    Window.Show();
                }
            }
            _prevKbs = kbs;
        }

        public void EnableAllStreams() {
            foreach (var stream in Streams)
                stream.Enabled = true;
        }

        public void DisableAllStreams() {
            foreach (var stream in Streams)
                stream.Enabled = false;
        }

        /// <summary>
        ///     Shows a dialog asking for a filename and saves the log file.
        /// </summary>
        public void SaveLogToFile() {
            // Configure save file dialog box
            var dlg = new SaveFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".log"; // Default file extension
            dlg.Filter = "Log files (.log)|*.log"; // Filter files by extension

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result != true) {
                return;
            }

            // Generate the log file.
            SaveLogToFile(dlg.FileName);
        }

        /// <summary>
        ///     Saves the log to the specified file.
        /// </summary>
        /// <param name="filename">Name of the file to save the log (usually ending in .log)</param>
        public void SaveLogToFile(string filename) {
            // Generate the log file.
            using (TextWriter t = new StreamWriter(filename)) {
                var updateNumber = -1;
                var maxStreamNameSize = 0;
                foreach (var item in Streams)
                    if (item.Name.Length > maxStreamNameSize)
                        maxStreamNameSize = item.Name.Length;
                // Store old items (not shown in the Logger window).
                foreach (var item in LogItemsOld) {
                    if (item.UpdateNumber > updateNumber) {
                        t.WriteLine("Update " + item.UpdateNumber);
                        updateNumber = item.UpdateNumber;
                    }
                    t.WriteLine(item.Stream.Name.PadLeft(maxStreamNameSize) + " | " + item.Content);
                }
                // Store last n items (shown in the logger window).
                foreach (var item in LogItems) {
                    if (item.UpdateNumber > updateNumber) {
                        t.WriteLine("Update " + item.UpdateNumber);
                        updateNumber = item.UpdateNumber;
                    }
                    t.WriteLine(item.Stream.Name.PadLeft(maxStreamNameSize) + " | " + item.Content);
                }
                t.Close();
            }
        }

        StreamItem SearchStream(String name) {
            foreach (var streamItem in Streams) {
                if (streamItem.Name == name)
                    return streamItem;
            }
            return null;
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The first format parameter</param>
        public void Log(String streamName, String format, Object arg0) {
            Log(streamName, String.Format(format, arg0));
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The first format parameter</param>
        /// <param name="arg1">The second format parameter</param>
        public void Log(String streamName, String format, Object arg0, Object arg1) {
            Log(streamName, String.Format(format, arg0, arg1));
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The first format parameter</param>
        /// <param name="arg1">The second format parameter</param>
        /// <param name="arg2">The third format parameter</param>
        public void Log(String streamName, String format, Object arg0, Object arg1, Object arg2) {
            Log(streamName, String.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The format parameters</param>
        public void Log(String streamName, String format, params Object[] args) {
            Log(streamName, String.Format(format, args));
        }

        /// <summary>
        ///     Los a message to the specified stream.
        /// </summary>
        /// <param name="streamName">Name of the Stream to log the message to</param>
        /// <param name="message">Message to log</param>
        public void Log(String streamName, String message) {
            var stream = SearchStream(streamName);
            // Is it a new stream?
            if (stream == null) {
                stream = new StreamItem {
                    Enabled = !Config.HiddenStreams.Contains(streamName),
                    Name = streamName,
                    Color = _colors[(_currentColor++)]
                };
                stream.PropertyChanged += stream_PropertyChanged;
                Streams.Add(stream);

                // Repeat the colors.
                if (_currentColor >= _colors.Length)
                    _currentColor = 0;
            }

            // Log even if the stream is disabled.
            var logItem = new LogItem { Stream = stream, Content = message, UpdateNumber = GearsetResources.Console.UpdateCount };
            LogItems.Enqueue(logItem);

            if (stream.Enabled)
                _scrollViewer.ScrollToEnd();
        }

        /// <summary>
        ///     Logs the specified message in the default stream.
        /// </summary>
        /// <param name="content">The message to log.</param>
        public void Log(String message) {
            LogItems.Enqueue(new LogItem { Stream = _defaultStream, Content = message, UpdateNumber = GearsetResources.Console.UpdateCount });
        }

        static Visual GetDescendantByType(Visual element, Type type) {
            if (element == null)
                return null;
            if (element.GetType() == type)
                return element;
            Visual foundElement = null;
            if (element is FrameworkElement) {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        void stream_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            FilteredView.Refresh();
            if (e.PropertyName == "Enabled") {
                var stream = sender as StreamItem;
                if (stream == null)
                    return;

                if (stream.Enabled && Config.HiddenStreams.Contains(stream.Name))
                    Config.HiddenStreams.Remove(stream.Name);
                else if (!stream.Enabled && !(Config.HiddenStreams.Contains(stream.Name)))
                    Config.HiddenStreams.Add(stream.Name);
            }
        }
    }
}
