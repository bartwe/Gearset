using System;
using System.Windows.Media;

namespace Gearset.Components.Logger {
    public class LogItem {
        /// <summary>
        /// The background color to use for this Log, it is the same for 
        /// items in the same update.
        /// </summary>
        /// <value>The color.</value>
        public Brush Color { get; set; }

        /// <summary>
        /// The number of the update where this log was generated
        /// </summary>
        public int UpdateNumber { get; set; }

        /// <summary>
        /// The name of the string where this logItem belongs.
        /// </summary>
        public StreamItem Stream { get; set; }

        /// <summary>
        /// The actual contents of the log
        /// </summary>
        public String Content { get; set; }
    }
}
