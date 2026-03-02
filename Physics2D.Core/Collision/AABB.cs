using Physics2D.Core.Math;

namespace Physics2D.Core.Collision;

/// <summary>
/// Axis-aligned bounding box used for broadphase and narrowphase overlap tests.
/// </summary>
public readonly struct AABB
{
    public Vec2 Min { get; }
    public Vec2 Max { get; }

    public AABB(Vec2 min, Vec2 max)
    {
        Min = min;
        Max = max;
    }

    public bool Overlaps(in AABB other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;
    }

    public Vec2 GetOverlap(in AABB other)
    {
        var overlapX = MathF.Min(Max.X, other.Max.X) - MathF.Max(Min.X, other.Min.X);
        var overlapY = MathF.Min(Max.Y, other.Max.Y) - MathF.Max(Min.Y, other.Min.Y);
        return new Vec2(overlapX, overlapY);
    }
}
