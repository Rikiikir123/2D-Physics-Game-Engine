using Physics2D.Core.Collision;
using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Unity-facing body definition that creates and syncs a core body and collider.
/// </summary>
public sealed class PhysicsBodyBehaviour : MonoBehaviour
{
    [Header("Body")]
    public bool IsStatic;
    public float Mass = 1f;
    public float Restitution = 0.1f;
    public float Friction = 0.6f;

    [Header("Collider")]
    public Vector2 Size = Vector2.one;
    public Vector2 Offset = Vector2.zero;

    public Body CoreBody { get; private set; } = default!;
    public BoxCollider CoreCollider { get; private set; } = default!;

    public void Initialize()
    {
        CoreBody = new Body(transform.position.ToCore(), Mass, IsStatic)
        {
            Restitution = Restitution,
            Friction = Friction
        };
        CoreCollider = new BoxCollider(CoreBody, Size.ToCore(), Offset.ToCore());
    }

    public void SyncFromUnity()
    {
        CoreBody.Position = ((Vector2)transform.position).ToCore();
    }

    public void SyncToUnity()
    {
        transform.position = CoreBody.Position.ToUnity();
    }
}
