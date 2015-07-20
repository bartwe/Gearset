namespace Gearset {
    public interface IUndoable {
        bool CanUndo { get; }
        bool CanRedo { get; }

        /// <summary>
        /// Does what the command does
        /// </summary>
        void Do();

        /// <summary>
        /// Reverts the effect of do
        /// </summary>
        void Undo();
    }
}
