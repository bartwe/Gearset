using System.Collections.Generic;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Selects the provided set of keys.
    /// </summary>
    public class DeleteKeysCommand : CurveEditorCommand {
        List<KeyWrapper> _deletedKeys;

        /// <summary>
        /// Creates a new command to select the given keys. You can pass null to deselect all.
        /// </summary>
        public DeleteKeysCommand(CurveEditorControl2 control)
            : base(control) {}

        public override bool CanUndo { get { return _deletedKeys != null; } }

        public override void Do() {
            // Store the keys to be removed
            if (_deletedKeys == null) {
                _deletedKeys = new List<KeyWrapper>();
                foreach (var key in Control.Selection) {
                    _deletedKeys.Add(key);
                }
            }
            // Delete them.
            foreach (var key in _deletedKeys) {
                key.Curve.RemoveKey(key.Id);
            }
            // Remove the reference from the selection
            Control.Selection.Clear();
        }

        public override void Undo() {
            // Restore the keys removed.
            foreach (var key in _deletedKeys) {
                key.Curve.RestoreKey(key);
                Control.Selection.Add(key);
            }
        }
    }
}
