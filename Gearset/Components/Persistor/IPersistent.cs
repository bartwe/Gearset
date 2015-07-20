using System;

namespace Gearset.Components.Persistor {
    /// <summary>
    /// Defines an object which members (fields and properties)
    /// can be initialized by the XDTK's Persistor Component.
    /// </summary>
    interface IPersistent {
        /// <summary>
        /// A comma separeted string of all IDs this persistent object
        /// will get its members initialized from.
        /// </summary>
        String Ids { get; }
    }
}
