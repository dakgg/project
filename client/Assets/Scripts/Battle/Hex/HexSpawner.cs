using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Hex
{
    /// <summary>
    /// A spawn point pinned to one hex cell. Draws a diamond marker on the cell
    /// and creates chasing enemies there via <see cref="SpawnEnemy"/>.
    /// Created at runtime by <see cref="BattlePathfindingDemo"/> — one per map corner.
    /// </summary>
    public class HexSpawner : MonoBehaviour
    {
        public Hex Cell { get; private set; }
        public Color Color { get; private set; }

        private float _hexSize;

        public void Initialize(Hex cell, float hexSize, Color color)
        {
            Cell = cell;
            _hexSize = hexSize;
            Color = color;

            Vector3 pos = cell.ToWorld(hexSize);
            pos.z = -0.02f; // in front of grid + border, behind units
            transform.position = pos;

            CreateMarker();
        }

        private void CreateMarker()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Marker";
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(transform, false);
            quad.transform.localScale = Vector3.one * _hexSize * 0.9f;
            quad.transform.localRotation = Quaternion.Euler(0f, 0f, 45f); // diamond shape

            var mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = Color };
        }

        /// <summary>Spawns a chasing enemy on this spawner's cell, tinted with the spawner color.</summary>
        public HexEnemy SpawnEnemy(HexGrid grid, float moveSpeed, Func<Hex> getTargetHex, HashSet<Hex> occupied)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = $"Enemy_{name}";
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(transform.parent, false);
            quad.transform.localScale = Vector3.one * _hexSize * 0.9f;
            quad.transform.localRotation = Quaternion.Euler(0f, 0f, 45f); // diamond, matches the marker

            var mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = Color };

            var enemy = quad.AddComponent<HexEnemy>();
            enemy.Initialize(grid, Cell, _hexSize, moveSpeed, getTargetHex, occupied);
            return enemy;
        }
    }
}
