using Physics2D.Core.Dynamics;

namespace Physics2D.Core.Collision;

/// <summary>
/// Base collider type. Current implementation supports AABB based colliders only.
/// </summary>
public abstract class Collider
{
    protected Collider(Body body)
    {
        Body = body;
    }

    /// <summary>
    /// Owning body of this collider.
    /// </summary>
    public Body Body { get; }

    /// <summary>
    /// Computes world-space AABB for this collider.
    /// </summary>
    public abstract AABB ComputeAabb();
}
