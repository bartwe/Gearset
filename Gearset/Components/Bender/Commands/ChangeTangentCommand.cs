namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Changes the tangent value of a given key.
    /// </summary>
    public class ChangeTangentCommand : CurveEditorCommand {
        readonly long _affectedKey;
        readonly TangentSelectionMode _selectedTangent;
        // Saved state.
        readonly float _prevTangentInValue;
        readonly float _prevTangentOutValue;
        readonly KeyTangentMode _prevTangentInMode;
        readonly KeyTangentMode _prevTangentOutMode;
        float _tangentValue;

        /// <summary>
        /// Creates a new command to select the given keys. You can pass null to deselect all.
        /// </summary>
        public ChangeTangentCommand(CurveEditorControl2 control, long keyId, TangentSelectionMode selectedTangent)
            : base(control) {
            // Store the parameters.
            _selectedTangent = selectedTangent;
            _affectedKey = keyId;

            var key = control.Keys[keyId];

            _prevTangentInValue = key.Key.TangentIn;
            _prevTangentOutValue = key.Key.TangentOut;
            _prevTangentInMode = key.TangentInMode;
            _prevTangentOutMode = key.TangentOutMode;
        }

        public override bool CanUndo { get { return true; } }

        public override void Do() {
            ChangeTangent(_tangentValue);
        }

        public override void Undo() {
            var key = Control.Keys[_affectedKey];

            key.SetInTangent(_prevTangentInValue);
            key.SetOutTangent(_prevTangentOutValue);
            key.TangentInMode = _prevTangentInMode;
            key.TangentOutMode = _prevTangentOutMode;
        }

        /// <summary>
        /// This method will update the offset and update the key tangent accordingly.
        /// This is to be used while dragging, before the mouse is released and the command
        /// added (without calling Do()) to the history.
        /// </summary>
        public void UpdateOffset(float tangent) {
            ChangeTangent(tangent);
            _tangentValue = tangent;
        }

        /// <summary>
        /// Performs the actual movement of the keys.
        /// </summary>
        void ChangeTangent(float tangent) {
            var key = Control.Keys[_affectedKey];
            if (_selectedTangent == TangentSelectionMode.In)
                key.SetInTangent(tangent);
            else
                key.SetOutTangent(tangent);
        }
    }
}
