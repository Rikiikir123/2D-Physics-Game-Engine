using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;

namespace Physics2D.Core.Collision;

/// <summary>
/// Stores collision manifold data for an AABB-to-AABB contact.
/// </summary>
public sealed class Contact
{
    public required Body A { get; init; }
    public required Body B { get; init; }
    public required Vec2 Normal { get; init; }
    public required float PenetrationDepth { get; init; }
}
