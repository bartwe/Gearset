using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.Profiler {
    //TODO: Added this with a much bigger buffer and no noise texture - think we need a better solution going forward.
    class TempBoxDrawer : Gear {
        const int MaxBoxes = 6000;
        readonly VertexPositionColor[] _vertices;
        int _boxCount;

        public TempBoxDrawer() : base(new GearConfig()) {
            _vertices = new VertexPositionColor[MaxBoxes * 6];
        }

        public void ShowBoxOnce(Vector2 min, Vector2 max) {
            if (_boxCount >= MaxBoxes)
                return;

            var tl = new Vector3(min, 0);
            var tr = new Vector3(max.X, min.Y, 0);
            var br = new Vector3(max, 0);
            var bl = new Vector3(min.X, max.Y, 0);

            var color = new Color(32, 32, 32, 127);

            var i = _boxCount * 6;
            _vertices[i + 0] = new VertexPositionColor(tl, color);
            _vertices[i + 1] = new VertexPositionColor(tr, color);
            _vertices[i + 2] = new VertexPositionColor(br, color);
            _vertices[i + 3] = new VertexPositionColor(tl, color);
            _vertices[i + 4] = new VertexPositionColor(br, color);
            _vertices[i + 5] = new VertexPositionColor(bl, color);

            _boxCount += 6;
        }

        public void ShowGradientBoxOnce(Vector2 min, Vector2 max, Color top, Color bottom) {
            if (_boxCount >= MaxBoxes)
                return;

            var tl = new Vector3(min, 0);
            var tr = new Vector3(max.X, min.Y, 0);
            var br = new Vector3(max, 0);
            var bl = new Vector3(min.X, max.Y, 0);

            var i = _boxCount * 6;

            _vertices[i + 0] = new VertexPositionColor(tl, top);
            ;
            _vertices[i + 1] = new VertexPositionColor(tr, top);
            _vertices[i + 2] = new VertexPositionColor(br, bottom);
            _vertices[i + 3] = new VertexPositionColor(tl, top);
            _vertices[i + 4] = new VertexPositionColor(br, bottom);
            _vertices[i + 5] = new VertexPositionColor(bl, bottom);

            _boxCount += 6;
        }

        public override void Draw(GameTime gameTime) {
            if (GearsetResources.CurrentRenderPass == RenderPass.ScreenSpacePass && _boxCount > 0) {
                GearsetResources.Effect2D.Texture = null;
                GearsetResources.Effect2D.TextureEnabled = false;
                GearsetResources.Effect2D.Techniques[0].Passes[0].Apply();
                GearsetResources.Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _boxCount * 2);

                _boxCount = 0;
            }
        }
    }
}
