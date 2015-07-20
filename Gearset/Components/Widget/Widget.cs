using System;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    class ActionItem {
        internal Action Action;
        internal String Name;

        public override string ToString() {
            return Name;
        }
    }

    public class Widget : Gear {
        readonly ObservableCollection<ActionItem> _buttonActions;
        int _initialPositionSetDelay = 3;

        public Widget()
            : base(new GearConfig()) {
            Window = new WidgetWindow();
            Window.DataContext = GearsetSettings.Instance;
            GearsetResources.GameWindow.Move += GameWindow_Move;
            GearsetResources.GameWindow.GotFocus += GameWindow_GotFocus;
            GearsetResources.GameWindow.VisibleChanged += GameWindow_VisibleChanged;

            // Data bind to action buttons.
            _buttonActions = new ObservableCollection<ActionItem>();
            Window.ButtonList.ItemsSource = _buttonActions;
        }

        internal WidgetWindow Window { get; private set; }

        public override void Update(GameTime gameTime) {
            if (_initialPositionSetDelay > 0) {
                _initialPositionSetDelay--;
                GameWindow_Move(this, null);
            }
            base.Update(gameTime);
        }

        void GameWindow_VisibleChanged(object sender, EventArgs e) {
            Window.Visibility = GearsetResources.GameWindow.Visible ? Visibility.Visible : Visibility.Hidden;
        }

        void GameWindow_GotFocus(object sender, EventArgs e) {
            Window.Topmost = true;
            Window.Topmost = false;
        }

        void GameWindow_Move(object sender, EventArgs e) {
            var form = GearsetResources.GameWindow;
            Window.Top = form.Top - Window.Height;
            Window.Left = GearsetResources.GameWindow.Left + 20;
        }

        public void AddAction(string name, Action action) {
            // Search for an action with that name.
            for (var i = 0; i < _buttonActions.Count; i++) {
                if (_buttonActions[i].Name == name) {
                    _buttonActions[i].Action = action;
                    return;
                }
            }
            // New action
            if (name != null && action != null)
                _buttonActions.Add(new ActionItem { Name = name, Action = action });
        }
    }
}
