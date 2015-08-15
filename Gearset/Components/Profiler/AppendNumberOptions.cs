using System;

namespace Gearset.Profiler.Extensions {
    /// <summary>
    /// Options for StringBuilder extension methods.
    /// </summary>
    [Flags]
    public enum AppendNumberOptions {
        // Normal format.
        None = 0,

        // Added "+" sign for positive value.
        PositiveSign = 1,

        // Insert Number group separation characters.
        // In Use, added "," for every 3 digits.
        NumberGroup = 2
    }
}
