﻿using System;
using System.Windows;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    ///     Wraps a CurveKey, giving it an ID and a reference to it's owner curve.
    /// </summary>
    public sealed class KeyWrapper {
        public static long LatestId;
        CurveKey _key;

        /// <summary>
        ///     Initializes a KeyWrapper
        /// </summary>
        /// <param name="curveKey">The key to wrap</param>
        /// <param name="curve">The curve that owns curveKey</param>
        /// <param name="id">Pass -1 to autogenerate a new id</param>
        public KeyWrapper(CurveKey curveKey, CurveWrapper curve, KeyTangentMode tangentMode, long id = -1) {
            if (id < 0)
                Id = LatestId++;
            else
                Id = id;
            Curve = curve;
            Key = curveKey;
            TangentInMode = tangentMode;
            TangentOutMode = tangentMode;
        }

        /// <summary>
        ///     Wrapped CurveKey instance.
        /// </summary>
        public CurveKey Key {
            get { return _key; }
            internal set {
                if (_key != null)
                    Curve.Control.RemoveKeyFromMap(_key);
                _key = value;
                Curve.Control.UpdateKeyMap(this, _key);
            }
        }

        /// <summary>
        ///     The curve where the wrapped CurveKey belongs.
        /// </summary>
        public CurveWrapper Curve { get; internal set; }

        /// <summary>
        ///     Id of the curve, this is used by undo commands to reference
        ///     a modification to the wrapped key.
        /// </summary>
        public long Id { get; internal set; }

        /// <summary>
        ///     Determines whether this key is selected or not.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Determines the way the in tangent is handled.
        /// </summary>
        public KeyTangentMode TangentInMode { get; set; }

        /// <summary>
        ///     Determines the way the out Tangent is handled.
        /// </summary>
        public KeyTangentMode TangentOutMode { get; set; }

        /// <summary>
        ///     Moves the key by the given offset. It will actually remove the old key
        ///     from the curve and add a new offseted curve.
        /// </summary>
        public void MoveKey(float positionOffset, float valueOffset) {
            // Copy the old key with offseted values.
            var newKey = new CurveKey(Key.Position + positionOffset, Key.Value + valueOffset);
            newKey.Continuity = Key.Continuity;
            newKey.TangentIn = Key.TangentIn;
            newKey.TangentOut = Key.TangentOut;

            // Add it to the curve.
            Curve.Curve.Keys.Remove(Key);
            Curve.Curve.Keys.Add(newKey);
            Key = newKey;
        }

        /// <summary>
        ///     Computes the tangent of this key (if set to some automatic mode). It will also
        ///     recomute the tangents of adjacent keys if they're also auto.
        /// </summary>
        public void ComputeTangentIfAuto() {
            if (IsTangentModeAuto(TangentInMode) || IsTangentModeAuto(TangentOutMode)) {
                var index = GetIndex();
                var savedIn = Key.TangentIn;
                var savedOut = Key.TangentOut;
                Curve.Curve.ComputeTangent(index, (CurveTangent)((int)TangentInMode), (CurveTangent)((int)TangentOutMode));

                // If one of the tangent was not auto, restore it.
                if (!IsTangentModeAuto(TangentInMode))
                    Key.TangentIn = savedIn;
                if (!IsTangentModeAuto(TangentOutMode))
                    Key.TangentOut = savedOut;

                // There's a possibility that the tangents might have become NaN if the two keys
                // where at the same position.
                if (float.IsNaN(Key.TangentIn))
                    Key.TangentIn = 0;
                if (float.IsNaN(Key.TangentOut))
                    Key.TangentOut = 0;
            }
        }

        /// <summary>
        ///     Returns true if the passed KeyTangentMode is automatically, i.e. it is
        ///     supposed to be calculated everytime the key is moved.
        /// </summary>
        public bool IsTangentModeAuto(KeyTangentMode tangentMode) {
            return tangentMode == KeyTangentMode.Smooth || tangentMode == KeyTangentMode.Linear || tangentMode == KeyTangentMode.Flat;
        }

        /// <summary>
        ///     Returns the position of the key in curve coords.
        /// </summary>
        /// <param name="control"></param>
        public Point GetPosition() {
            // Get the key position in screen coords.
            return new Point(Key.Position, Key.Value);
        }

        /// <summary>
        ///     Gets the positions of the tangent handles in screen coords.
        /// </summary>
        public void GetTangentHandleScreenPositions(out Point inHandle, out Point outHandle) {
            float distancePrev, distanceNext;
            GetPrevAndNextDistance(out distancePrev, out distanceNext);

            double aspect = (Curve.Control.MaxX - Curve.Control.MinX) / (Curve.Control.MaxY - Curve.Control.MinY);
            var inAngle = Math.Atan(Key.TangentIn * aspect / distancePrev);
            var outAngle = Math.Atan(Key.TangentOut * aspect / distanceNext);
            var handleLength = CurveEditorControl2.TangentHandleLength;

            var handleIn = new Point(Math.Cos(inAngle), Math.Sin(inAngle));
            var handleOut = new Point(Math.Cos(outAngle), Math.Sin(outAngle));

            //handleIn = Curve.Control.CurveCoordsToScreenNormal(ref handleIn);
            //handleOut = Curve.Control.CurveCoordsToScreenNormal(ref handleOut);

            //handleIn.Y *= aspect;
            //handleOut.Y *= aspect;

            // Normalize
            handleIn.X *= 1 / ((Vector)handleIn).Length;
            handleIn.Y *= 1 / ((Vector)handleIn).Length;
            handleOut.X *= 1 / ((Vector)handleOut).Length;
            handleOut.Y *= 1 / ((Vector)handleOut).Length;

            handleIn.X *= handleLength;
            handleIn.Y *= handleLength;
            handleOut.X *= handleLength;
            handleOut.Y *= handleLength;
            if (double.IsNaN(handleIn.X) || double.IsNaN(handleIn.Y)) {
                handleIn.X = 0;
                handleIn.Y = handleLength;
            }
            if (double.IsNaN(handleOut.X) || double.IsNaN(handleOut.Y)) {
                handleOut.X = 0;
                handleOut.Y = handleLength;
            }

            var screenPos = GetPosition();
            screenPos = Curve.Control.CurveCoordsToScreen(ref screenPos);
            inHandle = new Point((int)(screenPos.X - handleIn.X), (int)(screenPos.Y + handleIn.Y));
            outHandle = new Point((int)(screenPos.X + handleOut.X), (int)(screenPos.Y - handleOut.Y));
        }

        /// <summary>
        ///     Get distance between given index key position and previous/next key. Based
        ///     on XNA's curve editor method.
        /// </summary>
        public void GetPrevAndNextDistance(out float prev, out float next) {
            prev = next = 1;
            var keys = Curve.Curve.Keys;
            var index = 0;

            // Find the index of the key provided.
            for (var i = 0; i < keys.Count; i++) {
                if (keys[i] == Key)
                    index = i;
            }

            // From previous key.
            if (index > 0)
                prev = keys[index].Position - keys[index - 1].Position;
            else if (Curve.Curve.PreLoop == CurveLoopType.Oscillate && keys.Count > 1)
                prev = keys[1].Position - keys[0].Position;

            // From next key.
            if (index < keys.Count - 1)
                next = keys[index + 1].Position - keys[index].Position;
            else if (Curve.Curve.PostLoop == CurveLoopType.Oscillate && keys.Count > 1)
                next = keys[index].Position - keys[index - 1].Position;
        }

        internal int GetIndex() {
            for (var i = 0; i < Curve.Curve.Keys.Count; i++) {
                if (ReferenceEquals(Curve.Curve.Keys[i], Key))
                    return i;
            }

            throw new InvalidOperationException("Inconsistency found, key is not in curve.");
        }

        internal void SetInTangent(float tangent, bool computeTangents = true) {
            Key.TangentIn = tangent;
            TangentInMode = KeyTangentMode.Custom;
            if (computeTangents)
                Curve.ComputeTangents();
        }

        internal void SetOutTangent(float tangent, bool computeTangents = true) {
            Key.TangentOut = tangent;
            TangentOutMode = KeyTangentMode.Custom;
            if (computeTangents)
                Curve.ComputeTangents();
        }
    }
}
