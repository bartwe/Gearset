using System.Collections.Generic;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Moves a set of keys.
    /// </summary>
    public class MoveKeysCommand : CurveEditorCommand {
        readonly long[] _affectedKeys;
        float _positionOffset;
        float _valueOffset;

        /// <summary>
        /// Creates a new command to move a set of keys.
        /// </summary>
        public MoveKeysCommand(CurveEditorControl2 control, float positionOffset, float valueOffset)
            : base(control) {
            // Store the parameters.
            _positionOffset = positionOffset;
            _valueOffset = valueOffset;

            // Store the current selection, if any.
            _affectedKeys = new long[control.Selection.Count];
            var i = 0;
            foreach (var key in control.Selection) {
                _affectedKeys[i++] = key.Id;
            }
        }

        public override bool CanUndo { get { return _affectedKeys != null; } }

        public override void Do() {
            MoveKeys(_positionOffset, _valueOffset);
        }

        public override void Undo() {
            MoveKeys(-_positionOffset, -_valueOffset);
        }

        /// <summary>
        /// This method will move will update the offset and move the keys accordingly.
        /// This is to be used while dragging, before the mouse is released and the command
        /// added (without calling Do()) to the history.
        /// </summary>
        public void UpdateOffsets(float positionOffset, float valueOffset) {
            MoveKeys(positionOffset - _positionOffset, valueOffset - _valueOffset);
            _positionOffset = positionOffset;
            _valueOffset = valueOffset;
        }

        /// <summary>
        /// Performs the actual movement of the keys.
        /// </summary>
        void MoveKeys(float positionOffset, float valueOffset) {
            var affectedCurves = new List<CurveWrapper>();
            for (var i = 0; i < _affectedKeys.Length; i++) {
                var key = Control.Keys[_affectedKeys[i]];
                key.MoveKey(positionOffset, valueOffset);

                // Create the set of curve affected by this move so we can recalculate
                // the auto tangents.
                if (!affectedCurves.Contains(key.Curve))
                    affectedCurves.Add(key.Curve);
            }

            // Compute all auto tangents.
            foreach (var curve in affectedCurves) {
                curve.ComputeTangents();
            }
        }
    }
}
