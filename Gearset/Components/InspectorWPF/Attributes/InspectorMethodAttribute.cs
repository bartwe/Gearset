﻿using System;

namespace Gearset {
    /// <summary>
    ///     Add this attribute to fields or properties that need
    ///     to be customized when shown in Gearset's inspector window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InspectorMethodAttribute : Attribute {
        /// <summary>
        ///     The name to show in Gearset's inspector window.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     The tooltip to show in Gearset's inspector window.
        /// </summary>
        public string Tooltip { get; set; }
    }
}
