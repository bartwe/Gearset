using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Xna.Framework;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// ViewModel sealed class used to bind WPF controls to CurveEditorControl properties
    /// and methods.
    /// </summary>
    public sealed class CurveEditorControlsViewModel : INotifyPropertyChanged {
        public CurveEditorControlsViewModel(CurveEditorControl2 control) {
            Control = control;
            Control.ToolModeChanged += Control_ToolModeChanged;
            Control.SelectionChanged += Control_SelectionChanged;
            Control.SelectedKeysMoved += Control_SelectedKeysMoved;
        }

        /// <summary>
        /// Gets the control associated with this view-model.
        /// </summary>
        public CurveEditorControl2 Control { get; private set; }

        public bool AddKeys { get { return Control.ToolMode == ToolMode.AddKeys; } set { Control.ToolMode = value ? ToolMode.AddKeys : ToolMode.SelectMove; } }
        public bool ScaleKeys { get { return Control.ToolMode == ToolMode.ScaleKeys; } set { Control.ToolMode = value ? ToolMode.ScaleKeys : ToolMode.SelectMove; } }
        public bool ZoomBox { get { return Control.ToolMode == ToolMode.ZoomBox; } set { Control.ToolMode = value ? ToolMode.ZoomBox : ToolMode.SelectMove; } }
        public ICommand SmoothCommand { get { return new SetBothTangentsCommand(Control, this, KeyTangentMode.Smooth); } }

        public bool SmoothIn {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentInMode == KeyTangentMode.Smooth); }
            set {
                Control.SetTangentModes(KeyTangentMode.Smooth, null);
                OnTangentChanged();
            }
        }

        public bool SmoothOut {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentOutMode == KeyTangentMode.Smooth); }
            set {
                Control.SetTangentModes(null, KeyTangentMode.Smooth);
                OnTangentChanged();
            }
        }

        public ICommand LinearCommand { get { return new SetBothTangentsCommand(Control, this, KeyTangentMode.Linear); } }

        public bool LinearIn {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentInMode == KeyTangentMode.Linear); }
            set {
                Control.SetTangentModes(KeyTangentMode.Linear, null);
                OnTangentChanged();
            }
        }

        public bool LinearOut {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentOutMode == KeyTangentMode.Linear); }
            set {
                Control.SetTangentModes(null, KeyTangentMode.Linear);
                OnTangentChanged();
            }
        }

        public ICommand FlatCommand { get { return new SetBothTangentsCommand(Control, this, KeyTangentMode.Flat); } }

        public bool FlatIn {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentInMode == KeyTangentMode.Flat); }
            set {
                Control.SetTangentModes(KeyTangentMode.Flat, null);
                OnTangentChanged();
            }
        }

        public bool FlatOut {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentOutMode == KeyTangentMode.Flat); }
            set {
                Control.SetTangentModes(null, KeyTangentMode.Flat);
                OnTangentChanged();
            }
        }

        public ICommand CustomCommand { get { return new SetBothTangentsCommand(Control, this, KeyTangentMode.Custom); } }

        public bool CustomIn {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentInMode == KeyTangentMode.Custom); }
            set {
                Control.SetTangentModes(KeyTangentMode.Custom, null);
                OnTangentChanged();
            }
        }

        public bool CustomOut {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.TangentOutMode == KeyTangentMode.Custom); }
            set {
                Control.SetTangentModes(null, KeyTangentMode.Custom);
                OnTangentChanged();
            }
        }

        public bool Stepped {
            get { return Control.Selection.Count > 0 && Control.Selection.All(k => k.Key.Continuity == CurveContinuity.Step); }
            set {
                Control.SetKeysContinuity(value ? CurveContinuity.Step : CurveContinuity.Smooth);
                OnPropertyChanged("Stepped");
            }
        }

        public ICommand SetTangentsCommand { get { return new SetBothTangentsCommand(Control, this, KeyTangentMode.Custom); } }

        public String StatusText {
            get {
                if (Control.Selection.Count == 0)
                    return "Nothing selected";
                if (Control.Selection.Count == 1)
                    return String.Format("Key #{0} of {1} selected", Control.Selection[0].GetIndex(), Control.Selection[0].Curve.Name);
                return String.Format("{0} keys selected", Control.Selection.Count);
            }
        }

        public Object KeyPosition {
            get {
                if (Control.Selection.Count == 1)
                    return Control.Selection[0].Key.Position;
                if (Control.Selection.Count == 0)
                    return float.NaN;

                // Multiple keys are selected, check if all of them have the same
                // position, otherwise return NaN.
                var pos = Control.Selection[0].Key.Position;
                foreach (var k in Control.Selection) {
                    if (k.Key.Position != pos) {
                        return float.NaN;
                    }
                }
                // All keys are equal
                return pos;
            }
            set {
                if (!(value is float))
                    return;
                var pos = (float)value;
                if (float.IsNaN(pos))
                    return;
                if (Control.Selection.Count == 1)
                    Control.Selection[0].MoveKey(pos - Control.Selection[0].Key.Position, 0);
                else {
                    // Move all keys to the same position.
                    foreach (var key in Control.Selection) {
                        key.MoveKey(pos - key.Key.Position, 0);
                    }
                }
                Control.InvalidateVisual();
            }
        }

        public Object KeyValue {
            get {
                if (Control.Selection.Count == 1)
                    return Control.Selection[0].Key.Value;
                if (Control.Selection.Count == 0)
                    return float.NaN;

                // Multiple keys are selected, check if all of them have the same
                // position, otherwise return NaN.
                var val = Control.Selection[0].Key.Value;
                foreach (var k in Control.Selection) {
                    if (k.Key.Value != val) {
                        return float.NaN;
                    }
                }
                // All keys are equal
                return val;
            }
            set {
                if (!(value is float))
                    return;
                var val = (float)value;
                if (float.IsNaN(val))
                    return;
                if (Control.Selection.Count == 1)
                    Control.Selection[0].MoveKey(0, val - Control.Selection[0].Key.Value);
                else {
                    // Move all keys to the same position.
                    foreach (var key in Control.Selection) {
                        key.MoveKey(0, val - key.Key.Value);
                    }
                }
                Control.InvalidateVisual();
            }
        }

        public ICommand ZoomIn { get { return new ZoomCommand(Control, this, 1, 1); } }
        public ICommand ZoomOut { get { return new ZoomCommand(Control, this, -1, -1); } }
        public ICommand ZoomInX { get { return new ZoomCommand(Control, this, 1, 0); } }
        public ICommand ZoomInY { get { return new ZoomCommand(Control, this, 0, 1); } }
        public ICommand ZoomOutX { get { return new ZoomCommand(Control, this, -1, 0); } }
        public ICommand ZoomOutY { get { return new ZoomCommand(Control, this, 0, -1); } }
        public ICommand ZoomAllCommand { get { return new ZoomExtentsCommand(Control, this, false); } }
        public ICommand ZoomSelectionCommand { get { return new ZoomExtentsCommand(Control, this, true); } }
        public event PropertyChangedEventHandler PropertyChanged;

        void Control_SelectionChanged(object sender, EventArgs e) {
            OnTangentChanged();
            OnPropertyChanged("StatusText");
            OnPropertyChanged("KeyPosition");
            OnPropertyChanged("KeyValue");
        }

        void Control_SelectedKeysMoved(object sender, EventArgs e) {
            OnPropertyChanged("KeyPosition");
            OnPropertyChanged("KeyValue");
        }

        void OnTangentChanged() {
            OnPropertyChanged("SmoothIn");
            OnPropertyChanged("SmoothOut");

            OnPropertyChanged("LinearIn");
            OnPropertyChanged("LinearOut");

            OnPropertyChanged("FlatIn");
            OnPropertyChanged("FlatOut");

            OnPropertyChanged("StepIn");
            OnPropertyChanged("StepOut");

            OnPropertyChanged("CustomIn");
            OnPropertyChanged("CustomOut");
        }

        void Control_ToolModeChanged(object sender, EventArgs e) {
            // TODO: Create a ToolModeChangedEventArgs with the previous value so we can efficiently
            // trigger OnPropertyChanged on the properties that actually changed.
            OnPropertyChanged("AddKeys");
            OnPropertyChanged("ScaleKeys");
            OnPropertyChanged("ZoomBox");
        }

        public void OnPropertyChanged(String propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public sealed class SetBothTangentsCommand : ICommand {
            readonly CurveEditorControl2 _control;
            readonly CurveEditorControlsViewModel _viewModel;
            readonly KeyTangentMode _tangentMode;

            public SetBothTangentsCommand(CurveEditorControl2 control, CurveEditorControlsViewModel viewModel, KeyTangentMode tangentMode) {
                _control = control;
                _viewModel = viewModel;
                _tangentMode = tangentMode;
                CanExecuteChanged = delegate { };
            }

            public bool CanExecute(object parameter) {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter) {
                if (parameter != null) {
                    var mode = (KeyTangentMode)Enum.Parse(typeof(KeyTangentMode), (string)parameter);
                    _control.SetTangentModes(mode, mode);
                }
                else {
                    _control.SetTangentModes(_tangentMode, _tangentMode);
                }
                _viewModel.OnTangentChanged();
            }
        }

        public sealed class ZoomExtentsCommand : ICommand {
            readonly CurveEditorControl2 _control;
            readonly bool _selectionOnly;
            CurveEditorControlsViewModel _viewModel;

            public ZoomExtentsCommand(CurveEditorControl2 control, CurveEditorControlsViewModel viewModel, bool selectionOnly) {
                _control = control;
                _viewModel = viewModel;
                _selectionOnly = selectionOnly;

                control.SelectionChanged += control_SelectionChanged;
            }

            public bool CanExecute(object parameter) {
                if (_selectionOnly)
                    return _control.Selection.Count > 0;
                return _control.Keys.Count > 0;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter) {
                float minX, maxX, minY, maxY;

                minX = minY = float.MaxValue;
                maxY = maxX = float.MinValue;

                var keys = _selectionOnly ? _control.Selection : (ICollection<KeyWrapper>)_control.Keys;

                foreach (var key in keys) {
                    minX = Math.Min(minX, key.Key.Position);
                    maxX = Math.Max(maxX, key.Key.Position);
                    minY = Math.Min(minY, key.Key.Value);
                    maxY = Math.Max(maxY, key.Key.Value);
                }

                // Show some slack (15%?)
                var slackX = (maxX - minX) * 0.15f;
                var slackY = (maxY - minY) * 0.15f;
                _control.MinX = minX - slackX;
                _control.MaxX = maxX + slackX;
                _control.MinY = minY - slackY;
                _control.MaxY = maxY + slackY;
            }

            void control_SelectionChanged(object sender, EventArgs e) {
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public sealed class ZoomCommand : ICommand {
            readonly CurveEditorControl2 _control;
            readonly float _xFactor;
            readonly float _yFactor;
            CurveEditorControlsViewModel _viewModel;

            public ZoomCommand(CurveEditorControl2 control, CurveEditorControlsViewModel viewModel, float xFactor, float yFactor) {
                _control = control;
                _viewModel = viewModel;
                _xFactor = xFactor;
                _yFactor = yFactor;
                CanExecuteChanged = delegate { };
            }

            public bool CanExecute(object parameter) {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter) {
                // Scale variables makes the zoom increase accuracy while the range
                // is small and viceversa.
                var scaleSpeed = _control.ScaleSpeed * 0.2f;
                var scaleX = _xFactor * (_control.MaxX - _control.MinX) * scaleSpeed;
                var scaleY = _yFactor * (_control.MaxY - _control.MinY) * scaleSpeed;

                _control.MinX += scaleX;
                _control.MaxX -= scaleX;

                _control.MinY += scaleY;
                _control.MaxY -= scaleY;
            }
        }
    }
}
