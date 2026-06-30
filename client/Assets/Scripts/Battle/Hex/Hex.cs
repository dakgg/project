using System;
using UnityEngine;

namespace Battle.Hex
{
    /// <summary>
    /// Pointy-top hexagon cell using axial coordinates (q, r).
    /// Cube coordinate s is derived as -q-r. Pure data + math, no Unity scene dependency.
    /// Layout math follows the Red Blob Games conventions.
    /// </summary>
    public readonly struct Hex : IEquatable<Hex>
    {
        public readonly int Q;
        public readonly int R;
        public int S => -Q - R;

        public Hex(int q, int r)
        {
            Q = q;
            R = r;
        }

        // 6 neighbor directions for axial coordinates (pointy-top order).
        public static readonly Hex[] Directions =
        {
            new Hex(+1, 0), new Hex(+1, -1), new Hex(0, -1),
            new Hex(-1, 0), new Hex(-1, +1), new Hex(0, +1),
        };

        public static Hex Neighbor(Hex h, int direction)
        {
            Hex d = Directions[((direction % 6) + 6) % 6];
            return new Hex(h.Q + d.Q, h.R + d.R);
        }

        public static int Distance(Hex a, Hex b)
        {
            int dq = a.Q - b.Q;
            int dr = a.R - b.R;
            return (Mathf.Abs(dq) + Mathf.Abs(dq + dr) + Mathf.Abs(dr)) / 2;
        }

        public static Hex operator +(Hex a, Hex b) => new Hex(a.Q + b.Q, a.R + b.R);
        public static Hex operator -(Hex a, Hex b) => new Hex(a.Q - b.Q, a.R - b.R);

        // ---- Layout (pointy-top) ----

        private static readonly float Sqrt3 = Mathf.Sqrt(3f);

        /// <summary>Center of this hex in world space (XY plane) for the given size (center to corner).</summary>
        public Vector3 ToWorld(float size)
        {
            float x = size * (Sqrt3 * Q + Sqrt3 / 2f * R);
            float y = size * (1.5f * R);
            return new Vector3(x, y, 0f);
        }

        /// <summary>World position (XY plane) to the nearest hex.</summary>
        public static Hex FromWorld(Vector3 world, float size)
        {
            float q = (Sqrt3 / 3f * world.x - 1f / 3f * world.y) / size;
            float r = (2f / 3f * world.y) / size;
            return Round(q, r);
        }

        /// <summary>The 6 corner positions of this hex in world space (XY plane).</summary>
        public Vector3[] Corners(float size)
        {
            Vector3 center = ToWorld(size);
            var corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i - 30f);
                corners[i] = center + new Vector3(size * Mathf.Cos(angle), size * Mathf.Sin(angle), 0f);
            }
            return corners;
        }

        /// <summary>Round fractional axial coordinates to the nearest hex via cube rounding.</summary>
        private static Hex Round(float q, float r)
        {
            float s = -q - r;
            int rq = Mathf.RoundToInt(q);
            int rr = Mathf.RoundToInt(r);
            int rs = Mathf.RoundToInt(s);

            float dq = Mathf.Abs(rq - q);
            float dr = Mathf.Abs(rr - r);
            float ds = Mathf.Abs(rs - s);

            if (dq > dr && dq > ds)
                rq = -rr - rs;
            else if (dr > ds)
                rr = -rq - rs;

            return new Hex(rq, rr);
        }

        public bool Equals(Hex other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is Hex other && Equals(other);
        public override int GetHashCode() => unchecked(Q * 31 + R);
        public static bool operator ==(Hex a, Hex b) => a.Equals(b);
        public static bool operator !=(Hex a, Hex b) => !a.Equals(b);
        public override string ToString() => $"Hex({Q},{R})";
    }
}
