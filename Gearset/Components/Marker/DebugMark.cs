using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components {
    sealed class DebugMark {
        public bool ScreenSpace;
        public Vector2 ScreenSpacePosition;
        public Color Color;
        public int RemainingTime = -1;
        public VertexPositionColor[] Mark;
        public Texture2D Label;
        public MarkType Type { get; set; }

        #region MoveTo

        public void MoveTo(Vector3 position) {
            if (ScreenSpace) {
                ScreenSpacePosition = new Vector2(position.X, position.Y);
            }
            else {
                for (var i = 0; i < Mark.Length; i++)
                    Mark[i].Position = position;

                if (Type == MarkType.Cross) {
                    Mark[0].Position += new Vector3(+0.1f, 0, 0);
                    Mark[1].Position += new Vector3(-0.1f, 0, 0);
                    Mark[2].Position += new Vector3(0, -0.01f, 0);
                    Mark[3].Position += new Vector3(0, +0.01f, 0);
                    Mark[4].Position += new Vector3(0, 0, -0.1f);
                    Mark[5].Position += new Vector3(0, 0, +0.1f);
                }
            }
        }

        #endregion

        #region Constructors

        public DebugMark(Vector3 position, Color color, Texture2D label, bool screenSpace) {
            Initialize(position, color, -1, label, MarkType.Cross, screenSpace);
        }

        public DebugMark(Vector3 position, Color color, Texture2D label, MarkType type, bool screenSpace) {
            Initialize(position, color, -1, label, type, screenSpace);
        }

        void Initialize(Vector3 position, Color color, int time, Texture2D label, MarkType type, bool screenSpace) {
            Color = color;
            RemainingTime = time;
            Label = label;
            Type = type;
            ScreenSpace = screenSpace;

            if (screenSpace) ScreenSpacePosition = new Vector2(position.X, position.Y);
            else {
                if (type == MarkType.Cross) {
                    Mark = new VertexPositionColor[6];
                }
                else {
                    Mark = new VertexPositionColor[1];
                }

                for (var i = 0; i < Mark.Length; i++)
                    Mark[i].Color = color;

                MoveTo(position);
            }
        }

        #endregion
    }
}
