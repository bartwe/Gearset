using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gearset.Components.Persistor {
    [Serializable]
    sealed class ValueCollection : Dictionary<string, Dictionary<string, Object>> {
        public ValueCollection() {}

        public ValueCollection(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}
