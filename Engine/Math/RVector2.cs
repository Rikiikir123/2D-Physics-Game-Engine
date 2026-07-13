using System;

namespace Engine.Math
{
    public struct RVector2
    {
        public float X;
        public float Y;

        public RVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        // common constants
        public static RVector2 Zero  => new RVector2( 0f,  0f);
        public static RVector2 Up    => new RVector2( 0f, -1f);  // screen-space: y increases downward
        public static RVector2 Down  => new RVector2( 0f,  1f);
        public static RVector2 Left  => new RVector2(-1f,  0f);
        public static RVector2 Right => new RVector2( 1f,  0f);

        // magnitude of the vector
        public float Length => (float)System.Math.Sqrt(X * X + Y * Y);

        // squared magnitude, avoids the sqrt when only comparing distances
        public float LengthSquared => X * X + Y * Y;

        // unit vector in the same direction, returns Zero if length is zero
        public RVector2 Normalized
        {
            get
            {
                float len = Length;
                if (len < 0.0001f) return Zero;
                return this / len;
            }
        }

        // dot product: measures how much two vectors point in the same direction
        public static float Dot(RVector2 a, RVector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static RVector2 operator +(RVector2 a, RVector2 b)
        {
            return new RVector2(a.X + b.X, a.Y + b.Y);
        }

        public static RVector2 operator -(RVector2 a, RVector2 b)
        {
            return new RVector2(a.X - b.X, a.Y - b.Y);
        }

        public static RVector2 operator -(RVector2 v)
        {
            return new RVector2(-v.X, -v.Y);
        }

        public static RVector2 operator *(RVector2 v, float scalar)
        {
            return new RVector2(v.X * scalar, v.Y * scalar);
        }

        // allows writing scalar * vector as well as vector * scalar
        public static RVector2 operator *(float scalar, RVector2 v)
        {
            return new RVector2(v.X * scalar, v.Y * scalar);
        }

        public static RVector2 operator /(RVector2 v, float scalar)
        {
            return new RVector2(v.X / scalar, v.Y / scalar);
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2})";
        }
    }
}
