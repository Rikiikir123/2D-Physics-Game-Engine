using System;

namespace Engine.Math
{
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x,float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0f, 0f);

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator *(Vector2 v, float scalar)
        {
            return new Vector2(v.X * scalar, v.Y * scalar);
        }

    }
}
