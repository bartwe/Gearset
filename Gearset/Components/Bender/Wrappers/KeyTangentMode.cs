using Microsoft.Xna.Framework;

namespace Gearset.Components.CurveEditorControl {
    public enum KeyTangentMode {
        // Auto calc.
        Flat = CurveTangent.Flat,
        Linear = CurveTangent.Linear,
        Smooth = CurveTangent.Smooth,

        // No auto calc.
        Custom = 3
    }
}