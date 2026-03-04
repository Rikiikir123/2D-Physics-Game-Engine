using Physics2D.Core.Broadphase;
using Physics2D.Core.Collision;
using Physics2D.Core.Math;

namespace Physics2D.Core.Dynamics;

/// <summary>
/// Central simulation world orchestrating integration, broadphase, contact generation, and solving.
/// </summary>
public sealed class World
{
    public List<Body> Bodies { get; } = new();
    public List<PhysicsCollider> Colliders { get; } = new();
    public UniformGrid Grid { get; }
    public Solver Solver { get; }
    public Vec2 Gravity { get; set; }
    public IReadOnlyList<Contact> LastContacts => _lastContacts;

    private readonly List<Contact> _lastContacts = new();

    public World(Vec2 gravity, float gridCellSize = 2f, int solverIterations = 8)
    {
        Gravity = gravity;
        Grid = new UniformGrid(gridCellSize);
        Solver = new Solver { Iterations = solverIterations };
    }

    public void AddBody(Body body, PhysicsCollider collider)
    {
        Bodies.Add(body);
        Colliders.Add(collider);
    }

    public void Step(float dt)
    {
        #region Integrate External Forces
        foreach (var body in Bodies)
        {
            body.IntegrateForces(dt, Gravity);
        }
        #endregion

        #region Broadphase Build
        Grid.Clear();
        foreach (var collider in Colliders)
        {
            Grid.Insert(collider);
        }
        #endregion

        #region Narrowphase Contact Generation
        _lastContacts.Clear();
        foreach (var (a, b) in Grid.QueryPairs())
        {
            if (TryCreateContact(a, b, out var contact))
            {
                _lastContacts.Add(contact);
            }
        }
        #endregion

        #region Iterative Solve
        Solver.Solve(_lastContacts);
        #endregion

        #region Integrate Velocities + Clear Forces
        foreach (var body in Bodies)
        {
            body.IntegrateVelocity(dt);
            body.ClearForces();
        }
        #endregion
    }

    /// <summary>
    /// Performs AABB-vs-AABB narrowphase and computes the collision normal by minimum overlap axis.
    /// </summary>
    public static bool TryCreateContact(PhysicsCollider c1, PhysicsCollider c2, out Contact contact)
    {
        var aabb1 = c1.ComputeAABB();
        var aabb2 = c2.ComputeAABB();

        contact = null!;
        if (!aabb1.Overlaps(aabb2))
        {
            return false;
        }

        var overlap = aabb1.GetOverlap(aabb2);
        if (overlap.X <= 0f || overlap.Y <= 0f)
        {
            return false;
        }

        Vec2 normal;
        float penetration;

        var delta = c2.Body.Position - c1.Body.Position;
        if (overlap.X < overlap.Y)
        {
            penetration = overlap.X;
            normal = delta.X >= 0f ? new Vec2(1f, 0f) : new Vec2(-1f, 0f);
        }
        else
        {
            penetration = overlap.Y;
            normal = delta.Y >= 0f ? new Vec2(0f, 1f) : new Vec2(0f, -1f);
        }

        contact = new Contact
        {
            A = c1.Body,
            B = c2.Body,
            Normal = normal,
            PenetrationDepth = penetration
        };

        return true;
    }
}
