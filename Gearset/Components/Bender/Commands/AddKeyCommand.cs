using Microsoft.Xna.Framework;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Selects the provided set of keys.
    /// </summary>
    public sealed class AddKeyCommand : CurveEditorCommand {
        readonly long _curveId;
        readonly float _position;
        readonly float _value;

        /// <summary>
        /// Creates a new command to select the given keys. You can pass null to deselect all.
        /// </summary>
        public AddKeyCommand(CurveEditorControl2 control, long curveId, float position, float value)
            : base(control) {
            _curveId = curveId;
            KeyId = -1;
            _position = position;
            _value = value;
        }

        public long KeyId { get; set; }
        public override bool CanUndo { get { return KeyId >= 0; } }

        public override void Do() {
            KeyWrapper wrapper;
            if (KeyId < 0) {
                wrapper = Control.Curves[_curveId].AddKey(new CurveKey(_position, _value));
                KeyId = wrapper.Id;
            }
            else {
                wrapper = Control.Curves[_curveId].AddKey(new CurveKey(_position, _value), KeyId);
            }
        }

        public override void Undo() {
            Control.Curves[_curveId].RemoveKey(KeyId);
        }
    }
}
