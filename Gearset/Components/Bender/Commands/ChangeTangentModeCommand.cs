using System.Collections.Generic;
using System.Diagnostics;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    ///     Changes the tangent value of a given key.
    /// </summary>
    public sealed class ChangeTangentModeCommand : CurveEditorCommand {
        // Saved state.
        readonly long[] _affectedKeys;
        KeyTangentMode? _newTangentInMode;
        KeyTangentMode? _newTangentOutMode;
        KeyTangentMode[] _prevTangentInMode;
        KeyTangentMode[] _prevTangentOutMode;

        /// <summary>
        ///     Creates a new command to select the given keys. You can pass null to deselect all.
        /// </summary>
        public ChangeTangentModeCommand(CurveEditorControl2 control, KeyTangentMode? newTangentInMode, KeyTangentMode? newTangentOutMode)
            : base(control) {
            // Store the parameters.
            _newTangentInMode = newTangentInMode;
            _newTangentOutMode = newTangentOutMode;

            // Store the current selection, if any.
            _affectedKeys = new long[control.Selection.Count];
            var i = 0;
            foreach (var key in control.Selection) {
                _affectedKeys[i++] = key.Id;
            }
        }

        public override bool CanUndo { get { return true; } }

        public override void Do() {
            // Do we need to save prev values?
            if (_prevTangentInMode == null) {
                Debug.Assert(_prevTangentOutMode == null);
                _prevTangentInMode = new KeyTangentMode[_affectedKeys.Length];
                _prevTangentOutMode = new KeyTangentMode[_affectedKeys.Length];

                // Save the prev values.
                for (var i = 0; i < _affectedKeys.Length; i++) {
                    _prevTangentInMode[i] = Control.Keys[_affectedKeys[i]].TangentInMode;
                    _prevTangentInMode[i] = Control.Keys[_affectedKeys[i]].TangentOutMode;
                }
            }

            var affectedCurves = new HashSet<CurveWrapper>();

            // Change the tangent modes.
            for (var i = 0; i < _affectedKeys.Length; i++) {
                if (_newTangentInMode.HasValue)
                    Control.Keys[_affectedKeys[i]].TangentInMode = _newTangentInMode.Value;
                if (_newTangentOutMode.HasValue)
                    Control.Keys[_affectedKeys[i]].TangentOutMode = _newTangentOutMode.Value;
                affectedCurves.Add(Control.Keys[_affectedKeys[i]].Curve);
            }

            // Recalc tangents on affected curves.
            foreach (var curve in affectedCurves)
                curve.ComputeTangents();

            // Redraw!
            Control.InvalidateVisual();
        }

        public override void Undo() {
            var affectedCurves = new HashSet<CurveWrapper>();

            // Revert to previous values.
            for (var i = 0; i < _affectedKeys.Length; i++) {
                if (_newTangentInMode.HasValue)
                    Control.Keys[_affectedKeys[i]].TangentInMode = _prevTangentInMode[i];
                if (_newTangentOutMode.HasValue)
                    Control.Keys[_affectedKeys[i]].TangentOutMode = _prevTangentOutMode[i];
                affectedCurves.Add(Control.Keys[_affectedKeys[i]].Curve);
            }

            // Recalc tangents on affected curves.
            foreach (var curve in affectedCurves)
                curve.ComputeTangents();

            // Redraw!
            Control.InvalidateVisual();
        }
    }
}
