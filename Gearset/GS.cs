using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;

// TODO: You might want to set your own namespace here so it's easier for your
// to access the GS class.

namespace Gearset {
    /// <summary>
    ///     Wrapper for Gearset. It should be used instead of accessing Gearset's
    ///     methods directly as it provides easy removal and threadsafeness.
    /// </summary>
    public static class Gs {
        /// <summary>
        ///     The thread that initialized and owns this class.
        ///     (As a side note: should always be the main thread of the Game class.)
        /// </summary>
        static Thread _ownerThread;

        /// <summary>
        ///     Actions that where queued because the thread that called them was not the owner thread.
        /// </summary>
        static readonly ConcurrentQueue<Action> QueuedActions;

        static bool _initialized;

        static Gs() {
            QueuedActions = new ConcurrentQueue<Action>();
        }

        public static GearConsole Console { get; set; }

        /// <summary>
        ///     This is the component that calls Update and Draw to make Gearset draw.
        ///     You don't need to do anything special with this.
        /// </summary>
        public static GearsetComponent GearsetComponent { get; private set; }

        /// <summary>
        ///     If you're using a transform for your 2D objects (e.g. in the SpriteBatch)
        ///     make sure that Gearset knows about it either by setting it here or using
        ///     the SetMatrices overload.
        /// </summary>
        public static Matrix Transform2D { get { return Console.Transform2D; } set { Console.Transform2D = value; } }

        /// <summary>
        ///     Returns the needle position of the curves in Bender. The game can use this
        ///     value to let designers preview curve animations.
        /// </summary>
        public static float BenderNeedlePosition { get { return Console.BenderNeedlePosition; } }

        /// <summary>
        ///     This is the method you need to work for Gearset to work on your game.
        ///     Remember to call SetMatrices to make Gearset's camera match yours.
        /// </summary>
        /// <param name="game">Your game instance</param>
        [Conditional("USE_GEARSET")]
        public static void Initialize(Game game) {
            // Create the Gearset Component, this will be in charge of
            // Initializing Gearset and Updating/Drawing it every frame.
            GearsetComponent = new GearsetComponent(game);
            game.Components.Add(GearsetComponent);

            // This component updates this sealed class allowing it to process
            // calls from other threads which are queued.
            game.Components.Add(new GearsetWrapperUpdater(game));

            Console = GearsetComponent.Console;
            _ownerThread = Thread.CurrentThread;
            _initialized = true;
        }

        #region Update

        [Conditional("USE_GEARSET")]
        public static void Update(GameTime gameTime) {
            Debug.Assert(SameThread(), "The updating thread must be the same one that initialized this class");

            if (QueuedActions.Count > 0) {
                Action action;
                while (QueuedActions.TryDequeue(out action)) {
                    action();
                }
            }

            Console.Update(gameTime);
        }

        #endregion

        [Conditional("USE_GEARSET")]
        public static void Draw(GameTime gameTime) {
            Console.Draw(gameTime);
        }

        [Conditional("USE_GEARSET")]
        public static void StartFrame(GameTime gameTime) {
            Console.StartFrame();
        }

        /// <summary>
        ///     This sealed class will call update on the Debug sealed class so that it can pump
        ///     queued calls from other threads.
        /// </summary>
        sealed class GearsetWrapperUpdater : GearsetComponentBase {
            public GearsetWrapperUpdater(Game game)
                : base(game) {
                // This is important since the GearsetComponent will have an
                // UpdateOrder of int.MaxValue - 1.
                UpdateOrder = int.MaxValue - 2;
            }

            public override void Update(GameTime gameTime) {
                // If you rename this file. Update this:
                Gs.Update(gameTime);
            }
        }

        #region SetMatrices

        /// <summary>
        ///     Use this method after every Update of your game to update the camera
        ///     matrices so 3D overlays can be drawn correctly.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void SetMatrices(ref Matrix world, ref Matrix view, ref Matrix projection) {
            if (SameThread())
                Console.SetMatrices(ref world, ref view, ref projection);
            else {
                // Capture the parameters for lambda expr.
                var w = world;
                var v = view;
                var p = projection;
                EnqueueAction(() => Console.SetMatrices(ref w, ref v, ref p));
            }
        }

        /// <summary>
        ///     Use this method after every Update of your game to update the camera
        ///     matrices so 3D overlays can be drawn correctly.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void SetMatrices(ref Matrix world, ref Matrix view, ref Matrix projection, ref Matrix transform2D) {
            if (SameThread())
                Console.SetMatrices(ref world, ref view, ref projection, ref transform2D);
            else {
                // Capture the parameters for lambda expr.
                var w = world;
                var v = view;
                var p = projection;
                var t = transform2D;
                EnqueueAction(() => Console.SetMatrices(ref w, ref v, ref p, ref t));
            }
        }

        #endregion

        #region Thread Safe Helpers

        static bool SameThread() {
            return _ownerThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        ///     Wrapper for method execution.
        ///     It checks if the current calling thread is the same that initialized this class.
        ///     If not, then the action is queued to be consumed during the update process.
        /// </summary>
        /// <param name="action"></param>
        static void EnqueueAction(Action action) {
            QueuedActions.Enqueue(action);
        }

        #endregion

        #region Wrappers for Gearset methods

        /// <summary>
        ///     Adds or modifiy a key without value on the overlaid tree view.
        /// </summary>
        /// <param name="key">A dot-separated list of keys.</param>
        [Conditional("USE_GEARSET")]
        public static void Show(String key) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Show(key);
            else
                EnqueueAction(() => Console.Show(key));
        }

        /// <summary>
        ///     Adds or modifies a key/value pair to the overlaid tree view.
        /// </summary>
        /// <param name="key">A dot-separated list of keys.</param>
        /// <param name="value">The value to show.</param>
        [Conditional("USE_GEARSET")]
        public static void Show(String key, object value) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Show(key, value);
            else
                EnqueueAction(() => Console.Show(key, value));
        }

        /// <summary>
        ///     Adds an action button to the bottom of the game window.
        /// </summary>
        /// <param name="name">Name of the action as it will appear on the button.</param>
        /// <param name="action">Action to perform when the button is clicked.</param>
        [Conditional("USE_GEARSET")]
        public static void AddQuickAction(String name, Action action) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.AddQuickAction(name, action);
            else
                EnqueueAction(() => Console.AddQuickAction(name, action));
        }

        /// <summary>
        ///     Adds the provided value to the plot with the provided plotName.
        /// </summary>
        /// <param name="plotName">A name that represent a data set.</param>
        /// <param name="value">The value to add to the sampler</param>
        [Conditional("USE_GEARSET")]
        public static void Plot(String plotName, float value) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Plot(plotName, value);
            else
                EnqueueAction(() => Console.Plot(plotName, value));
        }

        /// <summary>
        ///     Adds the provided value to the plot with the provided plotName. At the same time modifies
        ///     the history length of the sampler.
        /// </summary>
        /// <param name="plotName">A name that represent a data set.</param>
        /// <param name="value">The value to add to the sampler</param>
        /// <param name="historyLength">The number of samples that the sampler will remember at any given time.</param>
        [Conditional("USE_GEARSET")]
        public static void Plot(String plotName, float value, int historyLength) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Plot(plotName, value, historyLength);
            else
                EnqueueAction(() => Console.Plot(plotName, value, historyLength));
        }

        /// <summary>
        ///     Los a message to the specified stream.
        /// </summary>
        /// <param name="streamName">Name of the Stream to log the message to</param>
        /// <param name="content">Message to log</param>
        [Conditional("USE_GEARSET")]
        public static void Log(String streamName, String content) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Log(streamName, content);
            else
                EnqueueAction(() => Console.Log(streamName, content));
        }

        /// <summary>
        ///     Logs the specified message in the default stream.
        /// </summary>
        /// <param name="content">The message to log.</param>
        [Conditional("USE_GEARSET")]
        public static void Log(String content) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Log(content);
            else
                EnqueueAction(() => Console.Log(content));
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The first format parameter</param>
        [Conditional("USE_GEARSET")]
        public static void Log(String streamName, String format, Object arg0) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Log(streamName, format, arg0);
            else
                EnqueueAction(() => Console.Log(streamName, format, arg0));
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The first format parameter</param>
        /// <param name="arg1">The second format parameter</param>
        [Conditional("USE_GEARSET")]
        public static void Log(String streamName, String format, Object arg0, Object arg1) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Log(streamName, format, arg0, arg1);
            else
                EnqueueAction(() => Console.Log(streamName, format, arg0, arg1));
        }

        /// <summary>
        ///     Logs a formatted string to the specified stream.
        /// </summary>
        /// <param name="streamName">Stream to log to</param>
        /// <param name="format">The format string</param>
        /// <param name="arg0">The first format parameter</param>
        /// <param name="arg1">The second format parameter</param>
        /// <param name="arg2">The third format parameter</param>
        [Conditional("USE_GEARSET")]
        public static void Log(String streamName, String format, Object arg0, Object arg1, Object arg2) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Log(streamName, format, arg0, arg1, arg2);
            else
                EnqueueAction(() => Console.Log(streamName, format, arg0, arg1, arg2));
        }

        [Conditional("USE_GEARSET")]
        public static void Log(String streamName, String format, params Object[] args) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Log(streamName, format, args);
            else
                EnqueueAction(() => Console.Log(streamName, format, args));
        }

        /// <summary>
        ///     Shows a dialog asking for a filename and saves the log to the specified file.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void SaveLogToFile() {
            if (!_initialized)
                return;
            if (SameThread())
                Console.SaveLogToFile();
            else
                EnqueueAction(() => Console.SaveLogToFile());
        }

        /// <summary>
        ///     Saves the log to the specified file.
        /// </summary>
        /// <param name="filename">Name of the file to save the log (usually ending in .log)</param>
        [Conditional("USE_GEARSET")]
        public static void SaveLogToFile(string filename) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.SaveLogToFile(filename);
            else
                EnqueueAction(() => Console.SaveLogToFile(filename));
        }

        /// <summary>
        ///     This is an experimental feature.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowMark(String key, Vector3 position, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowMark(key, position, color);
            else
                EnqueueAction(() => Console.ShowMark(key, position, color));
        }

        /// <summary>
        ///     This is an experimental feature.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowMark(String key, Vector3 position) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowMark(key, position);
            else
                EnqueueAction(() => Console.ShowMark(key, position));
        }

        /// <summary>
        ///     This is an experimental feature.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowMark(String key, Vector2 position, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowMark(key, position, color);
            else
                EnqueueAction(() => Console.ShowMark(key, position, color));
        }

        /// <summary>
        ///     This is an experimental feature.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowMark(String key, Vector2 position) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowMark(key, position);
            else
                EnqueueAction(() => Console.ShowMark(key, position));
        }

        /// <summary>
        ///     Shows huge text on the center of the screen which fades
        ///     out quickly.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void Alert(String message) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Alert(message);
            else
                EnqueueAction(() => Console.Alert(message));
        }

        /// <summary>
        ///     Draws a line between two points.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLine(String key, Vector3 v1, Vector3 v2) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLine(key, v1, v2);
            else
                EnqueueAction(() => Console.ShowLine(key, v1, v2));
        }

        /// <summary>
        ///     Draws a line between two points.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLine(String key, Vector3 v1, Vector3 v2, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLine(key, v1, v2, color);
            else
                EnqueueAction(() => Console.ShowLine(key, v1, v2, color));
        }

        /// <summary>
        ///     Draws a line between two points for one frame.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLineOnce(Vector3 v1, Vector3 v2) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLineOnce(v1, v2);
            else
                EnqueueAction(() => Console.ShowLineOnce(v1, v2));
        }

        /// <summary>
        ///     Draws a line between two points for one frame.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLineOnce(Vector3 v1, Vector3 v2, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLineOnce(v1, v2, color);
            else
                EnqueueAction(() => Console.ShowLineOnce(v1, v2, color));
        }

        /// <summary>
        ///     Draws a line between two points.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLine(String key, Vector2 v1, Vector2 v2) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLine(key, v1, v2);
            else
                EnqueueAction(() => Console.ShowLine(key, v1, v2));
        }

        /// <summary>
        ///     Draws a line between two points.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLine(String key, Vector2 v1, Vector2 v2, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLine(key, v1, v2, color);
            else
                EnqueueAction(() => Console.ShowLine(key, v1, v2, color));
        }

        /// <summary>
        ///     Draws a line between two points for one frame.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLineOnce(Vector2 v1, Vector2 v2) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLineOnce(v1, v2);
            else
                EnqueueAction(() => Console.ShowLineOnce(v1, v2));
        }

        /// <summary>
        ///     Draws a line between two points for one frame.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowLineOnce(Vector2 v1, Vector2 v2, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLineOnce(v1, v2, color);
            else
                EnqueueAction(() => Console.ShowLineOnce(v1, v2, color));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box.
        ///     <param name="key">Name of the persistent box</param>
        ///     <param name="box">The box to draw</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBox(String key, BoundingBox box) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBox(key, box);
            else
                EnqueueAction(() => Console.ShowBox(key, box));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box.
        ///     <param name="min">Minimum values of the box in each axis</param>
        ///     <param name="max">Maximum values of the box in each axis</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBox(String key, Vector3 min, Vector3 max) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBox(key, min, max);
            else
                EnqueueAction(() => Console.ShowBox(key, min, max));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box.
        ///     <param name="key">Name of the persistent box</param>
        ///     <param name="box">The box to draw</param>
        ///     <param name="color">The color that will be used to draw the box</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBox(String key, BoundingBox box, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBox(key, box, color);
            else
                EnqueueAction(() => Console.ShowBox(key, box, color));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box.
        ///     <param name="min">Minimum values of the box in each axis</param>
        ///     <param name="max">Maximum values of the box in each axis</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBox(String key, Vector3 min, Vector3 max, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBox(key, min, max, color);
            else
                EnqueueAction(() => Console.ShowBox(key, min, max, color));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box for one frame.
        ///     <param name="box">The BoundingBox to draw</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBoxOnce(BoundingBox box) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBoxOnce(box);
            else
                EnqueueAction(() => Console.ShowBoxOnce(box));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box for one frame.
        ///     <param name="min">Minimum values of the box in each axis</param>
        ///     <param name="max">Maximum values of the box in each axis</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBoxOnce(Vector3 min, Vector3 max) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBoxOnce(min, max);
            else
                EnqueueAction(() => Console.ShowBoxOnce(min, max));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box for one frame.
        ///     <param name="box">The BoundingBox to draw</param>
        ///     <param name="color">The color that will be used to draw the box</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBoxOnce(BoundingBox box, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBoxOnce(box, color);
            else
                EnqueueAction(() => Console.ShowBoxOnce(box, color));
        }

        /// <summary>
        ///     Shows an axis aligned bounding box for one frame.
        ///     <param name="min">Minimum values of the box in each axis</param>
        ///     <param name="max">Maximum values of the box in each axis</param>
        ///     <param name="color">The color that will be used to draw the box</param>
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ShowBoxOnce(Vector3 min, Vector3 max, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowBoxOnce(min, max, color);
            else
                EnqueueAction(() => Console.ShowBoxOnce(min, max, color));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphere(String key, BoundingSphere sphere) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphere(key, sphere);
            else
                EnqueueAction(() => Console.ShowSphere(key, sphere));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphere(String key, Vector3 center, float radius) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphere(key, center, radius);
            else
                EnqueueAction(() => Console.ShowSphere(key, center, radius));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphere(String key, BoundingSphere sphere, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphere(key, sphere, color);
            else
                EnqueueAction(() => Console.ShowSphere(key, sphere, color));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphere(String key, Vector3 center, float radius, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphere(key, center, radius, color);
            else
                EnqueueAction(() => Console.ShowSphere(key, center, radius, color));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphereOnce(BoundingSphere sphere) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphereOnce(sphere);
            else
                EnqueueAction(() => Console.ShowSphereOnce(sphere));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphereOnce(Vector3 center, float radius) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphereOnce(center, radius);
            else
                EnqueueAction(() => Console.ShowSphereOnce(center, radius));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowCylinderOnce(Vector3 center, Vector3 radius) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowCylinderOnce(center, radius);
            else
                EnqueueAction(() => Console.ShowCylinderOnce(center, radius));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowCylinderOnce(Vector3 center, Vector3 radius, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowCylinderOnce(center, radius, color);
            else
                EnqueueAction(() => Console.ShowCylinderOnce(center, radius, color));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphereOnce(BoundingSphere sphere, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphereOnce(sphere, color);
            else
                EnqueueAction(() => Console.ShowSphereOnce(sphere, color));
        }

        [Conditional("USE_GEARSET")]
        public static void ShowSphereOnce(Vector3 center, float radius, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowSphereOnce(center, radius, color);
            else
                EnqueueAction(() => Console.ShowSphereOnce(center, radius, color));
        }

        /// <summary>
        ///     Shows a label at the specified position (the text will be the label's name).
        /// </summary>
        /// <param name="name">
        ///     Name of the label as well of the text to show. Subsequent calls with the same name will modify this
        ///     label
        /// </param>
        /// <param name="position">Position where the label will be shown</param>
        [Conditional("USE_GEARSET")]
        public static void ShowLabel(String name, Vector2 position) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLabel(name, position);
            else
                EnqueueAction(() => Console.ShowLabel(name, position));
        }

        /// <summary>
        ///     Shows a label at the specified positon that displays the specified text.
        /// </summary>
        /// <param name="name">Name of the label. Subsequent calls with the same name will modify this label</param>
        /// <param name="position">Position of the label</param>
        /// <param name="text">Text to show on the label</param>
        [Conditional("USE_GEARSET")]
        public static void ShowLabel(String name, Vector2 position, String text) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLabel(name, position, text);
            else
                EnqueueAction(() => Console.ShowLabel(name, position, text));
        }

        /// <summary>
        ///     Shows a label at the specified positon that displays the specified text.
        /// </summary>
        /// <param name="name">Name of the label. Subsequent calls with the same name will modify this label</param>
        /// <param name="position">Position of the label</param>
        /// <param name="text">Text to show on the label</param>
        /// <param name="color">Color of the text</param>
        [Conditional("USE_GEARSET")]
        public static void ShowLabel(String name, Vector2 position, String text, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLabel(name, position, text, color);
            else
                EnqueueAction(() => Console.ShowLabel(name, position, text, color));
        }

        /// <summary>
        ///     Shows a label at the specified position (the text will be the label's name).
        /// </summary>
        /// <param name="name">
        ///     Name of the label as well of the text to show. Subsequent calls with the same name will modify this
        ///     label
        /// </param>
        /// <param name="position">Position where the label will be shown</param>
        [Conditional("USE_GEARSET")]
        public static void ShowLabel(String name, Vector3 position) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLabel(name, position);
            else
                EnqueueAction(() => Console.ShowLabel(name, position));
        }

        /// <summary>
        ///     Shows a label at the specified positon that displays the specified text.
        /// </summary>
        /// <param name="name">Name of the label. Subsequent calls with the same name will modify this label</param>
        /// <param name="position">Position of the label</param>
        /// <param name="text">Text to show on the label</param>
        [Conditional("USE_GEARSET")]
        public static void ShowLabel(String name, Vector3 position, String text) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLabel(name, position, text);
            else
                EnqueueAction(() => Console.ShowLabel(name, position, text));
        }

        /// <summary>
        ///     Shows a label at the specified positon that displays the specified text.
        /// </summary>
        /// <param name="name">Name of the label. Subsequent calls with the same name will modify this label</param>
        /// <param name="position">Position of the label</param>
        /// <param name="text">Text to show on the label</param>
        /// <param name="color">Color of the text</param>
        [Conditional("USE_GEARSET")]
        public static void ShowLabel(String name, Vector3 position, String text, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowLabel(name, position, text, color);
            else
                EnqueueAction(() => Console.ShowLabel(name, position, text, color));
        }

        /// <summary>
        ///     Sends an object to the Inspector window.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void Inspect(String name, Object o) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Inspect(name, o);
            else
                EnqueueAction(() => Console.Inspect(name, o));
        }

        /// <summary>
        ///     Sends an object to the Inspector window.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void Inspect(String name, Object o, bool autoExpand) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.Inspect(name, o, autoExpand);
            else
                EnqueueAction(() => Console.Inspect(name, o, autoExpand));
        }

        /// <summary>
        ///     Removes an object from the Inspector window.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void RemoveInspect(Object o) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.RemoveInspect(o);
            else
                EnqueueAction(() => Console.RemoveInspect(o));
        }

        /// <summary>
        ///     Clears the Inspector Window.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ClearInspector() {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ClearInspector();
            else
                EnqueueAction(() => Console.ClearInspector());
        }

        /// <summary>
        ///     Sets the function that is used by Gearset when a query is written to the
        ///     Finder by the user. It usually searches through your game objects and returns
        ///     a collection of the ones whose name or Type matches the query.
        ///     A search function receives a String and return IEnumerable (e.g. a List)
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void SetFinderSearchFunction(SearchFunction searchFunction) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.SetFinderSearchFunction(searchFunction);
            else
                EnqueueAction(() => Console.SetFinderSearchFunction(searchFunction));
        }

        /// <summary>
        ///     Shows a persistent Matrix Transform on the screen as 3 orthogonal vectors.
        /// </summary>
        /// <param name="name">Name of the persistent Matrix</param>
        /// <param name="transform">Transform to draw</param>
        /// <param name="axisScale">Scale to apply to each axis</param>
        [Conditional("USE_GEARSET")]
        public static void ShowTransform(String name, Matrix transform, float axisScale) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowTransform(name, transform, axisScale);
            else
                EnqueueAction(() => Console.ShowTransform(name, transform, axisScale));
        }

        /// <summary>
        ///     Shows a persistent Matrix Transform on the screen as 3 orthogonal vectors.
        /// </summary>
        /// <param name="name">Name of the persistent Matrix</param>
        /// <param name="transform">Transform to draw</param>
        [Conditional("USE_GEARSET")]
        public static void ShowTransform(String name, Matrix transform) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowTransform(name, transform);
            else
                EnqueueAction(() => Console.ShowTransform(name, transform));
        }

        /// <summary>
        ///     Shows a one-frame Matrix Transform on the screen as 3 orthogonal vectors.
        /// </summary>
        /// <param name="transform">Transform to draw</param>
        [Conditional("USE_GEARSET")]
        public static void ShowTransformOnce(Matrix transform) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowTransformOnce(transform);
            else
                EnqueueAction(() => Console.ShowTransformOnce(transform));
        }

        /// <summary>
        ///     Shows a one-frame Matrix Transform on the screen as 3 orthogonal vectors.
        /// </summary>
        /// <param name="transform">Transform to draw</param>
        /// <param name="axisScale">Scale to apply to each axis</param>
        [Conditional("USE_GEARSET")]
        public static void ShowTransformOnce(Matrix transform, float axisScale) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowTransformOnce(transform, axisScale);
            else
                EnqueueAction(() => Console.ShowTransformOnce(transform, axisScale));
        }

        /// <summary>
        ///     Shows a persistent Vector3 on the screen as an arrow.
        /// </summary>
        /// <param name="name">Name of the persistent Vector</param>
        /// <param name="location">Location of the vector to draw (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        /// <param name="color">Color of the arrow to draw</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector3(String name, Vector3 location, Vector3 vector, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector3(name, location, vector, color);
            else
                EnqueueAction(() => Console.ShowVector3(name, location, vector, color));
        }

        /// <summary>
        ///     Shows a persistent Vector3 on the screen as an arrow.
        /// </summary>
        /// <param name="name">Name of the persistent Vector</param>
        /// <param name="location">Location of the vector to draw (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector3(String name, Vector3 location, Vector3 vector) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector3(name, location, vector);
            else
                EnqueueAction(() => Console.ShowVector3(name, location, vector));
        }

        /// <summary>
        ///     Shows a Vector3 on the screen as an arrow for one frame.
        /// </summary>
        /// <param name="location">Location of the vector to show (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector3Once(Vector3 location, Vector3 vector) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector3Once(location, vector);
            else
                EnqueueAction(() => Console.ShowVector3Once(location, vector));
        }

        /// <summary>
        ///     Shows a Vector3 on the screen as an arrow for one frame.
        /// </summary>
        /// <param name="location">Location of the vector to show (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        /// <param name="color">Color of the arrow to draw</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector3Once(Vector3 location, Vector3 vector, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector3Once(location, vector, color);
            else
                EnqueueAction(() => Console.ShowVector3Once(location, vector, color));
        }

        /// <summary>
        ///     Shows a persistent Vector2 on the screen as an arrow (Screen space coordinates).
        /// </summary>
        /// <param name="name">Name of the persistent Vector</param>
        /// <param name="location">Location of the vector to draw (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        /// <param name="color">Color of the arrow to draw</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector2(String name, Vector2 location, Vector2 vector, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector2(name, location, vector, color);
            else
                EnqueueAction(() => Console.ShowVector2(name, location, vector, color));
        }

        /// <summary>
        ///     Shows a persistent Vector2 on the screen as an arrow (Screen space coordinates).
        /// </summary>
        /// <param name="name">Name of the persistent Vector</param>
        /// <param name="location">Location of the vector to draw (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector2(String name, Vector2 location, Vector2 vector) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector2(name, location, vector);
            else
                EnqueueAction(() => Console.ShowVector2(name, location, vector));
        }

        /// <summary>
        ///     Shows a Vector2 on the screen as an arrow for one frame (Screen space coordinates).
        /// </summary>
        /// <param name="location">Location of the vector to show (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector2Once(Vector2 location, Vector2 vector) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector2Once(location, vector);
            else
                EnqueueAction(() => Console.ShowVector2Once(location, vector));
        }

        /// <summary>
        ///     Shows a Vector2 on the screen as an arrow for one frame.
        /// </summary>
        /// <param name="location">Location of the vector to show (i.e. position of the start of the arrow)</param>
        /// <param name="vector">Vector to show</param>
        /// <param name="color">Color of the arrow to draw</param>
        [Conditional("USE_GEARSET")]
        public static void ShowVector2Once(Vector2 location, Vector2 vector, Color color) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ShowVector2Once(location, vector, color);
            else
                EnqueueAction(() => Console.ShowVector2Once(location, vector, color));
        }

        /// <summary>
        ///     Adds a curve for editing in Bender
        /// </summary>
        /// <param name="name">Name of the curve to add. Group using dot separators.</param>
        /// <param name="curve">Curve to edit in Bender.</param>
        [Conditional("USE_GEARSET")]
        public static void AddCurve(String name, Curve curve) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.AddCurve(name, curve);
            else
                EnqueueAction(() => Console.AddCurve(name, curve));
        }

        /// <summary>
        ///     Removes the provided curve from Bender.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void RemoveCurve(Curve curve) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.RemoveCurve(curve);
            else
                EnqueueAction(() => Console.RemoveCurve(curve));
        }

        /// <summary>
        ///     Removes a Curve or a Group by name. The complete dot-separated
        ///     path to the curve or group must be given.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void RemoveCurveOrGroup(String name) {
            if (!_initialized)
                return;
            if (SameThread())
                Console.RemoveCurveOrGroup(name);
            else
                EnqueueAction(() => Console.RemoveCurveOrGroup(name));
        }

        /// <summary>
        ///     Clears all Gearset Components erasing all retained data. Inspector and Logger won't be cleared.
        /// </summary>
        [Conditional("USE_GEARSET")]
        public static void ClearAll() {
            if (!_initialized)
                return;
            if (SameThread())
                Console.ClearAll();
            else
                EnqueueAction(() => Console.ClearAll());
        }

        #endregion
    }
}
