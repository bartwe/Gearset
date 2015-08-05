using System.Windows.Forms;
using Gearset.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if WINDOWS
using Gearset.About;

#endif

namespace Gearset {
    static class GearsetResources {
        internal static SpriteBatch SpriteBatch;
        internal static BasicEffect Effect;
        internal static BasicEffect Effect2D;
        internal static RenderPass CurrentRenderPass;
        internal static SpriteFont Font;
        internal static SpriteFont FontTiny;
        internal static SpriteFont FontAlert;
        internal static ResourceContentManager Content;
        internal static GearConsole Console;
        internal static float GlobalAlpha;
        internal static MouseComponent Mouse;
        internal static KeyboardComponent Keyboard;
        internal static GraphicsDevice Device { get { return Game.GraphicsDevice; } }
        // Here's the data that is provided by the game.

        #region Game specific data

        public static Game Game { get; internal set; }
        internal static Matrix World;
        internal static Matrix View;
        internal static Matrix Projection;
        internal static Matrix Transform2D;

        #endregion

#if WINDOWS
        internal static Form GameWindow;
        internal static AboutWindow AboutWindow;
        internal static AboutViewModel AboutViewModel;
#endif
    }
}
