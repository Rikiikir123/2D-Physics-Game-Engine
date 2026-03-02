using Physics2D.Core.Broadphase;
using Physics2D.Core.Collision;
using Physics2D.Core.Math;

namespace Physics2D.Core.Dynamics;

/// <summary>
/// Owns simulation objects and advances the world by fixed dt steps.
/// </summary>
public sealed class World
{
    private readonly List<Contact> _contacts = new();

    public World(Vec2 gravity, float gridCellSize = 1f, int solverIterations = 8)
    {
        Gravity = gravity;
        Grid = new UniformGrid(gridCellSize);
        Solver = new Solver(solverIterations);
    }

    public List<Body> Bodies { get; } = new();
    public List<Collider> Colliders { get; } = new();
    public UniformGrid Grid { get; }
    public Solver Solver { get; }
    public Vec2 Gravity { get; set; }

    public IReadOnlyList<Contact> LastContacts => _contacts;

    public void AddBody(Body body, Collider collider)
    {
        Bodies.Add(body);
        Colliders.Add(collider);
    }

    public void Step(float dt)
    {
        foreach (var body in Bodies)
        {
            body.IntegrateForces(dt, Gravity);
        }

        Grid.Clear();
        foreach (var collider in Colliders)
        {
            Grid.Insert(collider);
        }

        _contacts.Clear();
        var pairs = Grid.QueryPairs();
        foreach (var (a, b) in pairs)
        {
            if (TryGenerateContact(a, b, out var contact))
            {
                _contacts.Add(contact);
            }
        }

        Solver.SolveContacts(_contacts);

        foreach (var body in Bodies)
        {
            body.IntegrateVelocity(dt);
            body.ClearForces();
        }
    }

    public static bool TryGenerateContact(Collider a, Collider b, out Contact contact)
    {
        var aAabb = a.ComputeAabb();
        var bAabb = b.ComputeAabb();

        if (!aAabb.Overlaps(bAabb))
        {
            contact = default!;
            return false;
        }

        var overlap = aAabb.GetOverlap(bAabb);
        if (overlap.X <= 0f || overlap.Y <= 0f)
        {
            contact = default!;
            return false;
        }

        Vec2 normal;
        float depth;

        if (overlap.X < overlap.Y)
        {
            depth = overlap.X;
            normal = a.Body.Position.X < b.Body.Position.X ? new Vec2(-1f, 0f) : new Vec2(1f, 0f);
        }
        else
        {
            depth = overlap.Y;
            normal = a.Body.Position.Y < b.Body.Position.Y ? new Vec2(0f, -1f) : new Vec2(0f, 1f);
        }

        contact = new Contact
        {
            A = a.Body,
            B = b.Body,
            Normal = normal,
            PenetrationDepth = depth
        };

        return true;
    }
}
