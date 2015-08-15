namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Base sealed class for all curve editor commands.
    /// </summary>
    public abstract class CurveEditorCommand : IUndoable {
        public CurveEditorCommand(CurveEditorControl2 control) {
            Control = control;
        }

        public CurveEditorControl2 Control { get; private set; }
        public abstract bool CanUndo { get; }
        // Might be abstract but not needed yet.
        public bool CanRedo { get { return true; } }
        public abstract void Do();
        public abstract void Undo();
    }
}
