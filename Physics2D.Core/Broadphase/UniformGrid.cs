using Physics2D.Core.Collision;

namespace Physics2D.Core.Broadphase;

/// <summary>
/// Uniform spatial hash grid for broadphase culling.
/// </summary>
public sealed class UniformGrid
{
    private readonly Dictionary<(int X, int Y), List<PhysicsCollider>> _cells = new();
    private readonly HashSet<(PhysicsCollider A, PhysicsCollider B)> _pairs = new(new ColliderPairComparer());

    public UniformGrid(float cellSize)
    {
        CellSize = cellSize;
    }

    public float CellSize { get; }

    public void Clear()
    {
        _cells.Clear();
        _pairs.Clear();
    }

    public void Insert(PhysicsCollider collider)
    {
        var aabb = collider.ComputeAABB();
        var minX = ToCell(aabb.Min.X);
        var minY = ToCell(aabb.Min.Y);
        var maxX = ToCell(aabb.Max.X);
        var maxY = ToCell(aabb.Max.Y);

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var key = (x, y);
                if (!_cells.TryGetValue(key, out var list))
                {
                    list = new List<PhysicsCollider>();
                    _cells[key] = list;
                }

                list.Add(collider);
            }
        }
    }

    public IEnumerable<(PhysicsCollider A, PhysicsCollider B)> QueryPairs()
    {
        _pairs.Clear();

        foreach (var cell in _cells.Values)
        {
            for (var i = 0; i < cell.Count; i++)
            {
                for (var j = i + 1; j < cell.Count; j++)
                {
                    if (ReferenceEquals(cell[i].Body, cell[j].Body))
                    {
                        continue;
                    }

                    _pairs.Add((cell[i], cell[j]));
                }
            }
        }

        return _pairs;
    }

    private int ToCell(float coordinate) => (int)System.MathF.Floor(coordinate / CellSize);

    private sealed class ColliderPairComparer : IEqualityComparer<(PhysicsCollider A, PhysicsCollider B)>
    {
        public bool Equals((PhysicsCollider A, PhysicsCollider B) x, (PhysicsCollider A, PhysicsCollider B) y)
        {
            return (ReferenceEquals(x.A, y.A) && ReferenceEquals(x.B, y.B)) ||
                   (ReferenceEquals(x.A, y.B) && ReferenceEquals(x.B, y.A));
        }

        public int GetHashCode((PhysicsCollider A, PhysicsCollider B) obj)
        {
            var h1 = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.A);
            var h2 = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.B);
            return h1 < h2 ? HashCode.Combine(h1, h2) : HashCode.Combine(h2, h1);
        }
    }
}
