namespace Gearset.Components.InspectorWPF {
    public class ValueTypeWrapper<T> where T : struct {
        public T Value { get; set; }

        public override string ToString() {
            return Value + " [copy]";
        }
    }
}