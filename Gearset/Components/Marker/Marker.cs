using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components {
    /// <summary>
    ///     Places marks on 3D space with labels on 2D space
    /// </summary>
    public sealed class Marker : Gear {
        readonly Dictionary<String, DebugMark> _markTable = new Dictionary<String, DebugMark>();
        readonly SpriteBatch _spriteBatch;
        readonly Texture2D _markTexture;
        BasicEffect _effect;

        #region Constructor

        public Marker()
            : base(new GearConfig()) {
            _spriteBatch = GearsetResources.SpriteBatch;
            //this.effect = Resources.effect;
            _effect = GearsetResources.Effect;
#if XBOX
            this.markTexture = GearsetResources.Content.Load<Texture2D>("mark_Xbox360");
#elif WINDOWS_PHONE
            this.markTexture = GearsetResources.Content.Load<Texture2D>("mark_wp");
#else
            _markTexture = GearsetResources.Content.Load<Texture2D>("mark");
#endif
        }

        #endregion

        #region ShowMark (public methods)

        public void ShowMark(String key, Vector3 position, Color color) {
            if (!_markTable.ContainsKey(key))
                AddMark(key, position, color, false);
            else
                SetMark(key, position, color);
        }

        public void ShowMark(String key, Vector3 position) {
            if (!_markTable.ContainsKey(key))
                AddMark(key, position, Color.Yellow, false);
            else
                SetMark(key, position);
        }

        /// <summary>
        ///     ScreenSpace Mark.
        /// </summary>
        public void ShowMark(String key, Vector2 screenPosition, Color color) {
            var position = new Vector3(screenPosition, 0);
            if (!_markTable.ContainsKey(key))
                AddMark(key, position, color, true);
            else
                SetMark(key, position, color);
        }

        /// <summary>
        ///     ScreenSpace Mark.
        /// </summary>
        public void ShowMark(String key, Vector2 screenPosition) {
            var position = new Vector3(screenPosition, 0);
            if (!_markTable.ContainsKey(key))
                AddMark(key, position, Color.Yellow, true);
            else
                SetMark(key, position);
        }

        #endregion

        #region AddMark/SetMark (private methods)

        void AddMark(String key, Vector3 position, Color color, bool screenSpace) {
            var textSize = GearsetResources.FontTiny.MeasureString(key);
            RenderTarget2D renderTarget;


            renderTarget = new RenderTarget2D(GearsetResources.Device, (int)Math.Ceiling(textSize.X), (int)Math.Ceiling(textSize.Y));

            // Save the current render targets.
            var savedRenderTargets = GearsetResources.Device.GetRenderTargets();

            // Draw the label to a special renderTarget and then extract the texture.
            GearsetResources.Device.SetRenderTarget(renderTarget);
            GearsetResources.Device.Clear(new Color(0.0f, 0.0f, 0.0f, 0.4f));
            _spriteBatch.Begin();
            _spriteBatch.DrawString(GearsetResources.FontTiny, key, Vector2.One, Color.Black);
            _spriteBatch.DrawString(GearsetResources.FontTiny, key, Vector2.Zero, Color.White);
            _spriteBatch.End();

            // Restore the render targets.
            GearsetResources.Device.SetRenderTargets(savedRenderTargets);

            // Finally add the mark.
            _markTable.Add(key, new DebugMark(position, color, renderTarget, screenSpace));
        }

        void SetMark(String key, Vector3 position, Color color) {
            _markTable[key].MoveTo(position);
            _markTable[key].Color = color;
        }

        void SetMark(String key, Vector3 position) {
            _markTable[key].MoveTo(position);
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime) {
            switch (GearsetResources.CurrentRenderPass) {
                case RenderPass.BasicEffectPass:
                    DrawMarks();
                    break;
                case RenderPass.SpriteBatchPass:
                    DrawMarksLabels();
                    break;
            }
            base.Draw(gameTime);
        }

        void DrawMarks() {
            // Draw the marks.
            foreach (var m in _markTable) {
                var mark = m.Value;
                if (mark.ScreenSpace)
                    continue;
                switch (m.Value.Type) {
                    case MarkType.Cross:
                        GearsetResources.Device.DrawUserPrimitives(PrimitiveType.LineList, m.Value.Mark, 0, 3);
                        break;
                }
            }
        }

        void DrawMarksLabels() {
            // The marks' labels
            foreach (var m in _markTable) {
                var mark = m.Value;
                if (mark.ScreenSpace) {
                    if (mark.ScreenSpace == false)
                        continue;
                    _spriteBatch.Draw(_markTexture, mark.ScreenSpacePosition - new Vector2((int)(_markTexture.Width * .5f), (int)(_markTexture.Height * .5f)), mark.Color);
                    _spriteBatch.Draw(mark.Label, mark.ScreenSpacePosition + new Vector2((int)(_markTexture.Width * .5f), 0), Color.White);
                }
                else {
                    var texture = mark.Label;
                    var position = Vector3.Transform(mark.Mark[0].Position, Matrix.Identity * (GearsetResources.View * GearsetResources.Projection));
                    if (position.Z < 0)
                        continue;
                    var dest = new Rectangle((int)(((position.X / position.Z + 1) / 2) * GearsetResources.Device.Viewport.Width),
                        (int)(((-position.Y / position.Z + 1) / 2) * GearsetResources.Device.Viewport.Height),
                        texture.Width,
                        texture.Height);
                    //Color color = new Color((byte)(255 - position.Z * 10), (byte)(255 - position.Z * 10), (byte)(255 - position.Z * 10));
                    _spriteBatch.Draw(texture, dest, Color.White * GearsetResources.GlobalAlpha);
                }
            }
        }

        #endregion
    }
}
