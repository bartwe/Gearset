using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Rectangle = System.Drawing.Rectangle;

namespace Gearset.Components {
    /// <summary>
    ///     This is a game component that implements IUpdateable.
    /// </summary>
    public sealed class MouseComponent : Gear {
        MouseState _state;
        MouseState _prevState;

        /// <summary>
        ///     Can last only one frame true
        /// </summary>
        bool _justClicked;

        /// <summary>
        ///     Can last only one frame true
        /// </summary>
        bool _justDragging;

        /// <summary>
        ///     Remains true while the left mouse button is pressed.
        /// </summary>
        bool _mouseDown;

        /// <summary>
        ///     How long the mouse was down.
        /// </summary>
        int _mouseDownTime;

        /// <summary>
        ///     The position where the left button became pressed.
        /// </summary>
        Vector2 _mouseDownPosition;

        bool _dragging;

        #region Constructor

        public MouseComponent()
            : base(new GearConfig()) {
            _state = Mouse.GetState();
        }

        #endregion

        public Vector2 Position { get { return new Vector2(_state.X, _state.Y); } }
        bool HaveFocus { get { return Game.IsActive; } }

        /// <summary>
        ///     The distance the (down left) mouse can move without being
        ///     considered a drag (still a click). Should be zero for PC
        ///     games and some higher value for tablets.
        /// </summary>
        public float ClickThreshold { get; set; }

        /// <summary>
        ///     Set to true if the component should force the mouse to
        ///     stay inside the client logger bounds.
        /// </summary>
        public bool KeepMouseInWindow { get; set; }

        /// <summary>
        ///     Get the movement the mouse have made since it started dragging.
        ///     If the mouse is not dragging it will return Vector2.Zero.
        /// </summary>
        public Vector2 DragOffset {
            get {
                if (_dragging)
                    return Position - _mouseDownPosition;
                return Vector2.Zero;
            }
        }

        #region Update

        public override void Update(GameTime gameTime) {
            _prevState = _state;
            _state = Mouse.GetState();
            _justClicked = false;
            _justDragging = false;
            if (_mouseDown) {
                if (Vector2.Distance(Position, _mouseDownPosition) > ClickThreshold && !_dragging) {
                    _justDragging = true;
                    _dragging = true;
                }
                _mouseDownTime += gameTime.ElapsedGameTime.Milliseconds;
            }

            if (IsLeftJustUp()) {
                if (!_dragging)
                    _justClicked = true;
                _dragging = false;
                _mouseDown = false;
            }

            if (IsLeftJustDown()) {
                _mouseDownPosition = Position;
                _mouseDown = true;
                _mouseDownTime = 0;
            }

            // Keep the mouse inside
            if (KeepMouseInWindow) {
                var rect = Game.Window.ClientBounds; // Rectangle to clip (in screen coordinates)
#if WINDOWS
                Cursor.Clip = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
#endif
            }
            else {
#if WINDOWS
                Cursor.Clip = new Rectangle(int.MinValue / 2, int.MinValue / 2, int.MaxValue, int.MaxValue);
#endif
            }
            base.Update(gameTime);
        }

        #endregion

        #region Is Left Just Up/Down/Click

        /// <summary>
        ///     True if the mouse was just pressed, last one frame true.
        /// </summary>
        public bool IsLeftJustDown() {
            return (_state.LeftButton == ButtonState.Pressed && _prevState.LeftButton == ButtonState.Released && HaveFocus);
        }

        /// <summary>
        ///     True if the mouse was just released, last one frame true.
        /// </summary>
        public bool IsLeftJustUp() {
            return (_state.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed && HaveFocus);
        }

        /// <summary>
        ///     True if mouse did a released-pressed-released cycle
        ///     without moving the ClickThreshold.
        /// </summary>
        public bool IsLeftClick() {
            return _justClicked && HaveFocus;
        }

        #endregion

        #region Is Right Just Up/Down/Click

        /// <summary>
        ///     True if the mouse was just pressed, last one frame true.
        /// </summary>
        public bool IsRightJustDown() {
            return (_state.RightButton == ButtonState.Pressed && _prevState.RightButton == ButtonState.Released && HaveFocus);
        }

        /// <summary>
        ///     True if the mouse was just released, last one frame true.
        /// </summary>
        public bool IsRightJustUp() {
            return (_state.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed && HaveFocus);
        }

        #endregion

        #region Dragging

        /// <summary>
        ///     The mouse mave moved the ClickThreshold since it was pressed.
        /// </summary>
        /// <returns></returns>
        public bool IsDragging() {
            return _dragging && HaveFocus;
        }

        /// <summary>
        ///     The mouse just moved the threshold, this will only be true
        ///     for one frame.
        /// </summary>
        public bool IsJustDragging() {
            return _justDragging && HaveFocus;
        }

        #endregion
    }
}
