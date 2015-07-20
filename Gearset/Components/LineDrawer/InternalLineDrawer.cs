using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components {
    /// <summary>   
    /// Draw lines in 3D space, can draw line lists and line strips.
    /// </summary>
    public class InternalLineDrawer : Gear {
        /// <summary>
        /// Sets a maximun of lines we can draw.
        /// </summary>
        const int MaxLineCount = 10000; // Implies a 32k per buffer.

        /// <summary>
        /// Maps an id to an index in the dictionary.
        /// </summary>
        readonly Dictionary<String, int> _persistentLine3DTable = new Dictionary<String, int>();

        /// <summary>
        /// Maps an id to an index in the dictionary.
        /// </summary>
        readonly Dictionary<String, int> _persistentLine2DTable = new Dictionary<String, int>();

        readonly VertexPositionColor[] _persistentVertices3D;
        readonly VertexPositionColor[] _persistentVertices2D;
        readonly VertexPositionColor[] _singleFrameVertices2D;
        readonly VertexPositionColor[] _singleFrameVertices3D;

        /// <summary>
        /// When a persistent line is deleted it's index will be
        /// stored here so the next one can take it.
        /// </summary>
        readonly Queue<int> _freeSpots3D;

        /// <summary>
        /// When a persistent line is deleted it's index will be
        /// stored here so the next one can take it.
        /// </summary>
        readonly Queue<int> _freeSpots2D;

        readonly LineDrawerConfig _config;

        /// <summary>
        /// Number of lines to be drawn on this frame. There
        /// will be twice as vertices in the singleFrameVertices array.
        /// </summary>
        int _singleFrameLine2DCount;

        /// <summary>
        /// Number of lines to be drawn on this frame. There
        /// will be twice as vertices in the singleFrameVertices array.
        /// </summary>
        int _singleFrameLine3DCount;

        /// <summary>
        /// Number of lines to be drawn on this frame. There
        /// will be twice as vertices in the singleFrameVertices array.
        /// </summary>
        int _persistentLine3DCount;

        /// <summary>
        /// Number of lines to be drawn on this frame. There
        /// will be twice as vertices in the singleFrameVertices array.
        /// </summary>
        int _persistentLine2DCount;

        /// <summary>
        /// Defines the way that coordinates will be interpreted in 2D space. Defaults to Screen space.
        /// </summary>
        public CoordinateSpace CoordinateSpace;

        /// <summary>
        /// Gets or sets the config for the Line Drawer.
        /// </summary>
        public virtual LineDrawerConfig Config { get { return _config; } }

        public override void Update(GameTime gameTime) {
            // Make space for new frame data.
            _singleFrameLine2DCount = 0;
            _singleFrameLine3DCount = 0;

            base.Update(gameTime);
        }

        #region Draw

        public override void Draw(GameTime gameTime) {
            // Only draw if we're doing a BasicEffectPass pass
            if (GearsetResources.CurrentRenderPass == RenderPass.BasicEffectPass) {
                // If there are no lines, don't draw anything.
                if (_persistentLine3DCount > 0)
                    GearsetResources.Device.DrawUserPrimitives(PrimitiveType.LineList, _persistentVertices3D, 0, _persistentLine3DCount);

                if (_singleFrameLine3DCount > 0) {
                    GearsetResources.Device.DrawUserPrimitives(PrimitiveType.LineList, _singleFrameVertices3D, 0, _singleFrameLine3DCount);
                    _singleFrameLine3DCount = 0;
                }
            }

            var valid2DPass = (CoordinateSpace == CoordinateSpace.ScreenSpace ? RenderPass.ScreenSpacePass : RenderPass.GameSpacePass);
            if (GearsetResources.CurrentRenderPass == valid2DPass) {
                // If there are no lines, don't draw anything.
                if (_persistentLine2DCount > 0)
                    GearsetResources.Device.DrawUserPrimitives(PrimitiveType.LineList, _persistentVertices2D, 0, _persistentLine2DCount);

                if (_singleFrameLine2DCount > 0) {
                    GearsetResources.Device.DrawUserPrimitives(PrimitiveType.LineList, _singleFrameVertices2D, 0, _singleFrameLine2DCount);
                    _singleFrameLine2DCount = 0;
                }
            }
        }

        #endregion

        #region Constructor

        public InternalLineDrawer()
            : this(new LineDrawerConfig()) {}

        public InternalLineDrawer(LineDrawerConfig config)
            : base(config) {
            _config = config;
            config.Cleared += Config_Cleared;

            _persistentVertices3D = new VertexPositionColor[MaxLineCount * 2];
            _persistentVertices2D = new VertexPositionColor[MaxLineCount * 2];
            _singleFrameVertices2D = new VertexPositionColor[MaxLineCount * 2];
            _singleFrameVertices3D = new VertexPositionColor[MaxLineCount * 2];

            CoordinateSpace = CoordinateSpace.ScreenSpace;

            _freeSpots3D = new Queue<int>();
            _freeSpots2D = new Queue<int>();
        }

        public void Clear() {
            for (var i = 0; i < _persistentVertices3D.Length; i++) {
                _persistentVertices3D[i].Position = Vector3.Zero;
                _persistentVertices3D[i].Color = new Color(_persistentVertices3D[i].Color.R, _persistentVertices3D[i].Color.G, _persistentVertices3D[i].Color.B, 1);
            }

            for (var i = 0; i < _persistentVertices2D.Length; i++) {
                _persistentVertices2D[i].Position = Vector3.Zero;
                _persistentVertices2D[i].Color = new Color(_persistentVertices2D[i].Color.R, _persistentVertices2D[i].Color.G, _persistentVertices2D[i].Color.B, 1);
            }
        }

        void Config_Cleared(object sender, EventArgs e) {
            Clear();
        }

        #endregion

        #region ShowLine (public methods)

        /// <summary>
        /// Draws a line and keeps drawing it, its values can be changed
        /// by calling ShowLine again with the same key. If you want to
        /// make more efficient subsequent calls, get the returned index (
        /// and call it again but with the index overload.
        /// </summary>
        public int ShowLine(String key, Vector3 v1, Vector3 v2, Color color) {
            var index = 0;

            // Do we already know this line?
            if (_persistentLine3DTable.ContainsKey(key)) {
                index = _persistentLine3DTable[key];
                ShowLine(index, v1, v2, color);
            }
            else // We don't know this line, assign a new index to it.
            {
                // Can we take a spot left by other line?
                if (_freeSpots3D.Count > 0) {
                    index = _freeSpots3D.Dequeue();
                    _persistentLine3DTable.Add(key, index);
                    ShowLine(index, v1, v2, color);
                }
                else if (_persistentLine3DCount + 1 < MaxLineCount) {
                    index = (_persistentLine3DCount++) * 2;
                    _persistentLine3DTable.Add(key, index);
                    ShowLine(index, v1, v2, color);
                }
            }
            return index;
        }

        /// <summary>
        /// Draws a line and keeps drawing it, its values can be changed
        /// by calling ShowLine again with the same key. If you want to
        /// make more efficient subsequent calls, get the returned index (
        /// and call it again but with the index overload.
        /// </summary>
        public int ShowLine(String key, Vector2 v1, Vector2 v2, Color color) {
            var index = 0;
            // Do we already know this line?
            if (_persistentLine2DTable.ContainsKey(key)) {
                index = _persistentLine2DTable[key];
                ShowLine(index, v1, v2, color);
            }
            else // We don't know this line, assign a new index to it.
            {
                // Can we take a spot left by other line?
                if (_freeSpots2D.Count > 0) {
                    index = _freeSpots2D.Dequeue();
                    _persistentLine2DTable.Add(key, index);
                    ShowLine(index, v1, v2, color);
                }
                else if (_persistentLine2DCount + 1 < MaxLineCount) {
                    index = (_persistentLine2DCount++) * 2;
                    _persistentLine2DTable.Add(key, index);
                    ShowLine(index, v1, v2, color);
                }
            }
            return index;
        }

        /// <summary>
        /// Use this method only once, then if you want to update the line
        /// use the (int, Vector3, Vector3, Color) overload with the index
        /// returned by this method.
        /// </summary>
        internal int ShowLine(Vector3 v1, Vector3 v2, Color color) {
            int index;
            // Can we take a spot left by other line?
            if (_freeSpots3D.Count > 0)
                index = _freeSpots3D.Dequeue();
            else {
                if ((_persistentLine3DCount + 1) * 2 + 1 >= _persistentVertices3D.Length)
                    return 0;
                index = (_persistentLine3DCount++) * 2;
            }

            ShowLine(index, v1, v2, color);
            return index;
        }

        /// <summary>
        /// Use this method only once, then if you want to update the line
        /// use the (int, Vector2, Vector2, Color) overload with the index
        /// returned by this method.
        /// </summary>
        internal int ShowLine(Vector2 v1, Vector2 v2, Color color) {
            int index;
            // Can we take a spot left by other line?
            if (_freeSpots2D.Count > 0)
                index = _freeSpots2D.Dequeue();
            else {
                if ((_persistentLine2DCount + 1) * 2 + 1 >= _persistentVertices2D.Length)
                    return 0;
                index = (_persistentLine2DCount++) * 2;
            }

            ShowLine(index, v1, v2, color);
            return index;
        }

        /// <summary>
        /// Only use this method if you know you have a valid index. You
        /// can get a valid index by calling the other overlaods.
        /// </summary>
        internal void ShowLine(int index, Vector3 v1, Vector3 v2, Color color) {
            if (index + 1 >= _persistentVertices3D.Length)
                return;
            _persistentVertices3D[index + 0].Position = v1;
            _persistentVertices3D[index + 0].Color = color;
            _persistentVertices3D[index + 1].Position = v2;
            _persistentVertices3D[index + 1].Color = color;
        }

        /// <summary>
        /// Only use this method if you know you have a valid index. You
        /// can get a valid index by calling the other overlaods.
        /// </summary>
        internal void ShowLine(int index, Vector2 v1, Vector2 v2, Color color) {
            if (index + 1 >= _persistentVertices2D.Length)
                return;
            _persistentVertices2D[index + 0].Position = new Vector3(v1, 0);
            _persistentVertices2D[index + 0].Color = color;
            _persistentVertices2D[index + 1].Position = new Vector3(v2, 0);
            _persistentVertices2D[index + 1].Color = color;
        }

        /// <summary>
        /// Draws a line for one frame.
        /// </summary>
        public void ShowLineOnce(Vector3 v1, Vector3 v2, Color color) {
            if (!Visible || GearsetResources.GlobalAlpha <= 0)
                return;
            var index = (_singleFrameLine3DCount++) * 2;
            if (index + 1 >= _singleFrameVertices3D.Length)
                return;
            _singleFrameVertices3D[index + 0].Position = v1;
            _singleFrameVertices3D[index + 0].Color = color;
            _singleFrameVertices3D[index + 1].Position = v2;
            _singleFrameVertices3D[index + 1].Color = color;
        }

        /// <summary>
        /// Draws a line for one frame.
        /// </summary>
        public void ShowLineOnce(Vector2 v1, Vector2 v2, Color color) {
            if (!Visible || GearsetResources.GlobalAlpha <= 0)
                return;
            var index = (_singleFrameLine2DCount++) * 2;
            if (index + 1 >= _singleFrameVertices2D.Length)
                return;
            _singleFrameVertices2D[index + 0].Position = new Vector3(v1, 0);
            _singleFrameVertices2D[index + 0].Color = color;
            _singleFrameVertices2D[index + 1].Position = new Vector3(v2, 0);
            _singleFrameVertices2D[index + 1].Color = color;
        }

        /// <summary>
        /// If a line with the specified key existe, remove it. Else, do nothing.
        /// </summary>
        public void DeleteLine3(String key) {
            if (_persistentLine3DTable.ContainsKey(key)) {
                _freeSpots3D.Enqueue(_persistentLine3DTable[key]);
                _persistentLine3DTable.Remove(key);
            }
        }

        /// <summary>
        /// If a line with the specified key existe, remove it. Else, do nothing.
        /// </summary>
        public void DeleteLine3(int index) {
            // Make the current line invisible
            _persistentVertices3D[index].Color = Color.Transparent;
            _persistentVertices3D[index + 1].Color = Color.Transparent;

            // Let someone else take that index.
            _freeSpots3D.Enqueue(index);
        }

        /// <summary>
        /// If a line with the specified key exists, remove it. Else, do nothing.
        /// </summary>
        public void DeleteLine2(String key) {
            if (_persistentLine2DTable.ContainsKey(key)) {
                _freeSpots2D.Enqueue(_persistentLine2DTable[key]);
                _persistentLine2DTable.Remove(key);
            }
        }

        /// <summary>
        /// If a line with the specified key exists, remove it. Else, do nothing.
        /// </summary>
        public void DeleteLine2(int index) {
            // Make the current line invisible
            _persistentVertices2D[index].Color = Color.Transparent;
            _persistentVertices2D[index + 1].Color = Color.Transparent;

            // Let someone else take that index.
            _freeSpots2D.Enqueue(index);
        }

        #endregion
    }

    public enum CoordinateSpace {
        /// <summary>
        /// The geometry will be interpreted as being in screen space.
        /// </summary>
        ScreenSpace,

        /// <summary>
        /// The geometry will be interpreted as being in game space, thus the Transform2D matrix will be applied.
        /// </summary>
        GameSpace
    }
}
