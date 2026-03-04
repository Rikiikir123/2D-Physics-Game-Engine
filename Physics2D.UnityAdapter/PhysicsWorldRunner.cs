using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Owns the core world instance and steps the simulation in Unity FixedUpdate.
/// </summary>
public sealed class PhysicsWorldRunner : MonoBehaviour
{
    [SerializeField] private Vector2 gravity = new(0f, -9.81f);
    [SerializeField] private float gridCellSize = 1.5f;
    [SerializeField] private int solverIterations = 8;

    public World World { get; private set; } = null!;

    private PhysicsBodyBehaviour[] _bodies = System.Array.Empty<PhysicsBodyBehaviour>();

    private void Awake()
    {
        World = new World(gravity.ToCore(), gridCellSize, solverIterations);

        _bodies = FindObjectsByType<PhysicsBodyBehaviour>(FindObjectsSortMode.None);
        foreach (var behaviour in _bodies)
        {
            behaviour.Initialize();
            World.AddBody(behaviour.CoreBody, behaviour.CoreCollider);
        }
    }

    private void FixedUpdate()
    {
        World.Step(Time.fixedDeltaTime);

        foreach (var behaviour in _bodies)
        {
            behaviour.SyncToTransform();
        }
    }
}
