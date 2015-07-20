namespace Gearset.Components {
    /// <summary>
    /// Implement to allow the console pick this object, Type T
    /// defines kind of Pickable Volume that will be intersected
    /// with the casted ray.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IPickable<T> : IPickable {
        /// <summary>
        /// The pickable Volume
        /// </summary>
        T PickableVolume { get; }
    }

    interface IPickable {
        /// <summary>
        /// This method will be called when the object has been
        /// picked with the mouse
        /// </summary>
        void Picked();
    }
}
