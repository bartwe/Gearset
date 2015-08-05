using System.Windows;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Scales a set of keys.
    /// </summary>
    public class ScaleKeysCommand : CurveEditorCommand {
        readonly long[] _affectedKeys;
        readonly Point[] _normalizedPos;
        readonly ScaleBoxHandle _handle;
        Point _min;
        Point _max;
        Point _newMin;
        Point _newMax;

        public ScaleKeysCommand(CurveEditorControl2 control, Point min, Point max, ScaleBoxHandle handle)
            : base(control) {
            // Store the parameters.
            _min = min;
            _max = max;
            _newMin = min;
            _newMax = max;
            _handle = handle;

            // Store the current selection, if any.
            _affectedKeys = new long[control.Selection.Count];
            _normalizedPos = new Point[control.Selection.Count];
            var i = 0;
            foreach (var key in control.Selection) {
                _affectedKeys[i] = key.Id;

                var pos = key.GetPosition();
                _normalizedPos[i].X = (pos.X - min.X) / (max.X - min.X);
                _normalizedPos[i].Y = (pos.Y - min.Y) / (max.Y - min.Y);

                i++;
            }
        }

        public override bool CanUndo { get { return _affectedKeys != null; } }

        public override void Do() {
            ScaleKeys(_newMin, _newMax);
        }

        public override void Undo() {
            ScaleKeys(_min, _max);
        }

        /// <summary>
        /// This method will move will update the offset and move the keys accordingly.
        /// This is to be used while dragging, before the mouse is released and the command
        /// added (without calling Do()) to the history.
        /// </summary>
        /// <param name="offset">offset in curve coords</param>
        public void UpdateOffsets(Point offset) {
            _newMin = _min;
            _newMax = _max;

            // Calculate the size of the new box.
            switch (_handle) {
                case ScaleBoxHandle.BottomLeft:
                    _newMin = Point.Add(_min, (Vector)offset);
                    break;
                case ScaleBoxHandle.BottomRight:
                    _newMin.Y = _min.Y + offset.Y;
                    _newMax.X = _max.X + offset.X;
                    break;
                case ScaleBoxHandle.TopRight:
                    _newMax = Point.Add(_max, (Vector)offset);
                    break;
                case ScaleBoxHandle.TopLeft:
                    _newMin.X = _min.X + offset.X;
                    _newMax.Y = _max.Y + offset.Y;
                    break;
            }
            ScaleKeys(_newMin, _newMax);
        }

        /// <summary>
        /// Performs the actual movement of the keys.
        /// </summary>
        void ScaleKeys(Point newMin, Point newMax) {
            for (var i = 0; i < _affectedKeys.Length; i++) {
                // Remap to the new box;
                var newPosition = new Point((newMax.X - newMin.X) * _normalizedPos[i].X + newMin.X, (newMax.Y - newMin.Y) * _normalizedPos[i].Y + newMin.Y);
                var k = Control.Keys[_affectedKeys[i]];
                k.MoveKey((float)(newPosition.X - k.Key.Position), (float)(newPosition.Y - k.Key.Value));
            }
        }
    }
}
