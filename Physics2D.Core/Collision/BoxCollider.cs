using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;

namespace Physics2D.Core.Collision;

/// <summary>
/// Axis-aligned box collider (no rotation support by design).
/// </summary>
public sealed class BoxCollider : Collider
{
    public BoxCollider(Body body, Vec2 size, Vec2 offset) : base(body)
    {
        Size = size;
        Offset = offset;
    }

    public Vec2 Size { get; set; }
    public Vec2 Offset { get; set; }

    public override AABB ComputeAabb()
    {
        var half = Size * 0.5f;
        var center = Body.Position + Offset;
        return new AABB(center - half, center + half);
    }
}
