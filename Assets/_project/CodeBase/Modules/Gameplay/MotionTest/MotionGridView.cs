using System.Collections.Generic;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBase.MotionTest
{
    public sealed class MotionGridView : MonoBehaviour
    {
        private MotionTestController _host;
        private Mesh _mesh;
        private Mesh _gridMesh;
        private GridPosition _gridCell = new GridPosition(int.MinValue, int.MinValue);
        private long _gridRevision = -1;
        private bool _gridCombat, _gridAll;
        private GridPosition _lastCell = new GridPosition(int.MinValue, int.MinValue);
        private bool _lastCombat, _lastAll;
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<int> _indices = new List<int>();
        private GameObject _root;
        public void Bind(MotionTestController host)
        {
            _host = host; _root = new GameObject("Motion grid and preview"); _root.transform.SetParent(transform, false);
            _mesh = CreateMesh("Path preview"); _gridMesh = CreateMesh("Cell outlines");
        }
        private Mesh CreateMesh(string name)
        {
            var node = new GameObject(name); node.transform.SetParent(_root.transform, false);
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            node.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = node.AddComponent<MeshRenderer>(); renderer.sharedMaterial = _host.World.Config.GridMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return mesh;
        }
        private void LateUpdate()
        {
            if (_host == null || !_host.Ready) return;
            _host.Map.Cell(_host.Hero.transform.position, out var cell);
            if (cell == _lastCell && _lastCombat == _host.Combat && _lastAll == _host.ShowAllCells) return;
            _lastCell = cell; _lastCombat = _host.Combat; _lastAll = _host.ShowAllCells; Refresh();
        }
        public void Refresh()
        {
            if (_mesh == null) return;
            RefreshGrid();
            _mesh.Clear(); _vertices.Clear(); _colors.Clear(); _indices.Clear();
            if (!_host.Combat) return;
            var map = _host.Map; var config = _host.World.Config; var size = config.Dimension;
            map.Cell(_host.Hero.transform.position, out var center);
            var path = _host.Preview;
            if (path.Found)
            {
                long cost = 0;
                for (var i = 1; i < path.Cells.Count; ++i)
                {
                    cost += map.Board.GetCell(path.Cells[i]).MoveCost;
                    var color = _host.Busy ? new Color(.3f, .85f, 1, 1) : cost <= _host.Session.Actions ? new Color(.25f, 1, .65f, 1) : new Color(1, .25f, .25f, 1);
                    Strip(map.World(path.Cells[i - 1]) + Vector3.up * .16f, map.World(path.Cells[i]) + Vector3.up * .16f, .09f, color);
                }
            }
            if (_host.Hover.HasValue && map.Board.Contains(_host.Hover.Value))
                Outline(map.World(_host.Hover.Value), config.CellSize * .4f, .07f, path.Found ? Color.yellow : new Color(1, .2f, .2f));
            _mesh.SetVertices(_vertices); _mesh.SetColors(_colors); _mesh.SetTriangles(_indices, 0); _mesh.RecalculateBounds();
        }
        private void RefreshGrid()
        {
            _host.Map.Cell(_host.Hero.transform.position, out var center);
            var revision = _host.Session?.Revision ?? -1;
            if (_gridCell == center && _gridRevision == revision && _gridCombat == _host.Combat && _gridAll == _host.ShowAllCells) return;
            _gridCell = center; _gridRevision = revision; _gridCombat = _host.Combat; _gridAll = _host.ShowAllCells;
            _gridMesh.Clear(); _vertices.Clear(); _colors.Clear(); _indices.Clear();
            if (!_host.Combat) return;
            var map = _host.Map; var config = _host.World.Config; var size = config.Dimension;
            var radius = _host.ShowAllCells ? size : config.VisibleRadius;
            for (var z = Mathf.Max(0, center.Y - radius); z <= Mathf.Min(size - 1, center.Y + radius); ++z)
            for (var x = Mathf.Max(0, center.X - radius); x <= Mathf.Min(size - 1, center.X + radius); ++x)
            {
                var cell = new GridPosition(x, z);
                if (!map.Board.GetCell(cell).Walkable) continue;
                var color = map.Board.GetCell(cell).MoveCost > 1 ? new Color(1, .65f, .2f, .4f) : new Color(.3f, .8f, .95f, .28f);
                if (cell == _host.Session.Position) color = new Color(.2f, 1, .7f, .9f);
                Outline(map.World(cell), config.CellSize * .47f, .04f, color);
            }
            _gridMesh.SetVertices(_vertices); _gridMesh.SetColors(_colors); _gridMesh.SetTriangles(_indices, 0); _gridMesh.RecalculateBounds();
        }
        private void Outline(Vector3 point, float half, float width, Color color)
        {
            var corners = new[] { new Vector3(-half, 0, -half), new Vector3(half, 0, -half), new Vector3(half, 0, half), new Vector3(-half, 0, half) };
            for (var i = 0; i < 4; ++i)
            {
                var p = point + corners[i];
                if (_host.Map.Support(p, out var support, out _) && Mathf.Abs(support.y - point.y) <= 1) p.y = support.y;
                corners[i] = p + Vector3.up * .07f;
            }
            for (var i = 0; i < 4; ++i) Strip(corners[i], corners[(i + 1) % 4], width, color);
        }
        private void Strip(Vector3 a, Vector3 b, float width, Color color)
        {
            var side = Vector3.Cross((b - a).normalized, Vector3.up).normalized * width * .5f; var n = _vertices.Count;
            _vertices.Add(a - side); _vertices.Add(a + side); _vertices.Add(b + side); _vertices.Add(b - side);
            for (var i = 0; i < 4; ++i) _colors.Add(color);
            _indices.Add(n); _indices.Add(n + 1); _indices.Add(n + 2); _indices.Add(n); _indices.Add(n + 2); _indices.Add(n + 3);
        }
        private void OnDestroy() { if (_mesh != null) Destroy(_mesh); if (_gridMesh != null) Destroy(_gridMesh); if (_root != null) Destroy(_root); }
    }
}
