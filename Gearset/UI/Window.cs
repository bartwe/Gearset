using Microsoft.Xna.Framework;

namespace Gearset.UI {
    /// <summary>
    /// A Window, with a title bar to show a title and drag it. 
    /// Also a scale nob at the bottom-right corner
    /// </summary>
    public class Window : LayoutBox {
        public Window(Vector2 position, Vector2 clientSize)
            : base(position, clientSize) {
            TitleBar = new LayoutBox(position) { Parent = this };
            ScaleNob = new LayoutBox(position, new Vector2(6)) { Parent = this };

            ScaleNob.Dragged += scaleNob_Dragged;
            TitleBar.Dragged += titleBar_Dragged;
            Dragged += titleBar_Dragged;

            TitleBarSize = 20;
            //UpdateLayout();
        }

        /// <summary>
        /// Gets the title bar LayoutBox
        /// </summary>
        public LayoutBox TitleBar { get; set; }

        /// <summary>
        /// Gets the scale nob LayoutBox.
        /// </summary>
        public LayoutBox ScaleNob { get; set; }

        /// <summary>
        /// Defines a hight for the title bar.
        /// </summary>
        public float TitleBarSize {
            get { return TitleBar.Height; }
            set {
                TitleBar.Height = value;
                UpdateLayout();
            }
        }

        void titleBar_Dragged(object sender, ref Vector2 delta) {
            Position += delta;
            UpdateLayout();
        }

        void scaleNob_Dragged(object sender, ref Vector2 delta) {
            Size += delta;
            UpdateLayout();
        }

        protected override void OnPositionChanged() {
            UpdateLayout();
        }

        protected override void OnSizeChanged() {
            UpdateLayout();
        }

        /// <summary>
        /// Positions (and sizes) the title bar and the scale nob.
        /// </summary>
        public void UpdateLayout() {
            if (TitleBar == null || ScaleNob == null)
                return;

            TitleBar.Top = -TitleBarSize;
            TitleBar.Left = 0;
            TitleBar.Width = Width;
            TitleBar.Height = TitleBarSize;

            ScaleNob.Position = Size + new Vector2(1);
        }
    }
}
