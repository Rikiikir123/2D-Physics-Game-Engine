using Physics2D.Core.Dynamics;

namespace Physics2D.Core.Collision;

/// <summary>
/// Base class for collider shapes attached to simulation bodies.
/// </summary>
public abstract class PhysicsCollider
{
    protected PhysicsCollider(Body body)
    {
        Body = body;
    }

    public Body Body { get; }

    public abstract AABB ComputeAABB();
}
