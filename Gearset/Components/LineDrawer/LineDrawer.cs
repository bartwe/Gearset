namespace Gearset.Components {
    /// <summary>   
    /// Draw lines in 2D/3D space, can draw line lists and line strips.
    /// </summary>
    public sealed class LineDrawer : InternalLineDrawer {
        public LineDrawer()
            : base(GearsetSettings.Instance.LineDrawerConfig) {
            CoordinateSpace = CoordinateSpace.GameSpace;
        }
    }
}
