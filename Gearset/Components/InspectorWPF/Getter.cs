using System;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Delegate used for methods that return the value of a variable.
    /// </summary>
    public delegate Object Getter(params Object[] o);
}