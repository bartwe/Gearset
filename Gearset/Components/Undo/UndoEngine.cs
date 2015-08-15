using System.Collections.Generic;
using System.Diagnostics;

namespace Gearset {
    public sealed class UndoEngine {
        /// <summary>
        /// History of commands for undo.
        /// </summary>
        readonly LinkedList<IUndoable> _undoStack;

        /// <summary>
        /// History of commands for redo.
        /// </summary>
        readonly LinkedList<IUndoable> _redoStack;

        public UndoEngine() {
            _undoStack = new LinkedList<IUndoable>();
            _redoStack = new LinkedList<IUndoable>();

//#if DEBUG
//            GearsetResources.Console.Inspect("Undo Engine", this);
//#endif
        }

        /// <summary>
        /// Executes a command and adds it to the command history so it can
        /// be undone/redone.
        /// </summary>
        /// <param name="command">The command to execute and keep history of.</param>
        public void Execute(IUndoable command) {
            command.Do();
            AddCommand(command);
        }

        /// <summary>
        /// Undoes the last done command
        /// </summary>
        public void Undo() {
            if (_undoStack.Count > 0) {
                var command = _undoStack.Last.Value;
                if (command.CanUndo) {
                    command.Undo();
                    _undoStack.RemoveLast();
                    _redoStack.AddLast(command);
                }
                else {
#if DEBUG
                    // WHY CANT UNDO?
                    Debugger.Break();
#endif
                }
            }
        }

        /// <summary>
        /// Redo the last undone command
        /// </summary>
        public void Redo() {
            if (_redoStack.Count > 0) {
                var command = _redoStack.Last.Value;
                if (command.CanRedo) {
                    command.Do();
                    _redoStack.RemoveLast();
                    _undoStack.AddLast(command);
                }
            }
        }

        /// <summary>
        /// Adds a command to the history without executing it. This is usefull if
        /// the command was executed somewhere else but still needs undo/redo.
        /// </summary>
        /// <param name="currentMover"></param>
        public void AddCommand(IUndoable command) {
            _undoStack.AddLast(command);

            // Clear the redo stack (it might be empty already) because we just editted.
            _redoStack.Clear();
        }
    }
}
