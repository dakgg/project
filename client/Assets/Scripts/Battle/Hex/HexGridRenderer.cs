using System.Collections.Generic;
using UnityEngine;

namespace Battle.Hex
{
    /// <summary>
    /// Renders a <see cref="HexGrid"/> at runtime with procedurally generated meshes —
    /// no sprite/tile assets required. Builds one fill mesh (triangles, vertex colors)
    /// and one border mesh (line topology). Cell state is changed by recoloring the
    /// fill mesh in place via <see cref="SetCellColor"/>.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexGridRenderer : MonoBehaviour
    {
        // 7 vertices per hex: center + 6 corners (triangle fan).
        private const int VertsPerHex = 7;

        private Mesh _fillMesh;
        private Color[] _colors;
        private readonly Dictionary<Hex, int> _cellVertexStart = new Dictionary<Hex, int>();

        private float _size;
        private Color _defaultColor = new Color(0.22f, 0.24f, 0.30f);
        private Color _blockedColor = new Color(0.10f, 0.10f, 0.12f);

        public Color DefaultColor => _defaultColor;
        public Color BlockedColor => _blockedColor;

        /// <summary>(Re)builds the meshes for the given grid. Cells start with default/blocked colors.</summary>
        public void Build(HexGrid grid, float size, Color defaultColor, Color blockedColor, Color borderColor)
        {
            _size = size;
            _defaultColor = defaultColor;
            _blockedColor = blockedColor;
            _cellVertexStart.Clear();

            var cells = new List<Hex>(grid.AllCells);
            int n = cells.Count;

            var vertices = new Vector3[n * VertsPerHex];
            _colors = new Color[n * VertsPerHex];
            var triangles = new int[n * 6 * 3];
            var lineVertices = new Vector3[n * 6];
            var lineIndices = new int[n * 12];

            int vi = 0, ti = 0, lvi = 0, lii = 0;
            foreach (Hex h in cells)
            {
                int start = vi;
                _cellVertexStart[h] = start;
                Color c = grid.IsBlocked(h) ? blockedColor : defaultColor;

                Vector3 center = h.ToWorld(size);
                Vector3[] corners = h.Corners(size);

                // center vertex
                vertices[vi] = center;
                _colors[vi] = c;
                vi++;
                // corner vertices
                for (int k = 0; k < 6; k++)
                {
                    vertices[vi] = corners[k];
                    _colors[vi] = c;
                    vi++;
                }
                // fan triangles (center, corner k, corner k+1)
                for (int k = 0; k < 6; k++)
                {
                    triangles[ti++] = start;
                    triangles[ti++] = start + 1 + k;
                    triangles[ti++] = start + 1 + (k + 1) % 6;
                }

                // border lines
                int lineStart = lvi;
                for (int k = 0; k < 6; k++)
                {
                    lineVertices[lvi++] = corners[k];
                }
                for (int k = 0; k < 6; k++)
                {
                    lineIndices[lii++] = lineStart + k;
                    lineIndices[lii++] = lineStart + (k + 1) % 6;
                }
            }

            // Fill mesh on this GameObject.
            _fillMesh = new Mesh { name = "HexFill" };
            _fillMesh.indexFormat = vertices.Length > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            _fillMesh.vertices = vertices;
            _fillMesh.colors = _colors;
            _fillMesh.triangles = triangles;
            _fillMesh.RecalculateBounds();

            var mf = GetComponent<MeshFilter>();
            mf.sharedMesh = _fillMesh;
            var mr = GetComponent<MeshRenderer>();
            mr.sharedMaterial = CreateVertexColorMaterial();

            BuildBorder(lineVertices, lineIndices, borderColor);
        }

        private void BuildBorder(Vector3[] lineVertices, int[] lineIndices, Color borderColor)
        {
            Transform existing = transform.Find("Border");
            GameObject borderGo = existing != null ? existing.gameObject : new GameObject("Border");
            borderGo.transform.SetParent(transform, false);

            var lineMesh = new Mesh { name = "HexBorder" };
            lineMesh.indexFormat = lineVertices.Length > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            lineMesh.vertices = lineVertices;
            var lineColors = new Color[lineVertices.Length];
            for (int i = 0; i < lineColors.Length; i++) lineColors[i] = borderColor;
            lineMesh.colors = lineColors;
            lineMesh.SetIndices(lineIndices, MeshTopology.Lines, 0);
            lineMesh.RecalculateBounds();

            var mf = borderGo.GetComponent<MeshFilter>();
            if (mf == null) mf = borderGo.AddComponent<MeshFilter>();
            mf.sharedMesh = lineMesh;
            var mr = borderGo.GetComponent<MeshRenderer>();
            if (mr == null) mr = borderGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = CreateVertexColorMaterial();
            // Draw borders slightly in front of fills.
            borderGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        }

        private static Material CreateVertexColorMaterial()
        {
            // Sprites/Default is part of the built-in pipeline and honors vertex colors.
            Shader shader = Shader.Find("Sprites/Default");
            return new Material(shader);
        }

        /// <summary>Recolors a single cell in place (no mesh rebuild).</summary>
        public void SetCellColor(Hex h, Color color)
        {
            if (_fillMesh == null || !_cellVertexStart.TryGetValue(h, out int start))
                return;
            for (int k = 0; k < VertsPerHex; k++)
                _colors[start + k] = color;
            _fillMesh.colors = _colors;
        }

        /// <summary>Resets every cell to its default color, honoring blocked cells.</summary>
        public void ResetColors(HexGrid grid)
        {
            if (_fillMesh == null)
                return;
            foreach (var kv in _cellVertexStart)
            {
                Color c = grid.IsBlocked(kv.Key) ? _blockedColor : _defaultColor;
                for (int k = 0; k < VertsPerHex; k++)
                    _colors[kv.Value + k] = c;
            }
            _fillMesh.colors = _colors;
        }
    }
}
