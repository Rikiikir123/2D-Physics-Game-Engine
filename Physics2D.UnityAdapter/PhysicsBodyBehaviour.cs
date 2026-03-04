using Physics2D.Core.Collision;
using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Unity-side holder for a core body and box collider.
/// Does not use Rigidbody2D and syncs transform with custom simulation.
/// </summary>
public sealed class PhysicsBodyBehaviour : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private bool isStatic;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float restitution = 0.05f;
    [SerializeField] private float friction = 0.8f;

    [Header("Collider")]
    [SerializeField] private Vector2 size = Vector2.one;
    [SerializeField] private Vector2 offset = Vector2.zero;

    public Body CoreBody { get; private set; } = null!;
    public PhysicsBoxCollider CoreCollider { get; private set; } = null!;

    public void Initialize()
    {
        CoreBody = new Body(transform.position.ToCore(), mass: mass, isStatic: isStatic)
        {
            Restitution = restitution,
            Friction = friction
        };

        CoreCollider = new PhysicsBoxCollider(CoreBody, size.ToCore(), offset.ToCore());
    }

    public void SyncToTransform()
    {
        var p = CoreBody.Position.ToUnity();
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }
}
