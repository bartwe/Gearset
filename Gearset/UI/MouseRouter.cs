using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gearset.UI {
    /// <summary>
    /// Routes the mouse events in to the appopiate box in the box list.
    /// </summary>
    public class MouseRouter {
        //private UIManager parent;

        readonly List<LayoutBox> _downCollidingSet;
        readonly List<LayoutBox> _upCollidingSet;
        MouseState _prevState;
        MouseState _state;
        List<LayoutBox> _overCollidingSet;

        public MouseRouter() {
            //this.parent = parent;
            _state = new MouseState();
            _downCollidingSet = new List<LayoutBox>();
            _upCollidingSet = new List<LayoutBox>();
            _overCollidingSet = new List<LayoutBox>();
        }

        bool HaveFocus { get { return true; } } // return parent.Game.IsActive; } }

        /// <summary>
        /// True if the mouse was just pressed, last one frame true.
        /// </summary>
        public bool IsLeftJustDown { get { return (_state.LeftButton == ButtonState.Pressed && _prevState.LeftButton == ButtonState.Released && HaveFocus); } }

        /// <summary>
        /// True if the mouse was just released, last one frame true.
        /// </summary>
        public bool IsLeftJustUp { get { return (_state.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed && HaveFocus); } }

        public void Update() {
            _prevState = _state;
            _state = Mouse.GetState();
            var position = new Vector2(_state.X, _state.Y);
            var prevPosition = new Vector2(_prevState.X, _prevState.Y);

            // Check mouse down.
            if (IsLeftJustDown) {
                FindCollidingBoxes(_downCollidingSet, position);
                foreach (var item in _downCollidingSet)
                    item.RaiseMouseDown(item.WorldToLocal(position));
            }
            // Check mouse up, click.
            else if (IsLeftJustUp) {
                FindCollidingBoxes(_upCollidingSet, position);
                foreach (var item in _upCollidingSet) {
                    var local = item.WorldToLocal(position);
                    item.RaiseMouseUp(local);
                    if (_downCollidingSet.Contains(item))
                        item.RaiseClick(local);
                }

                // Forget the down list.
                _downCollidingSet.Clear();
            }

            if (_state.LeftButton == ButtonState.Released) {
                foreach (var box in UiManager.Boxes) {
                    if (box.Contains(position))
                        box.IsMouseOver = true;
                    else
                        box.IsMouseOver = false;
                }
            }

            // Check drag.
            if (position != prevPosition) {
                foreach (var item in _downCollidingSet) {
                    item.RaiseDragged(position - prevPosition);
                }
            }
        }

        void FindCollidingBoxes(List<LayoutBox> collidingSet, Vector2 position) {
            collidingSet.Clear();
            foreach (var box in UiManager.Boxes) {
                if (box.Contains(position))
                    collidingSet.Add(box);
            }
        }
    }
}
