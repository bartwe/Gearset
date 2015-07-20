using System;
using Gearset.Components;
using Microsoft.Xna.Framework;

namespace Gearset.UI {
    /// <summary>
    /// A box.
    /// </summary>
    public class LayoutBox {
        Vector2 _position;
        Vector2 _size;

        public LayoutBox(Vector2 position, Vector2 size) {
            Position = position;
            Size = size;

            UiManager.Boxes.Add(this);
        }

        public LayoutBox(Vector2 position) {
            Position = position;
            Size = new Vector2(100); // Some default size.

            UiManager.Boxes.Add(this);
        }

        /// <summary>
        /// Gets or sets the parent of this LayoutBox.
        /// </summary>
        public LayoutBox Parent { get; set; }

        public bool IsMouseOver { get; internal set; }

        /// <summary>
        /// Gets or sets the position of this LayoutBox.
        /// </summary>
        /// <value>The position</value>
        public Vector2 Position {
            get { return _position; }
            set {
                _position = value;
                OnPositionChanged();
            }
        }

        public Vector2 Size {
            get { return _size; }
            set {
                _size = new Vector2(Math.Max(0, value.X), Math.Max(0, value.Y));
                OnSizeChanged();
            }
        }

        public float Left { get { return _position.X; } set { _position.X = value; } }
        public float Right { get { return _position.X + _size.X; } set { _position.X = value - _size.X; } }
        public float Top { get { return _position.Y; } set { _position.Y = value; } }
        public float Bottom { get { return _position.X + _size.Y; } set { _position.Y = value - _size.Y; } }
        public Vector2 Center { get { return _position + _size * .5f; } set { _position = value - _size * .5f; } }
        public Vector2 TopLeft { get { return new Vector2(Position.X, _position.Y); } set { _position = value; } }
        public Vector2 TopRight { get { return new Vector2(Position.X + Size.X, _position.Y); } set { _position = new Vector2(value.X - _size.X, value.Y); } }
        public Vector2 BottomRight { get { return _position + _size; } set { _position = value - _size; } }
        public Vector2 BottomLeft { get { return new Vector2(Position.X, _position.Y + Size.Y); } set { _position = new Vector2(value.X, value.Y - _size.Y); } }
        public float Width { get { return _size.X; } set { _size.X = Math.Max(value, 0); } }
        public float Height { get { return _size.Y; } set { _size.Y = Math.Max(value, 0); } }

        /// <summary>
        /// Returns the area where the elements of this UI box must be drawn.
        /// </summary>
        public Rectangle DrawArea { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y); } }

        public event RefEventHandler<Vector2> MouseDown;
        public event RefEventHandler<Vector2> MouseUp;
        public event RefEventHandler<Vector2> Click;
        // For now the argument means the delta change.
        public event RefEventHandler<Vector2> Dragged;
        protected virtual void OnPositionChanged() {}
        protected virtual void OnSizeChanged() {}

        public Vector2 GetScreenPosition() {
            var screenPosition = Position;
            var current = Parent;
            while (current != null) {
                screenPosition += current.Position;
                current = current.Parent;
            }
            return screenPosition;
        }

        /// <summary>
        /// Helper methods, draws the border of the box. Must be called every frame.
        /// </summary>
        public void DrawCrossLines(Color color) {
            DrawCrossLines(color, GearsetResources.Console.LineDrawer);
        }

        public void DrawCrossLines(Color color, InternalLineDrawer lineDrawer) {
            lineDrawer.ShowLineOnce(TopLeft, BottomRight, color);
            lineDrawer.ShowLineOnce(TopRight, BottomLeft, color);
        }

        /// <summary>
        /// Helper methods, draws the border of the box. Must be called every frame.
        /// </summary>
        public void DrawBorderLines(Color color) {
            DrawBorderLines(color, GearsetResources.Console.LineDrawer);
        }

        /// <summary>
        /// Helper methods, draws the border of the box. Must be called every frame.
        /// TODO: Move this to a UI debug drawer or something similar
        /// </summary>
        internal void DrawBorderLines(Color color, InternalLineDrawer lineDrawer) {
            var screenPos = GetScreenPosition();
            var tl = screenPos;
            var tr = new Vector2(screenPos.X + _size.X, screenPos.Y);
            var br = new Vector2(screenPos.X + _size.X, screenPos.Y + _size.Y);
            var bl = new Vector2(screenPos.X, screenPos.Y + _size.Y);
            lineDrawer.ShowLineOnce(tl, tr, color);
            lineDrawer.ShowLineOnce(tr, br, color);
            lineDrawer.ShowLineOnce(br, bl, color);
            lineDrawer.ShowLineOnce(bl, tl, color);
        }

        /// <summary>
        /// Returns true if the passed point is contained in this box.
        /// </summary>
        internal bool Contains(Vector2 point) {
            var min = GetScreenPosition();
            var max = min + Size;

            return
                point.X >= min.X && point.X <= max.X &&
                point.Y >= min.Y && point.Y <= max.Y;
        }

        /// <summary>
        /// Returns the passed world point in local point of this LayoutBox.
        /// </summary>
        public Vector2 WorldToLocal(Vector2 point) {
            var current = Parent;
            while (current != null) {
                point -= current.Position;
                current = current.Parent;
            }
            return point;
        }

        #region Event raisers

        /// <summary>
        /// Only to be called by the MouseRouter
        /// </summary>
        internal void RaiseMouseDown(Vector2 position) {
            if (MouseDown != null)
                MouseDown(this, ref position);
        }

        /// <summary>
        /// Only to be called by the MouseRouter
        /// </summary>
        internal void RaiseMouseUp(Vector2 position) {
            if (MouseUp != null)
                MouseUp(this, ref position);
        }

        /// <summary>
        /// Only to be called by the MouseRouter
        /// </summary>
        internal void RaiseClick(Vector2 position) {
            if (Click != null)
                Click(this, ref position);
        }

        /// <summary>
        /// Only to be called by the MouseRouter
        /// </summary>
        internal void RaiseDragged(Vector2 delta) {
            if (Dragged != null)
                Dragged(this, ref delta);
        }

        #endregion
    }
}
