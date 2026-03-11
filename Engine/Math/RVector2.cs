using System;

namespace Engine.Math
{
    public struct RVector2
    {
        public float X;
        public float Y;

        public RVector2(float x,float y)
        {
            X = x;
            Y = y;
        }

        public static RVector2 Zero => new RVector2(0f, 0f);

        public static RVector2 operator +(RVector2 a, RVector2 b)
        {
            return new RVector2(a.X + b.X, a.Y + b.Y);
        }
        public static RVector2 operator -(RVector2 a, RVector2 b)
        {
            return new RVector2(a.X - b.X, a.Y - b.Y);
        }

        public static RVector2 operator *(RVector2 v, float scalar)
        {
            return new RVector2(v.X * scalar, v.Y * scalar);
        }
        public static RVector2 operator /(RVector2 v, float scalar)
        {
            return new RVector2(v.X / scalar, v.Y / scalar);
        }

    }
}
