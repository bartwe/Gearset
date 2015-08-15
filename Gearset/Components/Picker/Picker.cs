using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    /// Class that handles picking objects from the screen
    /// </summary>
    sealed class Picker : Gear {
        readonly List<IPickable> _pickables;
        IPickable _hoveringObject;
        IPickable _selectedObject;

        public Picker()
            : base(new GearConfig()) {
            _pickables = new List<IPickable>();
        }

        public IPickable Picked { get; private set; }

        /// <summary>
        /// Adds a new Pickable so it can be picked with
        /// the mouse.
        /// </summary>
        /// <param name="pickable"></param>
        public void AddPickable(IPickable pickable) {
            _pickables.Add(pickable);
        }

        public sealed override void Update(GameTime gameTime) {
            //  Unproject the screen space mouse coordinate into model space 
            //  coordinates. Because the world space matrix is identity, this 
            //  gives the coordinates in world space.
            var vp = GearsetResources.Game.GraphicsDevice.Viewport;
            //  Note the order of the parameters! Projection first.
            var pos1 = vp.Unproject(new Vector3(GearsetResources.Mouse.Position, 0), GearsetResources.Projection, GearsetResources.View, Matrix.Identity);
            var pos2 = vp.Unproject(new Vector3(GearsetResources.Mouse.Position, 1), GearsetResources.Projection, GearsetResources.View, Matrix.Identity);
            var dir = Vector3.Normalize(pos2 - pos1);

            // Cast a ray and check if we've hit anything
            IPickable closestPickable = null;
            float? distanceToClosest = null;
            var ray = new Ray(pos1, dir);
            foreach (var pickable in _pickables) {
                var dist = new float?();
                if (pickable is IPickable<BoundingBox>) {
                    var p = pickable as IPickable<BoundingBox>;
                    var v = p.PickableVolume;
                    ray.Intersects(ref v, out dist);
                }
                else if (pickable is IPickable<BoundingSphere>) {
                    var p = pickable as IPickable<BoundingSphere>;
                    var v = p.PickableVolume;
                    ray.Intersects(ref v, out dist);
                }
                else if (pickable is IPickable<Plane>) {
                    var p = pickable as IPickable<Plane>;
                    var v = p.PickableVolume;
                    ray.Intersects(ref v, out dist);
                }
                else if (pickable is IPickable<Rectangle>) {
                    var p = pickable as IPickable<Rectangle>;
                    if (p.PickableVolume.Contains(new Point((int)GearsetResources.Mouse.Position.X, (int)GearsetResources.Mouse.Position.Y)))
                        dist = 0;
                }

                if (dist.HasValue && (!distanceToClosest.HasValue || dist.Value < distanceToClosest) && _selectedObject != pickable) {
                    distanceToClosest = dist.Value;
                    closestPickable = pickable;
                }
            }

            // Check if we actually hit something
            if (distanceToClosest.HasValue) {
                _hoveringObject = closestPickable;

                if (GearsetResources.Mouse.IsLeftJustDown()) {
                    Picked = closestPickable;

                    // Let the object know that it has been picked.
                    Picked.Picked();

                    _selectedObject = Picked;
                }
            }
            else {
                _hoveringObject = null;
            }
        }

        public sealed override void Draw(GameTime gameTime) {
            // Only draw if we're doing a BasicEffectPass pass
            if (GearsetResources.CurrentRenderPass != RenderPass.BasicEffectPass) return;

            if (_hoveringObject is IPickable<BoundingBox>)
                BoundingBoxHelper.DrawBoundingBox(((IPickable<BoundingBox>)_hoveringObject).PickableVolume, Color.Gray);
            if (_selectedObject is IPickable<BoundingBox>)
                BoundingBoxHelper.DrawBoundingBox(((IPickable<BoundingBox>)_selectedObject).PickableVolume, Color.White);
        }
    }
}
