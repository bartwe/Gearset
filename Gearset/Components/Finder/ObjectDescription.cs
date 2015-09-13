using System;

namespace Gearset {
    public sealed class ObjectDescription {
        readonly String _name;

        /// <summary>
        ///     Creates an ObjectDescription. The name field will be taken out
        ///     of the object's ToString() method.
        /// </summary>
        /// <param name="o">The matching object.</param>
        /// <param name="description">A string describing the object.</param>
        public ObjectDescription(Object o, String description) {
            Object = o;
            Description = description;
        }

        /// <summary>
        ///     Creates an ObjectDescription.
        /// </summary>
        /// <param name="o">The matching object.</param>
        /// <param name="description">A string describing the object.</param>
        /// <param name="name">The name to use instead of the object's ToString. Pass null to use the Object's ToString.</param>
        public ObjectDescription(Object o, String description, String name) {
            _name = name;
            Object = o;
            Description = description;
        }

        public String Name { get { return _name == null ? Object.ToString() : _name; } }
        public Object Object { get; set; }
        public String Description { get; set; }

        public override string ToString() {
            return Object + " (" + Description + ")";
        }
    }
}
