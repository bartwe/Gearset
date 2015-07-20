using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gearset.UI {
    public static class UiManager {
        static readonly MouseRouter MouseRouter;

        static UiManager() {
            Boxes = new List<LayoutBox>();
            MouseRouter = new MouseRouter();
        }

        public static List<LayoutBox> Boxes { get; private set; }

        public static void Update(GameTime gameTime) {
            MouseRouter.Update();
        }
    }
}
