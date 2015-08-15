using System.Collections.Generic;
using System.Diagnostics;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Selects the provided set of keys.
    /// </summary>
    public sealed class SelectKeysCommand : CurveEditorCommand {
        readonly long[] _newSelection;
        long[] _previousSelection;

        /// <summary>
        /// Creates a new command to select the given keys. You can pass null to deselect all.
        /// </summary>
        public SelectKeysCommand(CurveEditorControl2 control, IList<long> keysToSelect)
            : base(control) {
            var count = 0;
            // Store the new selection, if any.
            if (keysToSelect != null)
                count = keysToSelect.Count;
            _newSelection = new long[count];
            for (var i = 0; i < count; i++) {
                _newSelection[i] = keysToSelect[i];
            }
        }

        public override bool CanUndo { get { return _previousSelection != null; } }

        public override void Do() {
            // Save the previous selection before we make the change.
            if (_previousSelection == null) {
                _previousSelection = new long[Control.Selection.Count];
                for (var i = 0; i < Control.Selection.Count; i++) {
                    _previousSelection[i] = Control.Selection[i].Id;
                }
            }

            // Change the selection
            Control.Selection.Clear();
            for (var i = 0; i < _newSelection.Length; i++) {
                Control.Selection.Add(Control.Keys[_newSelection[i]]);
            }
        }

        public override void Undo() {
            Debug.Assert(_previousSelection != null, "Inconsistent state.");

            // Change the selection
            Control.Selection.Clear();
            for (var i = 0; i < _previousSelection.Length; i++) {
                Control.Selection.Add(Control.Keys[_previousSelection[i]]);
            }
        }
    }
}
