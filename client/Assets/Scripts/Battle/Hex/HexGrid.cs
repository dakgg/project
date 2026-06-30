using System.Collections.Generic;

namespace Battle.Hex
{
    /// <summary>
    /// A hexagon-shaped map of cells with optional blocked (obstacle) cells.
    /// Pure C# model — no Unity scene dependency.
    /// </summary>
    public class HexGrid
    {
        private readonly HashSet<Hex> _cells = new HashSet<Hex>();
        private readonly HashSet<Hex> _blocked = new HashSet<Hex>();

        public int Radius { get; }

        /// <summary>Builds a hexagon-shaped grid of the given radius (number of rings around center).</summary>
        public HexGrid(int radius)
        {
            Radius = radius;
            for (int q = -radius; q <= radius; q++)
            {
                int rMin = System.Math.Max(-radius, -q - radius);
                int rMax = System.Math.Min(radius, -q + radius);
                for (int r = rMin; r <= rMax; r++)
                    _cells.Add(new Hex(q, r));
            }
        }

        public IEnumerable<Hex> AllCells => _cells;

        public bool Contains(Hex h) => _cells.Contains(h);

        public bool IsBlocked(Hex h) => _blocked.Contains(h);

        public bool IsWalkable(Hex h) => _cells.Contains(h) && !_blocked.Contains(h);

        /// <summary>Toggles the obstacle state of a valid cell. Returns the new blocked state.</summary>
        public bool ToggleBlocked(Hex h)
        {
            if (!_cells.Contains(h))
                return false;
            if (_blocked.Contains(h))
            {
                _blocked.Remove(h);
                return false;
            }
            _blocked.Add(h);
            return true;
        }

        public void SetBlocked(Hex h, bool blocked)
        {
            if (!_cells.Contains(h))
                return;
            if (blocked) _blocked.Add(h);
            else _blocked.Remove(h);
        }

        /// <summary>Enumerates the walkable neighbors of a cell (in-bounds and not blocked).</summary>
        public IEnumerable<Hex> WalkableNeighbors(Hex h)
        {
            for (int i = 0; i < 6; i++)
            {
                Hex n = Hex.Neighbor(h, i);
                if (IsWalkable(n))
                    yield return n;
            }
        }
    }
}
