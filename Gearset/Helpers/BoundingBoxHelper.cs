#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Gearset {
    class BoundingBoxHelper {
        static BasicEffect _effect;
        static Random _random = new Random();
        static VertexPositionColor[] _vertices;

        public static void Initialize() {
            _effect = GearsetResources.Effect;
            _vertices = new VertexPositionColor[24];
        }

        static void InitializeVertices(BoundingBox box, Color color) {
            var corners = box.GetCorners();


            _vertices[0].Position = corners[0];
            _vertices[1].Position = corners[1];
            _vertices[2].Position = corners[1];
            _vertices[3].Position = corners[2];
            _vertices[4].Position = corners[2];
            _vertices[5].Position = corners[3];
            _vertices[6].Position = corners[3];
            _vertices[7].Position = corners[0];

            _vertices[8].Position = corners[4];
            _vertices[9].Position = corners[5];
            _vertices[10].Position = corners[5];
            _vertices[11].Position = corners[6];
            _vertices[12].Position = corners[6];
            _vertices[13].Position = corners[7];
            _vertices[14].Position = corners[7];
            _vertices[15].Position = corners[4];

            _vertices[16].Position = corners[4];
            _vertices[17].Position = corners[0];
            _vertices[18].Position = corners[5];
            _vertices[19].Position = corners[1];
            _vertices[20].Position = corners[6];
            _vertices[21].Position = corners[2];
            _vertices[22].Position = corners[7];
            _vertices[23].Position = corners[3];

            for (var i = 0; i < 24; i++) {
                _vertices[i].Color = color;
            }
        }

        #region Draw

        /// <summary>
        /// Draws White a specified Bounding Box.
        /// </summary>
        public static void DrawBoundingBox(BoundingBox box) {
            DrawBoundingBox(box, Color.White);
        }

        /// <summary>
        /// Draws with the specified color, the specified BBox.
        /// </summary>
        public static void DrawBoundingBox(BoundingBox box, Color color) {
            InitializeVertices(box, color);
            GearsetResources.Device.DrawUserPrimitives(PrimitiveType.LineList, _vertices, 0, 12);
        }

        #endregion
    }
}
