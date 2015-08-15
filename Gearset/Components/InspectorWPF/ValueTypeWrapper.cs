namespace Gearset.Components.InspectorWPF {
    public sealed class ValueTypeWrapper<T> where T : struct {
        public T Value { get; set; }

        public sealed override string ToString() {
            return Value + " [copy]";
        }
    }
}