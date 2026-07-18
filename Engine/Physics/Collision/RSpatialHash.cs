using Engine.Physics;

namespace Engine.Physics.Collision
{
    // uniform grid that maps AABB bounds into cells so only nearby body pairs
    // are passed to narrow-phase instead of checking every body against every other body
    public class RSpatialHash
    {
        public float CellSize;

        private readonly Dictionary<(int, int), List<int>> cells = new();

        public RSpatialHash(float cellSize = 64f)
        {
            CellSize = cellSize;
        }

        public void Clear()
        {
            cells.Clear();
        }

        // insert body index into every cell its aabb overlaps
        public void Insert(int index, RAABB bounds)
        {
            int minX = CellCoord(bounds.Left);
            int maxX = CellCoord(bounds.Right);
            int minY = CellCoord(bounds.Top);
            int maxY = CellCoord(bounds.Bottom);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var key = (x, y);
                    if (!cells.TryGetValue(key, out List<int>? list))
                    {
                        list = new List<int>();
                        cells[key] = list;
                    }
                    list.Add(index);
                }
            }
        }

        // collect unique unordered pairs (i, j) with i < j that share at least one cell
        public void GetCandidatePairs(List<(int, int)> pairs)
        {
            pairs.Clear();
            HashSet<(int, int)> seen = new();

            foreach (var list in cells.Values)
            {
                for (int a = 0; a < list.Count; a++)
                {
                    for (int b = a + 1; b < list.Count; b++)
                    {
                        int i = list[a];
                        int j = list[b];
                        if (i == j)
                        {
                            continue;
                        }

                        if (i > j)
                        {
                            (i, j) = (j, i);
                        }

                        if (seen.Add((i, j)))
                        {
                            pairs.Add((i, j));
                        }
                    }
                }
            }
        }

        private int CellCoord(float value)
        {
            // floor division so negative coords still map into distinct cells
            return (int)System.Math.Floor(value / CellSize);
        }
    }
}
