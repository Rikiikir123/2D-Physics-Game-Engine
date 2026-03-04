using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;

namespace Physics2D.Core.Collision;

/// <summary>
/// Axis-aligned box collider represented by size and local offset.
/// </summary>
public sealed class PhysicsBoxCollider : PhysicsCollider
{
    public PhysicsBoxCollider(Body body, Vec2 size, Vec2 offset) : base(body)
    {
        Size = size;
        Offset = offset;
    }

    public Vec2 Size { get; set; }
    public Vec2 Offset { get; set; }

    public override AABB ComputeAABB()
    {
        var center = Body.Position + Offset;
        var half = Size * 0.5f;
        return new AABB(center - half, center + half);
    }
}
