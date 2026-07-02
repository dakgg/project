using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Hex
{
    /// <summary>
    /// BattleScene entry point for the hex A* demo. Builds a hexagon grid, draws it,
    /// spawns a unit token, and drives interaction:
    ///   - Left click  : set destination → A* path → highlight → move the token.
    ///   - Right click : toggle an obstacle on a cell (spawner/unit/enemy cells excluded).
    /// The map starts with no obstacles. Six spawners sit on the map's corner cells and
    /// periodically spawn enemies that chase the player unit with per-step A* repathing.
    /// Attach this to an empty GameObject in the scene; everything else is created at runtime.
    /// </summary>
    public class BattlePathfindingDemo : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int mapRadius = 5;
        [SerializeField] private float hexSize = 0.5f;
        [SerializeField] private float topBarHeightPx = 100f; // screen pixels reserved for the top bar UI

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f; // hexes per second

        [Header("Enemies")]
        [SerializeField] private float enemySpawnInterval = 3f;
        [SerializeField] private int maxEnemies = 12;
        [SerializeField] private float enemyMoveSpeed = 2.5f; // hexes per second

        [Header("Colors")]
        [SerializeField] private Color defaultColor = new Color(0.22f, 0.24f, 0.30f);
        [SerializeField] private Color blockedColor = new Color(0.10f, 0.10f, 0.12f);
        [SerializeField] private Color borderColor = new Color(0.45f, 0.48f, 0.55f);
        [SerializeField] private Color pathColor = new Color(0.25f, 0.55f, 0.95f);
        [SerializeField] private Color startColor = new Color(0.30f, 0.85f, 0.40f);
        [SerializeField] private Color goalColor = new Color(0.95f, 0.75f, 0.20f);
        [SerializeField] private Color unitColor = new Color(0.95f, 0.30f, 0.35f);

        private HexGrid _grid;
        private HexGridRenderer _renderer;
        private readonly List<HexSpawner> _spawners = new List<HexSpawner>();
        private readonly List<HexEnemy> _enemies = new List<HexEnemy>();
        private readonly HashSet<Hex> _enemyOccupied = new HashSet<Hex>();
        private Transform _unit;
        private Hex _unitHex;
        private List<Hex> _currentPath = new List<Hex>();
        private Coroutine _moveRoutine;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            SetupCamera();

            _grid = new HexGrid(mapRadius);

            _renderer = GetComponent<HexGridRenderer>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<HexGridRenderer>();
            _renderer.Build(_grid, hexSize, defaultColor, blockedColor, borderColor);

            CreateSpawners();

            _unitHex = FindSpawnCell();
            CreateUnit();
            RedrawHighlights();

            StartCoroutine(SpawnEnemyLoop());
        }

        private void SetupCamera()
        {
            if (_camera == null)
                return;
            _camera.orthographic = true;

            // Grid extents (pointy-top hexagon map): widest at the q = ±mapRadius corner
            // cells (± half a hex beyond their centers), tallest at the r = ±mapRadius
            // corner cells (± one hex tip beyond their centers).
            float halfWidth = hexSize * Mathf.Sqrt(3f) * (mapRadius + 0.5f);
            float halfHeight = hexSize * (1.5f * mapRadius + 1f);

            // Fit both axes for the current aspect (portrait 1080x1920 included); the
            // vertical fit only gets the screen below the top bar.
            const float margin = 1.05f;
            float screenH = _camera.pixelHeight;
            float usableH = Mathf.Max(1f, screenH - topBarHeightPx);
            float sizeForHeight = halfHeight * screenH / usableH;
            float size = Mathf.Max(sizeForHeight, halfWidth / _camera.aspect) * margin;
            _camera.orthographicSize = size;

            // Anchor the grid's top edge right below the bar (not centered in the leftover
            // space — with a width-bound fit the vertical slack is large and centering
            // would make the bar reservation invisible).
            float topBarWorld = topBarHeightPx * (2f * size / screenH);
            float camY = halfHeight - size + topBarWorld;
            _camera.transform.SetPositionAndRotation(new Vector3(0f, camY, -10f), Quaternion.identity);
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
        }

        private void CreateSpawners()
        {
            // One spawner on each of the 6 corner cells of the hexagon map.
            for (int i = 0; i < 6; i++)
            {
                Hex dir = Hex.Directions[i];
                Hex corner = new Hex(dir.Q * mapRadius, dir.R * mapRadius);

                var go = new GameObject($"Spawner_{i}");
                go.transform.SetParent(transform, false);
                var spawner = go.AddComponent<HexSpawner>();
                spawner.Initialize(corner, hexSize, Color.HSVToRGB(i / 6f, 0.75f, 0.95f));
                _spawners.Add(spawner);
            }
        }

        private IEnumerator SpawnEnemyLoop()
        {
            var wait = new WaitForSeconds(enemySpawnInterval);
            int index = 0;
            while (true)
            {
                yield return wait;
                _enemies.RemoveAll(e => e == null);
                if (_enemies.Count >= maxEnemies)
                    continue;

                // Round-robin over spawners; skip cells that are taken right now.
                for (int tries = 0; tries < _spawners.Count; tries++)
                {
                    HexSpawner spawner = _spawners[index];
                    index = (index + 1) % _spawners.Count;
                    if (spawner.Cell == _unitHex || _enemyOccupied.Contains(spawner.Cell) || !_grid.IsWalkable(spawner.Cell))
                        continue;
                    _enemies.Add(spawner.SpawnEnemy(_grid, enemyMoveSpeed, () => _unitHex, _enemyOccupied));
                    break;
                }
            }
        }

        private bool IsSpawnerCell(Hex h)
        {
            foreach (HexSpawner s in _spawners)
                if (s.Cell == h)
                    return true;
            return false;
        }

        private Hex FindSpawnCell()
        {
            if (_grid.IsWalkable(new Hex(0, 0)))
                return new Hex(0, 0);
            foreach (Hex h in _grid.AllCells)
                if (_grid.IsWalkable(h))
                    return h;
            return new Hex(0, 0);
        }

        private void CreateUnit()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Unit";
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(transform, false);
            quad.transform.localScale = Vector3.one * hexSize * 1.1f;

            var mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = unitColor };

            _unit = quad.transform;
            _unit.position = WorldOf(_unitHex);
        }

        private Vector3 WorldOf(Hex h)
        {
            Vector3 w = h.ToWorld(hexSize);
            w.z = -0.05f; // in front of grid + border
            return w;
        }

        private void Update()
        {
            if (_camera == null)
                return;

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
                HandleLeftClick();
            else if (mouse.rightButton.wasPressedThisFrame)
                HandleRightClick();
        }

        private bool TryGetMouseHex(out Hex hex)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 screen = new Vector3(mousePos.x, mousePos.y, -_camera.transform.position.z); // distance to z=0 plane
            Vector3 world = _camera.ScreenToWorldPoint(screen);
            hex = Hex.FromWorld(world, hexSize);
            return _grid.Contains(hex);
        }

        private void HandleLeftClick()
        {
            if (!TryGetMouseHex(out Hex goal) || !_grid.IsWalkable(goal))
                return;
            if (_moveRoutine != null)
                return; // ignore clicks while moving

            List<Hex> path = HexPathfinder.FindPath(_grid, _unitHex, goal);
            if (path.Count == 0)
            {
                Debug.Log($"[HexDemo] No path from {_unitHex} to {goal}");
                return;
            }
            _currentPath = path;
            RedrawHighlights();
            _moveRoutine = StartCoroutine(MoveAlong(path));
        }

        private void HandleRightClick()
        {
            if (!TryGetMouseHex(out Hex hex))
                return;
            if (hex == _unitHex)
                return; // do not block the unit's own cell
            if (IsSpawnerCell(hex))
                return; // keep spawner cells walkable
            if (_enemyOccupied.Contains(hex))
                return; // do not block a cell an enemy is on / moving into

            _grid.ToggleBlocked(hex);
            _currentPath.Clear();
            _renderer.ResetColors(_grid);
            RedrawHighlights();
        }

        private IEnumerator MoveAlong(List<Hex> path)
        {
            // path[0] is the current cell; step through the rest.
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 from = WorldOf(path[i - 1]);
                Vector3 to = WorldOf(path[i]);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * moveSpeed;
                    _unit.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                    yield return null;
                }
                _unitHex = path[i];
            }

            _moveRoutine = null;
            _currentPath.Clear();
            RedrawHighlights();
        }

        private void RedrawHighlights()
        {
            _renderer.ResetColors(_grid);
            foreach (Hex h in _currentPath)
                _renderer.SetCellColor(h, pathColor);
            if (_currentPath.Count > 0)
            {
                _renderer.SetCellColor(_currentPath[0], startColor);
                _renderer.SetCellColor(_currentPath[_currentPath.Count - 1], goalColor);
            }
            _renderer.SetCellColor(_unitHex, startColor);
        }
    }
}
