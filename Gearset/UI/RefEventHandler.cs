using System;

namespace Gearset.UI {
    /// <summary>
    ///     Event handler that passes the event arguments by ref.
    /// </summary>
    public delegate void RefEventHandler<T>(Object sender, ref T args);
}
