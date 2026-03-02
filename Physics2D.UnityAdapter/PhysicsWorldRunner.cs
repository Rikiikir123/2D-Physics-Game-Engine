using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Thin adapter that hosts a core world and advances it in Unity's FixedUpdate.
/// </summary>
public sealed class PhysicsWorldRunner : MonoBehaviour
{
    public Vector2 Gravity = new(0f, -9.81f);
    public float GridCellSize = 1f;
    public int SolverIterations = 8;

    public World World { get; private set; } = default!;

    private PhysicsBodyBehaviour[] _bodies = System.Array.Empty<PhysicsBodyBehaviour>();

    private void Awake()
    {
        World = new World(Gravity.ToCore(), GridCellSize, SolverIterations);

        _bodies = FindObjectsByType<PhysicsBodyBehaviour>(FindObjectsSortMode.None);
        foreach (var body in _bodies)
        {
            body.Initialize();
            World.AddBody(body.CoreBody, body.CoreCollider);
        }
    }

    private void FixedUpdate()
    {
        foreach (var body in _bodies)
        {
            if (body.CoreBody.IsStatic)
            {
                body.SyncFromUnity();
            }
        }

        World.Step(Time.fixedDeltaTime);

        foreach (var body in _bodies)
        {
            body.SyncToUnity();
        }
    }
}
