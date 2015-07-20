﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// Changes the tangent value of a given key.
    /// </summary>
    public class ChangeContinuityCommand : CurveEditorCommand {
        readonly CurveContinuity newKeyContinuity;
        // Saved state.
        readonly long[] affectedKeys;
        CurveContinuity[] prevKeyContinuity;

        /// <summary>
        /// Creates a new command to select the given keys. You can pass null to deselect all.
        /// </summary>
        public ChangeContinuityCommand(CurveEditorControl2 control, CurveContinuity newKeyContinuity)
            : base(control) {
            // Store the parameters.
            this.newKeyContinuity = newKeyContinuity;

            // Store the current selection, if any.
            affectedKeys = new long[control.Selection.Count];
            var i = 0;
            foreach (var key in control.Selection) {
                affectedKeys[i++] = key.Id;
            }
        }

        public override bool CanUndo { get { return true; } }

        public override void Do() {
            // Do we need to save prev values?
            if (prevKeyContinuity == null) {
                prevKeyContinuity = new CurveContinuity[affectedKeys.Length];

                // Save the prev values.
                for (var i = 0; i < affectedKeys.Length; i++) {
                    prevKeyContinuity[i] = Control.Keys[affectedKeys[i]].Key.Continuity;
                }
            }

            // Set new values.
            for (var i = 0; i < affectedKeys.Length; i++) {
                Control.Keys[affectedKeys[i]].Key.Continuity = newKeyContinuity;
            }

            // Redraw!
            Control.InvalidateVisual();
        }

        public override void Undo() {
            var affectedCurves = new HashSet<CurveWrapper>();

            // Revert to previous values.
            for (var i = 0; i < affectedKeys.Length; i++) {
                Control.Keys[affectedKeys[i]].Key.Continuity = prevKeyContinuity[i];
            }

            // Redraw!
            Control.InvalidateVisual();
        }
    }
}
