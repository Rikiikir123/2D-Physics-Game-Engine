using System.Collections.Generic;
using Engine.Physics;
using Engine.Physics.Bodies;
using Engine.Physics.World;
using UnityEngine;

/// <summary>
/// Owns an RPhysicsWorld and steps it in FixedUpdate.
/// Add this to an empty GameObject; other bridge scripts find it in the scene.
/// </summary>
public class RPhysicsWorldBehaviour : MonoBehaviour
{
    public static RPhysicsWorldBehaviour Instance { get; private set; }

    [Tooltip("Optional: also call UpdateBounds each step using these world extents (engine Y-down units).")]
    public bool manageScreenBounds = false;
    public float worldWidth = 800f;
    public float worldHeight = 450f;

    public RPhysicsWorld World { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple RPhysicsWorldBehaviour instances; using the first.");
            return;
        }

        Instance = this;
        World = new RPhysicsWorld();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void FixedUpdate()
    {
        if (World == null)
        {
            return;
        }

        if (manageScreenBounds)
        {
            World.UpdateBounds(worldHeight, worldWidth);
        }

        World.Step(Time.fixedDeltaTime);
    }

    public void RegisterBody(RRigidBody body)
    {
        if (World == null || body == null)
        {
            return;
        }

        if (!World.Bodies.Contains(body))
        {
            World.Bodies.Add(body);
        }
    }

    public void RegisterStaticCollider(RStaticCollider collider)
    {
        if (World == null || collider == null)
        {
            return;
        }

        if (!World.StaticColliders.Contains(collider))
        {
            World.StaticColliders.Add(collider);
        }
    }
}
