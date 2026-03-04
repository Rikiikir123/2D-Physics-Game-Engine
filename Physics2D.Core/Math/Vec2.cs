using System.Runtime.CompilerServices;

namespace Physics2D.Core.Math;

/// <summary>
/// Lightweight 2D vector used by the custom physics engine.
/// </summary>
public readonly struct Vec2
{
    public static readonly Vec2 Zero = new(0f, 0f);
    public static readonly Vec2 One = new(1f, 1f);

    public readonly float X;
    public readonly float Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float Length => System.MathF.Sqrt((X * X) + (Y * Y));
    public float LengthSquared => (X * X) + (Y * Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Dot(in Vec2 other) => (X * other.X) + (Y * other.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec2 Perpendicular() => new(-Y, X);

    public Vec2 Normalized()
    {
        var len = Length;
        if (len <= 1e-6f)
        {
            return Zero;
        }

        var inv = 1f / len;
        return new Vec2(X * inv, Y * inv);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator +(in Vec2 a, in Vec2 b) => new(a.X + b.X, a.Y + b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(in Vec2 a, in Vec2 b) => new(a.X - b.X, a.Y - b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(in Vec2 v, float s) => new(v.X * s, v.Y * s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(float s, in Vec2 v) => new(v.X * s, v.Y * s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator /(in Vec2 v, float s) => new(v.X / s, v.Y / s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(in Vec2 v) => new(-v.X, -v.Y);

    public override string ToString() => $"({X:0.###}, {Y:0.###})";
}
