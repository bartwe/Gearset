using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components {
    /// <summary>
    /// Shows important alerts on the screen
    /// </summary>
    public class Alerter : Gear {
        readonly List<AlertItem> _alerts = new List<AlertItem>();
        readonly SpriteBatch _spriteBatch;
        readonly float _textHeight;

        /// <summary>
        /// We use this list to delete elements that are not
        /// being shown anymore.
        /// </summary>
        readonly LinkedList<AlertItem> _toRemove = new LinkedList<AlertItem>();

        Vector2 _alertPosition;

        #region Constructors

        public Alerter()
            : base(GearsetSettings.Instance.AlerterConfig) {
            _spriteBatch = GearsetResources.SpriteBatch;
            _textHeight = GearsetResources.FontAlert.MeasureString("M").Y;
            OnResolutionChanged();
        }

        #endregion

        public override void OnResolutionChanged() {
            _alertPosition = new Vector2(GearsetResources.Device.Viewport.Width / 2, GearsetResources.Device.Viewport.Height / 2 - _textHeight * 1.5f);
        }

        #region Alerts

        public void Alert(String message) {
            _alerts.Add(new AlertItem(message, _alertPosition, 40));
            _alertPosition.Y += 48;
            if (_alertPosition.Y >= 300) _alertPosition.Y = 150;
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime) {
            foreach (var i in _alerts) {
                if (i.RemainingTime > 0) i.RemainingTime--;
                else if (i.RemainingTime == 0) _toRemove.AddLast(i);
            }
            foreach (var i in _toRemove) {
                _alerts.Remove(i);
            }
            _toRemove.Clear();
            base.Update(gameTime);
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime) {
            // Only draw if we're doing a spriteBatch passs
            if (GearsetResources.CurrentRenderPass != RenderPass.SpriteBatchPass) return;
            foreach (var i in _alerts) {
                var textWidth = GearsetResources.FontAlert.MeasureString(i.Text).X;
                var origin = new Vector2(textWidth * .5f, _textHeight * .5f);
                var alpha = (byte)MathHelper.Clamp((i.RemainingTime / 7f) * 255, 0, 255);
                var forecolor = new Color(alpha, alpha, alpha, alpha);
                var backcolor = new Color(0, 0, 0, (byte)MathHelper.Clamp((i.RemainingTime / 60f) * 255, 0, 255));
                _spriteBatch.DrawString(GearsetResources.FontAlert, i.Text, i.Position + new Vector2(-2, 2), backcolor, 0, origin, Vector2.One, SpriteEffects.None, 0);
                _spriteBatch.DrawString(GearsetResources.FontAlert, i.Text, i.Position + new Vector2(0, 0), forecolor, 0, origin, Vector2.One, SpriteEffects.None, 0);
            }
        }

        #endregion
    }
}
