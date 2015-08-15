namespace Gearset.Components {
    public enum CoordinateSpace {
        /// <summary>
        /// The geometry will be interpreted as being in screen space.
        /// </summary>
        ScreenSpace,

        /// <summary>
        /// The geometry will be interpreted as being in game space, thus the Transform2D matrix will be applied.
        /// </summary>
        GameSpace
    }
}
