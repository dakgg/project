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
    ///   - Right click : toggle an obstacle on a cell (recomputes the active path).
    /// Attach this to an empty GameObject in the scene; everything else is created at runtime.
    /// </summary>
    public class BattlePathfindingDemo : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int mapRadius = 5;
        [SerializeField] private float hexSize = 0.5f;
        [SerializeField, Range(0f, 0.4f)] private float obstacleRatio = 0.15f;
        [SerializeField] private int randomSeed = 12345;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f; // hexes per second

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
            GenerateObstacles();

            _renderer = GetComponent<HexGridRenderer>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<HexGridRenderer>();
            _renderer.Build(_grid, hexSize, defaultColor, blockedColor, borderColor);

            _unitHex = FindSpawnCell();
            CreateUnit();
            RedrawHighlights();
        }

        private void SetupCamera()
        {
            if (_camera == null)
                return;
            _camera.orthographic = true;
            // Fit the whole hexagon map (a little margin).
            _camera.orthographicSize = hexSize * (mapRadius + 1.5f) * 1.5f;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _camera.transform.rotation = Quaternion.identity;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
        }

        private void GenerateObstacles()
        {
            var rng = new System.Random(randomSeed);
            foreach (Hex h in new List<Hex>(_grid.AllCells))
            {
                if (h == new Hex(0, 0))
                    continue; // keep center clear for the spawn
                if (rng.NextDouble() < obstacleRatio)
                    _grid.SetBlocked(h, true);
            }
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
