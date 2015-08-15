using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components {
    sealed class SolidBoxDrawer : Gear {
        const int MaxBoxes = 1500;
        readonly VertexPositionColorTexture[] _vertices;
        readonly SamplerState _wrapSamplerState;
        int _boxCount;
        public Texture2D NoiseTexture;
        //private bool inspected;

        public SolidBoxDrawer()
            : base(new GearConfig()) {
            _vertices = new VertexPositionColorTexture[MaxBoxes * 6];

            // Generate a texture of gray noise.
            NoiseTexture = new Texture2D(GearsetResources.Game.GraphicsDevice, 128, 128);
            var random = new Random();
            var textureSize = 128 * 128;
            var noise = new Color[textureSize];
            for (var i = 0; i < textureSize; i++) {
                var shade = (byte)random.Next(100, 150);
                noise[i].R = shade;
                noise[i].G = shade;
                noise[i].B = shade;
                noise[i].A = 255;
            }
            NoiseTexture.SetData(noise);

            _wrapSamplerState = new SamplerState();
            _wrapSamplerState.AddressU = TextureAddressMode.Wrap;
            _wrapSamplerState.AddressV = TextureAddressMode.Wrap;
            _wrapSamplerState.Filter = TextureFilter.Point;
        }

        public sealed override void Update(GameTime gameTime) {
            //if (!inspected)
            //    GearsetResources.Console.Inspect("SOlidBoxDrawer", this);
            //inspected = true;
            base.Update(gameTime);
        }

        public void ShowBoxOnce(Vector2 min, Vector2 max) {
            if ((_boxCount + 6) > MaxBoxes)
                return;
            var tl = new Vector3(min, 0);
            var tr = new Vector3(max.X, min.Y, 0);
            var br = new Vector3(max, 0);
            var bl = new Vector3(min.X, max.Y, 0);

            var color = new Color(32, 32, 32, 127);

            var i = _boxCount * 6;
            _vertices[i + 0] = new VertexPositionColorTexture(tl, color, new Vector2(0, 0));
            _vertices[i + 1] = new VertexPositionColorTexture(tr, color, new Vector2(1, 0));
            _vertices[i + 2] = new VertexPositionColorTexture(br, color, new Vector2(1, 1));
            _vertices[i + 3] = new VertexPositionColorTexture(tl, color, new Vector2(0, 0));
            _vertices[i + 4] = new VertexPositionColorTexture(br, color, new Vector2(1, 1));
            _vertices[i + 5] = new VertexPositionColorTexture(bl, color, new Vector2(0, 1));

            _boxCount += 6;
        }

        public void ShowGradientBoxOnce(Vector2 min, Vector2 max, Color top, Color bottom) {
            if ((_boxCount + 6) > MaxBoxes)
                return;
            var tl = new Vector3(min, 0);
            var tr = new Vector3(max.X, min.Y, 0);
            var br = new Vector3(max, 0);
            var bl = new Vector3(min.X, max.Y, 0);

            var i = _boxCount * 6;

            var tiling = (max - min) * 1 / 128f;
            _vertices[i + 0] = new VertexPositionColorTexture(tl, top, new Vector2(0, 0));
            _vertices[i + 1] = new VertexPositionColorTexture(tr, top, new Vector2(tiling.X, 0));
            _vertices[i + 2] = new VertexPositionColorTexture(br, bottom, new Vector2(tiling.X, tiling.Y));
            _vertices[i + 3] = new VertexPositionColorTexture(tl, top, new Vector2(0, 0));
            _vertices[i + 4] = new VertexPositionColorTexture(br, bottom, new Vector2(tiling.X, tiling.Y));
            _vertices[i + 5] = new VertexPositionColorTexture(bl, bottom, new Vector2(0, tiling.Y));

            _boxCount += 6;
        }

        public sealed override void Draw(GameTime gameTime) {
            if (GearsetResources.CurrentRenderPass == RenderPass.ScreenSpacePass && _boxCount > 0) {
                GearsetResources.Effect2D.Texture = NoiseTexture;
                GearsetResources.Effect2D.TextureEnabled = true;
                GearsetResources.Effect2D.Techniques[0].Passes[0].Apply();
                GearsetResources.Game.GraphicsDevice.SamplerStates[0] = _wrapSamplerState;
                GearsetResources.Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _boxCount * 2);
                GearsetResources.Effect2D.Texture = null;
                GearsetResources.Effect2D.TextureEnabled = false;
                GearsetResources.Effect2D.Techniques[0].Passes[0].Apply();

                _boxCount = 0;
            }
        }
    }
}
