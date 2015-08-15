using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gearset.Components {
    /// <summary>
    /// Displays a hierarchy of values that need to be traced.
    /// </summary>
    public sealed class TreeView : Gear {
        readonly TreeViewNode _root;
        readonly Texture2D _closedTexture;
        readonly Texture2D _openedTexture;
        int _iterationCount;
        Vector2 _position;
        MouseState _prevMouse;
        public TreeViewConfig Config { get { return GearsetSettings.Instance.TreeViewConfig; } }

        public void Clear() {
            _root.Nodes.Clear();
        }

        #region Set

        /// <summary>
        /// Sets the value of a specified key, if the key is not present
        /// in the tree, then is added.
        /// </summary>
        /// <param name="key"></param>
        public void Set(String key, Object value) {
            var foundKey = false;
            var currentNode = _root;
            var remainingName = key;
            while (!foundKey) {
                _iterationCount++;
                String subName;
                var foundSubKey = false;

                var dotIndex = remainingName.IndexOf('.');
                if (dotIndex < 0 || dotIndex >= remainingName.Length) {
                    subName = remainingName;
                    remainingName = "";
                    foundKey = true;
                }
                else {
                    subName = remainingName.Remove(dotIndex, remainingName.Length - dotIndex);
                    remainingName = remainingName.Substring(subName.Length + 1);
                }

                foreach (var node in currentNode.Nodes) {
                    if (node.Name == subName) {
                        currentNode = node;
                        foundSubKey = true; // A part of the key was found.
                        break;
                    }
                }
                if (!foundSubKey) {
                    // If there's no subKey, we create it.
                    var newNode = new TreeViewNode(subName);
                    currentNode.Nodes.Add(newNode);
                    currentNode = newNode;
                    foundSubKey = true; // A part of the key was found (created).
                }
            }
            currentNode.Value = value;
        }

        #endregion

        #region Update

        public sealed override void Update(GameTime gameTime) {
#if XBOX360
            return;
#endif
            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed) {
                LeftClick(new Vector2(mouse.X, mouse.Y));
            }
            _prevMouse = mouse;
        }

        #endregion

        #region Constructor

        public TreeView()
            : base(GearsetSettings.Instance.TreeViewConfig) {
            Config.Cleared += Config_Cleared;
            Config.Filter = String.Empty;

            _root = new TreeViewNode("root");
#if XBOX
            this.openedTexture = GearsetResources.Content.Load<Texture2D>("close_Xbox360");
            this.closedTexture = GearsetResources.Content.Load<Texture2D>("open_Xbox360");
#elif WINDOWS_PHONE
            this.openedTexture = GearsetResources.Content.Load<Texture2D>("close_wp");
            this.closedTexture = GearsetResources.Content.Load<Texture2D>("open_wp");
#else
            _openedTexture = GearsetResources.Content.Load<Texture2D>("close");
            _closedTexture = GearsetResources.Content.Load<Texture2D>("open");
#endif
            _position = new Vector2(5, 20);
        }

        void Config_Cleared(object sender, EventArgs e) {
            Clear();
        }

        #endregion

        #region LeftClick

        public void LeftClick(Vector2 clickPos) {
            var position = 20;
            foreach (var node in _root.Nodes) {
                if (Config.Filter == String.Empty || node.FilterName.Contains(Config.Filter))
                    position = CheckLeftRecursively(node, position, 1, clickPos);
            }
        }

        int CheckLeftRecursively(TreeViewNode node, int position, int level, Vector2 clickPos) {
            var newPosition = position + 12;

            var rect = new Rectangle((int)_position.X + level * 11 - 12, (int)_position.Y + position, _closedTexture.Width, _closedTexture.Height);
            if (rect.Contains((int)clickPos.X, (int)clickPos.Y)) {
                node.Toggle();
                return newPosition;
            }
            if (node.Open) {
                foreach (var n in node.Nodes) {
                    newPosition = CheckLeftRecursively(n, newPosition, level + 1, clickPos);
                }
            }
            return newPosition;
        }

        #endregion

        #region Draw

        public sealed override void Draw(GameTime gameTime) {
            // Only draw if we're doing a spriteBatch pass
            if (GearsetResources.CurrentRenderPass != RenderPass.SpriteBatchPass) return;

            var position = 20;
            foreach (var node in _root.Nodes) {
                if (Config.Filter == String.Empty || node.FilterName.Contains(Config.Filter))
                    position = DrawRecursively(node, position, 1);
            }
        }

        int DrawRecursively(TreeViewNode node, int position, int level) {
            var newPosition = position + 12;
            DrawNode(node, position, level);
            if (node.Open) {
                foreach (var n in node.Nodes) {
                    newPosition = DrawRecursively(n, newPosition, level + 1);
                }
            }
            return newPosition;
        }


        void DrawText(String text, Vector2 drawPosition) {
            var color = new Color(1, 1, 1, GearsetResources.GlobalAlpha) * GearsetResources.GlobalAlpha;
            var shadowColor = new Color(0, 0, 0, GearsetResources.GlobalAlpha) * GearsetResources.GlobalAlpha;

            GearsetResources.SpriteBatch.DrawString(GearsetResources.Font, text, drawPosition + new Vector2(-1, 0), shadowColor);
            GearsetResources.SpriteBatch.DrawString(GearsetResources.Font, text, drawPosition + new Vector2(0, 1), shadowColor);
            GearsetResources.SpriteBatch.DrawString(GearsetResources.Font, text, drawPosition + new Vector2(0, -1), shadowColor);
            GearsetResources.SpriteBatch.DrawString(GearsetResources.Font, text, drawPosition + new Vector2(1, 0), shadowColor);
            GearsetResources.SpriteBatch.DrawString(GearsetResources.Font, text, drawPosition, color);

            var textSize = GearsetResources.Font.MeasureString(text);
            textSize.Y = 12;
            textSize.X += 6;
            drawPosition.X -= 3;
            GearsetResources.Console.SolidBoxDrawer.ShowBoxOnce(drawPosition, drawPosition + textSize);
        }

        void DrawNode(TreeViewNode node, int position, int level) {
            var drawPosition = new Vector2(_position.X + level * 11, _position.Y + position);
            if (node.Nodes.Count == 0) {
                DrawText(node.Name + ": " + TextHelper.FormatForDebug(node.Value), drawPosition);
            }
            else {
                var color = new Color(1, 1, 1, GearsetResources.GlobalAlpha) * GearsetResources.GlobalAlpha;
                if (!node.Open)
                    GearsetResources.SpriteBatch.Draw(_closedTexture, drawPosition - new Vector2(12, 0), color);
                else
                    GearsetResources.SpriteBatch.Draw(_openedTexture, drawPosition - new Vector2(12, 0), color);
                DrawText(node.Name, drawPosition);
            }
        }

        #endregion
    }
}
