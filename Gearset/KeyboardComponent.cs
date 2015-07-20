using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gearset.Components {
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class KeyboardComponent : Gear {
        KeyboardState _prevState;
        KeyboardState _state;

        public KeyboardComponent()
            : base(new GearConfig()) {}

        /// <summary>
        /// Gets the current state of the keyboard.
        /// </summary>
        public KeyboardState State { get { return _state; } }

        public override void Update(GameTime gameTime) {
            _prevState = _state;
            _state = Keyboard.GetState();
        }

        internal bool IsKeyJustDown(Keys key) {
            return (_state.IsKeyDown(key) && _prevState.IsKeyUp(key));
        }

        internal bool IsKeyDown(Keys key) {
            return (_state.IsKeyDown(key));
        }
    }
}
