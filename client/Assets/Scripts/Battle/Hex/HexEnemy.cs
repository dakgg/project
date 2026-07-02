using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Hex
{
    /// <summary>
    /// An enemy token that chases a target cell over the hex grid. Recomputes an
    /// A* path every step (cheap on maps this small, and it naturally tracks a
    /// moving unit), reserves its next cell in a shared occupancy set so enemies
    /// do not stack, and idles when adjacent to the target or when no path exists.
    /// </summary>
    public class HexEnemy : MonoBehaviour
    {
        private const float RetryDelay = 0.2f;

        private HexGrid _grid;
        private float _hexSize;
        private float _moveSpeed;
        private Func<Hex> _getTargetHex;
        private HashSet<Hex> _occupied;

        public Hex Cell { get; private set; }

        public void Initialize(HexGrid grid, Hex startCell, float hexSize, float moveSpeed,
            Func<Hex> getTargetHex, HashSet<Hex> occupied)
        {
            _grid = grid;
            _hexSize = hexSize;
            _moveSpeed = moveSpeed;
            _getTargetHex = getTargetHex;
            _occupied = occupied;

            Cell = startCell;
            _occupied.Add(startCell);
            transform.position = WorldOf(startCell);
            StartCoroutine(ChaseLoop());
        }

        private void OnDestroy()
        {
            if (_occupied != null)
                _occupied.Remove(Cell);
        }

        private Vector3 WorldOf(Hex h)
        {
            Vector3 w = h.ToWorld(_hexSize);
            w.z = -0.05f; // in front of grid + border
            return w;
        }

        private IEnumerator ChaseLoop()
        {
            var retry = new WaitForSeconds(RetryDelay);
            while (true)
            {
                Hex target = _getTargetHex();
                if (Hex.Distance(Cell, target) <= 1)
                {
                    yield return retry; // reached the unit — wait next to it
                    continue;
                }

                List<Hex> path = HexPathfinder.FindPath(_grid, Cell, target);
                if (path.Count < 2 || _occupied.Contains(path[1]))
                {
                    yield return retry; // no path, or next cell taken — try again shortly
                    continue;
                }

                Hex next = path[1];
                _occupied.Remove(Cell);
                _occupied.Add(next); // reserve before moving so others route around

                Vector3 from = WorldOf(Cell);
                Vector3 to = WorldOf(next);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * _moveSpeed;
                    transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                    yield return null;
                }
                Cell = next;
            }
        }
    }
}
