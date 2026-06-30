using System.Collections.Generic;

namespace Battle.Hex
{
    /// <summary>
    /// A* pathfinding over a <see cref="HexGrid"/>. Uniform step cost (1), so the
    /// returned path has the minimum number of hexes. Heuristic is the hex distance,
    /// which is admissible and consistent — A* yields an optimal path.
    /// </summary>
    public static class HexPathfinder
    {
        /// <summary>
        /// Finds a path from start to goal inclusive. Returns an empty list if no path
        /// exists or if either endpoint is not walkable.
        /// </summary>
        public static List<Hex> FindPath(HexGrid grid, Hex start, Hex goal)
        {
            var path = new List<Hex>();
            if (grid == null || !grid.IsWalkable(start) || !grid.IsWalkable(goal))
                return path;

            if (start == goal)
            {
                path.Add(start);
                return path;
            }

            var open = new MinHeap();
            var cameFrom = new Dictionary<Hex, Hex>();
            var gScore = new Dictionary<Hex, int> { [start] = 0 };
            var closed = new HashSet<Hex>();

            open.Push(start, Hex.Distance(start, goal));

            while (open.Count > 0)
            {
                Hex current = open.Pop();
                if (current == goal)
                    return Reconstruct(cameFrom, current);

                if (!closed.Add(current))
                    continue;

                int currentG = gScore[current];
                foreach (Hex next in grid.WalkableNeighbors(current))
                {
                    if (closed.Contains(next))
                        continue;

                    int tentativeG = currentG + 1;
                    if (gScore.TryGetValue(next, out int knownG) && tentativeG >= knownG)
                        continue;

                    cameFrom[next] = current;
                    gScore[next] = tentativeG;
                    int f = tentativeG + Hex.Distance(next, goal);
                    open.Push(next, f);
                }
            }

            return path; // empty — no path
        }

        private static List<Hex> Reconstruct(Dictionary<Hex, Hex> cameFrom, Hex current)
        {
            var path = new List<Hex> { current };
            while (cameFrom.TryGetValue(current, out Hex prev))
            {
                current = prev;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Minimal binary min-heap keyed by priority (f-score). Unity 6000.1 targets
        /// .NET Standard 2.1, which lacks System.Collections.Generic.PriorityQueue.
        /// </summary>
        private class MinHeap
        {
            private readonly List<Hex> _items = new List<Hex>();
            private readonly List<int> _priorities = new List<int>();

            public int Count => _items.Count;

            public void Push(Hex item, int priority)
            {
                _items.Add(item);
                _priorities.Add(priority);
                int i = _items.Count - 1;
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (_priorities[parent] <= _priorities[i])
                        break;
                    Swap(i, parent);
                    i = parent;
                }
            }

            public Hex Pop()
            {
                Hex root = _items[0];
                int last = _items.Count - 1;
                _items[0] = _items[last];
                _priorities[0] = _priorities[last];
                _items.RemoveAt(last);
                _priorities.RemoveAt(last);

                int i = 0;
                int count = _items.Count;
                while (true)
                {
                    int left = 2 * i + 1;
                    int right = 2 * i + 2;
                    int smallest = i;
                    if (left < count && _priorities[left] < _priorities[smallest]) smallest = left;
                    if (right < count && _priorities[right] < _priorities[smallest]) smallest = right;
                    if (smallest == i)
                        break;
                    Swap(i, smallest);
                    i = smallest;
                }
                return root;
            }

            private void Swap(int a, int b)
            {
                (_items[a], _items[b]) = (_items[b], _items[a]);
                (_priorities[a], _priorities[b]) = (_priorities[b], _priorities[a]);
            }
        }
    }
}
