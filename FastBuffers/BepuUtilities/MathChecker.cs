﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BepuUtilities
{
    /// <summary>
    /// Contains conditional extensions to check for bad values in various structures.
    /// </summary>
    public static class MathChecker
    {
        /// <summary>
        /// Checks a single float for validity.  Separate from the extension method to avoid throwing exceptions deep in a call tree.
        /// </summary>
        /// <param name="f">Value to validate.</param>
        /// <returns>True if the value is invalid, false if it is valid.</returns>
        private static bool IsInvalid(float f)
        {
            return float.IsNaN(f) || float.IsInfinity(f);
        }
        

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this float f)
        {
            if (IsInvalid(f))
            {
                throw new InvalidOperationException("Invalid value.");
            }
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this Vector3 v)
        {
            if (IsInvalid(v.LengthSquared()))
            {
                throw new InvalidOperationException("Invalid value.");
            }
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this Vector4 v)
        {
            if (IsInvalid(v.LengthSquared()))
            {
                throw new InvalidOperationException("Invalid value.");
            }
        }


        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix3x3 m)
        {
            m.X.Validate();
            m.Y.Validate();
            m.Z.Validate();
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix m)
        {
            m.X.Validate();
            m.Y.Validate();
            m.Z.Validate();
            m.W.Validate();
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this Quaternion q)
        {
            if (IsInvalid(q.LengthSquared()))
            {
                throw new InvalidOperationException("Invalid value.");
            }
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this RigidTransform r)
        {
            r.Position.Validate();
            r.Orientation.Validate();
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this AffineTransform a)
        {
            a.LinearTransform.Validate();
            a.Translation.Validate();
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this BoundingBox b)
        {
            b.Min.Validate();
            b.Max.Validate();
        }

        /// <summary>
        /// Checks the value to see if it is a NaN or infinite.  If it is, an exception is thrown.
        /// This is only run when the CHECKMATH symbol is defined.
        /// </summary>
        [Conditional("CHECKMATH")]
        public static void Validate(this BoundingSphere b)
        {
            b.Center.Validate();
            b.Radius.Validate();
        }

        [Conditional("CHECKMATH")]
        public static void Validate(this Vector<float> f, int laneCount = -1)
        {
            if (laneCount < -1 || laneCount > Vector<float>.Count)
                throw new ArgumentException("Invalid lane count.");
            if (laneCount == -1)
                laneCount = Vector<float>.Count;
            ref var casted = ref Unsafe.As<Vector<float>, float>(ref f);
            for (int i = 0; i < laneCount; ++i)
            {
                var value = Unsafe.Add(ref casted, i);
                if (float.IsNaN(value) || float.IsInfinity(value))
                {
                    throw new InvalidOperationException($"Invalid floating point value: {value}.");
                }
            }
        }

        [Conditional("CHECKMATH")]
        public static void Validate(this Vector<float> f, Vector<int> lanesToTest)
        {
            ref var castedValues = ref Unsafe.As<Vector<float>, float>(ref f);
            ref var castedMask = ref Unsafe.As<Vector<int>, int>(ref lanesToTest);
            for (int i = 0; i < Vector<float>.Count; ++i)
            {
                var mask = Unsafe.Add(ref castedMask, i);
                if (mask != 0)
                {
                    var value = Unsafe.Add(ref castedValues, i);
                    if (float.IsNaN(value) || float.IsInfinity(value))
                    {
                        throw new InvalidOperationException($"Invalid floating point value: {value}.");
                    }
                }
            }
        }

        [Conditional("CHECKMATH")]
        public static void Validate(this Vector2Wide v, int laneCount = -1)
        {
            v.X.Validate(laneCount);
            v.Y.Validate(laneCount);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Vector3Wide v, int laneCount = -1)
        {
            v.X.Validate(laneCount);
            v.Y.Validate(laneCount);
            v.Z.Validate(laneCount);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix2x2Wide m, int laneCount = -1)
        {
            m.X.Validate(laneCount);
            m.Y.Validate(laneCount);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix2x3Wide m, int laneCount = -1)
        {
            m.X.Validate(laneCount);
            m.Y.Validate(laneCount);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix3x3Wide m, int laneCount = -1)
        {
            m.X.Validate(laneCount);
            m.Y.Validate(laneCount);
            m.Z.Validate(laneCount);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this QuaternionWide m, int laneCount = -1)
        {
            m.X.Validate(laneCount);
            m.Y.Validate(laneCount);
            m.Z.Validate(laneCount);
            m.W.Validate(laneCount);
        }

        [Conditional("CHECKMATH")]
        public static void Validate(this Vector2Wide v, Vector<int> laneMask)
        {
            v.X.Validate(laneMask);
            v.Y.Validate(laneMask);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Vector3Wide v, Vector<int> laneMask)
        {
            v.X.Validate(laneMask);
            v.Y.Validate(laneMask);
            v.Z.Validate(laneMask);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix2x2Wide m, Vector<int> laneMask)
        {
            m.X.Validate(laneMask);
            m.Y.Validate(laneMask);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix2x3Wide m, Vector<int> laneMask)
        {
            m.X.Validate(laneMask);
            m.Y.Validate(laneMask);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this Matrix3x3Wide m, Vector<int> laneMask)
        {
            m.X.Validate(laneMask);
            m.Y.Validate(laneMask);
            m.Z.Validate(laneMask);
        }
        [Conditional("CHECKMATH")]
        public static void Validate(this QuaternionWide m, Vector<int> laneMask)
        {
            m.X.Validate(laneMask);
            m.Y.Validate(laneMask);
            m.Z.Validate(laneMask);
            m.W.Validate(laneMask);
        }
    }
}
