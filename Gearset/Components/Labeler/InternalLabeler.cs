using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    /// Shows labels of text in 2D positions or 3d (TODO)s unprojected positions.
    /// </summary>
    public class InternalLabeler : Gear {
        readonly Dictionary<String, Label2D> _persistent2DLabels;
        readonly Dictionary<String, Label3D> _persistent3DLabels;

        public InternalLabeler()
            : this(new LabelerConfig()) {}

        public InternalLabeler(LabelerConfig config)
            : base(config) {
            _persistent2DLabels = new Dictionary<string, Label2D>();
            _persistent3DLabels = new Dictionary<string, Label3D>();
            Config.DefaultColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            Config.Cleared += Config_Cleared;
        }

        public LabelerConfig Config { get { return GearsetSettings.Instance.LabelerConfig; } }

        void Config_Cleared(object sender, EventArgs e) {
            _persistent2DLabels.Clear();
            _persistent3DLabels.Clear();
        }

        /// <summary>
        /// Remove a persistent label if exists.
        /// </summary>
        public void HideLabel(String name) {
            _persistent2DLabels.Remove(name);
        }

        public sealed override void Draw(GameTime gameTime) {
            if (GearsetResources.CurrentRenderPass != RenderPass.SpriteBatchPass)
                return;

            // Draw persistent labels.
            foreach (var label in _persistent2DLabels.Values) {
                GearsetResources.SpriteBatch.DrawString(GearsetResources.FontTiny, label.Text, label.Position + Vector2.One, Color.Black * GearsetResources.GlobalAlpha);
                GearsetResources.SpriteBatch.DrawString(GearsetResources.FontTiny, label.Text, label.Position, label.Color * GearsetResources.GlobalAlpha);
            }

            // Draw persistent labels.
            Matrix transform;
            Matrix.Multiply(ref GearsetResources.World, ref GearsetResources.View, out transform);
            Matrix.Multiply(ref transform, ref GearsetResources.Projection, out transform);
            var screenSize = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth, Game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            foreach (var label in _persistent3DLabels.Values) {
                Vector3 projected;
                Vector3.Transform(ref label.Position, ref transform, out projected);

                var screenPos = new Vector2(projected.X, -projected.Y) / projected.Z;
                screenPos = screenPos * .5f + Vector2.One * .5f;
                screenPos *= screenSize;
                GearsetResources.SpriteBatch.DrawString(GearsetResources.FontTiny, label.Text, screenPos + Vector2.One, Color.Black * GearsetResources.GlobalAlpha);
                GearsetResources.SpriteBatch.DrawString(GearsetResources.FontTiny, label.Text, screenPos, label.Color * GearsetResources.GlobalAlpha);
            }
            base.Draw(gameTime);
        }

        sealed class Label2D {
            public String Name;
            public String Text;
            public Vector2 Position;
            public Color Color;

            public Label2D(String name, Vector2 position, String text, Color color) {
                Name = name;
                Text = text;
                Position = position;
                Color = color;
            }
        }

        sealed class Label3D {
            public String Name;
            public String Text;
            public Vector3 Position;
            public Color Color;

            public Label3D(String name, Vector3 position, String text, Color color) {
                Name = name;
                Text = text;
                Position = position;
                Color = color;
            }
        }

        #region Persistent 2D Labels

        /// <summary>
        /// Shows a label in the specified position. or changes a label position.
        /// </summary>
        public void ShowLabel(String name, Vector2 position) {
            Label2D label;
            if (!_persistent2DLabels.TryGetValue(name, out label)) {
                label = new Label2D(name, position, name, Config.DefaultColor);
                _persistent2DLabels.Add(name, label);
            }
            else {
                label.Position = position;
            }
        }

        /// <summary>
        /// Creates a label, or changes its values.
        /// </summary>
        public void ShowLabel(String name, Vector2 position, String text) {
            Label2D label;
            if (!_persistent2DLabels.TryGetValue(name, out label)) {
                label = new Label2D(name, position, text, Config.DefaultColor);
                _persistent2DLabels.Add(name, label);
            }
            else {
                label.Text = text;
                label.Position = position;
            }
        }

        /// <summary>
        /// Creates a label or changes its values
        /// </summary>
        public void ShowLabel(String name, Vector2 position, String text, Color color) {
            Label2D label;
            if (!_persistent2DLabels.TryGetValue(name, out label)) {
                label = new Label2D(name, position, text, color);
                _persistent2DLabels.Add(name, label);
            }
            else {
                label.Text = text;
                label.Position = position;
                label.Color = color;
            }
        }

        #endregion

        #region Persistent 3D Labels

        /// <summary>
        /// Shows a label in the specified position. or changes a label position.
        /// </summary>
        public void ShowLabel(String name, Vector3 position) {
            Label3D label;
            if (!_persistent3DLabels.TryGetValue(name, out label)) {
                label = new Label3D(name, position, name, Config.DefaultColor);
                _persistent3DLabels.Add(name, label);
            }
            else {
                label.Position = position;
            }
        }

        /// <summary>
        /// Creates a label, or changes its values.
        /// </summary>
        public void ShowLabel(String name, Vector3 position, String text) {
            Label3D label;
            if (!_persistent3DLabels.TryGetValue(name, out label)) {
                label = new Label3D(name, position, text, Config.DefaultColor);
                _persistent3DLabels.Add(name, label);
            }
            else {
                label.Text = text;
                label.Position = position;
            }
        }

        /// <summary>
        /// Creates a label or changes its values
        /// </summary>
        public void ShowLabel(String name, Vector3 position, String text, Color color) {
            Label3D label;
            if (!_persistent3DLabels.TryGetValue(name, out label)) {
                label = new Label3D(name, position, text, color);
                _persistent3DLabels.Add(name, label);
            }
            else {
                label.Text = text;
                label.Position = position;
                label.Color = color;
            }
        }

        #endregion
    }
}
