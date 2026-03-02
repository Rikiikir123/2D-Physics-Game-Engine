namespace Physics2D.Core.Math;

/// <summary>
/// Lightweight immutable-ish 2D vector for deterministic-style fixed timestep simulation.
/// </summary>
public struct Vec2
{
    public float X;
    public float Y;

    public static readonly Vec2 Zero = new(0f, 0f);
    public static readonly Vec2 One = new(1f, 1f);

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float Length() => MathF.Sqrt((X * X) + (Y * Y));

    public float LengthSquared() => (X * X) + (Y * Y);

    public static float Dot(in Vec2 a, in Vec2 b) => (a.X * b.X) + (a.Y * b.Y);

    public Vec2 Normalize()
    {
        var len = Length();
        if (len <= 1e-6f)
        {
            return Zero;
        }

        return this * (1f / len);
    }

    public Vec2 Perpendicular() => new(-Y, X);

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
    public static Vec2 operator *(float scalar, Vec2 a) => a * scalar;
    public static Vec2 operator /(Vec2 a, float scalar) => new(a.X / scalar, a.Y / scalar);
    public static Vec2 operator -(Vec2 a) => new(-a.X, -a.Y);

    public override string ToString() => $"({X:0.###}, {Y:0.###})";
}
