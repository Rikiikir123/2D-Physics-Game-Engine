using Physics2D.Core.Collision;

namespace Physics2D.Core.Broadphase;

/// <summary>
/// Uniform grid broadphase. Reduces pair checks by hashing colliders into spatial buckets.
/// </summary>
public sealed class UniformGrid
{
    private readonly Dictionary<(int X, int Y), List<Collider>> _cells = new();
    private readonly HashSet<(Collider A, Collider B)> _pairSet = new();

    public UniformGrid(float cellSize)
    {
        if (cellSize <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        }

        CellSize = cellSize;
    }

    public float CellSize { get; }

    public void Clear()
    {
        _cells.Clear();
        _pairSet.Clear();
    }

    public void Insert(Collider collider)
    {
        var aabb = collider.ComputeAabb();

        var minX = WorldToCell(aabb.Min.X);
        var maxX = WorldToCell(aabb.Max.X);
        var minY = WorldToCell(aabb.Min.Y);
        var maxY = WorldToCell(aabb.Max.Y);

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var key = (x, y);
                if (!_cells.TryGetValue(key, out var list))
                {
                    list = new List<Collider>();
                    _cells[key] = list;
                }

                list.Add(collider);
            }
        }
    }

    public List<(Collider A, Collider B)> QueryPairs()
    {
        _pairSet.Clear();

        foreach (var cell in _cells.Values)
        {
            for (var i = 0; i < cell.Count; i++)
            {
                for (var j = i + 1; j < cell.Count; j++)
                {
                    var a = cell[i];
                    var b = cell[j];
                    if (ReferenceEquals(a, b))
                    {
                        continue;
                    }

                    if (a.GetHashCode() <= b.GetHashCode())
                    {
                        _pairSet.Add((a, b));
                    }
                    else
                    {
                        _pairSet.Add((b, a));
                    }
                }
            }
        }

        return _pairSet.ToList();
    }

    public List<(Collider A, Collider B)> QueryPairsNaive(IReadOnlyList<Collider> colliders)
    {
        var pairs = new List<(Collider A, Collider B)>();
        for (var i = 0; i < colliders.Count; i++)
        {
            for (var j = i + 1; j < colliders.Count; j++)
            {
                pairs.Add((colliders[i], colliders[j]));
            }
        }

        return pairs;
    }

    private int WorldToCell(float value) => (int)MathF.Floor(value / CellSize);
}
