using System;
using System.Collections.Generic;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.MotionTest
{
    public sealed class SurfaceGridMap
    {
        private readonly MotionTestConfig _config;
        private readonly Vector3[] _positions;
        private readonly NavMeshQueryFilter _filter;
        public GridBoard Board { get; }
        public double BuildMilliseconds { get; }
        public int WalkableCount { get; }
        public int BlockedEdgeCount { get; }
        public SurfaceGridMap(MotionTestConfig config)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            _config = config;
            _filter = new NavMeshQueryFilter { agentTypeID = config.AgentTypeId, areaMask = NavMesh.AllAreas };
            var size = config.Dimension; _positions = new Vector3[size * size];
            var cells = new List<GridCell>(size * size);
            var canWalk = new bool[size * size];
            for (var z = 0; z < size; ++z)
            for (var x = 0; x < size; ++x)
            {
                var cell = new GridPosition(x, z);
                var center = new Vector3(config.Origin + (x + .5f) * config.CellSize, 0, config.Origin + (z + .5f) * config.CellSize);
                var valid = Support(center, out var point, out var cost) &&
                    NavMesh.SamplePosition(point, out var hit, .22f, _filter) &&
                    new Vector2(point.x - hit.position.x, point.z - hit.position.z).sqrMagnitude < .0064f &&
                    Mathf.Abs(point.y - hit.position.y) < .25f;
                var index = z * size + x; _positions[index] = point; canWalk[index] = valid;
                if (valid) ++WalkableCount;
                cells.Add(new GridCell(cell, valid, Mathf.RoundToInt(point.y / config.HeightUnit), Math.Max(1, cost)));
            }
            var edges = new List<GridTransition>();
            for (var z = 0; z < size; ++z)
            for (var x = 0; x < size; ++x)
            {
                var a = new GridPosition(x, z);
                if (!canWalk[z * size + x]) continue;
                foreach (var delta in new[] { new GridPosition(1, 0), new GridPosition(0, 1) })
                {
                    var b = new GridPosition(x + delta.X, z + delta.Y);
                    if (b.X >= size || b.Y >= size || !canWalk[b.Y * size + b.X]) continue;
                    if (ClearSegment(World(a), World(b))) continue;
                    edges.Add(new GridTransition(a, b)); edges.Add(new GridTransition(b, a));
                }
            }
            BlockedEdgeCount = edges.Count;
            Board = new GridBoard(size, size, cells, edges);
            BuildMilliseconds = timer.Elapsed.TotalMilliseconds;
        }

        public bool Cell(Vector3 world, out GridPosition cell)
        {
            cell = new GridPosition(Mathf.FloorToInt((world.x - _config.Origin) / _config.CellSize),
                Mathf.FloorToInt((world.z - _config.Origin) / _config.CellSize));
            return cell.X >= 0 && cell.Y >= 0 && cell.X < _config.Dimension && cell.Y < _config.Dimension;
        }
        public Vector3 World(GridPosition cell) => _positions[cell.Y * _config.Dimension + cell.X];
        public bool Support(Vector3 xz, out Vector3 point, out int cost)
        {
            point = xz; cost = 1;
            if (!Physics.Raycast(new Vector3(xz.x, 30, xz.z), Vector3.down, out var hit, 60, ~ (1 << 2), QueryTriggerInteraction.Ignore)) return false;
            var surface = hit.collider.GetComponent<MotionSurface>();
            if (surface == null || !surface.Walkable) return false;
            point = hit.point; cost = surface.Cost;
            var settings = NavMesh.GetSettingsByID(_config.AgentTypeId);
            return Vector3.Angle(hit.normal, Vector3.up) <= settings.agentSlope + .1f;
        }
        public bool ClearSegment(Vector3 a, Vector3 b)
        {
            if (NavMesh.Raycast(a, b, out _, _filter)) return false;
            var distance = Vector3.Distance(a, b);
            if (distance < .001f) return true;
            var bottom = a + Vector3.up * (_config.AgentRadius + .06f);
            var top = a + Vector3.up * (_config.AgentHeight - _config.AgentRadius);
            return !Physics.CapsuleCast(bottom, top, _config.AgentRadius, (b - a) / distance, distance,
                1, QueryTriggerInteraction.Ignore);
        }
        public bool NearestReachable(Vector3 from, out GridPosition destination)
        {
            Cell(from, out var origin); var best = float.MaxValue; destination = default; var found = false;
            var path = new NavMeshPath();
            for (var z = origin.Y - 2; z <= origin.Y + 2; ++z)
            for (var x = origin.X - 2; x <= origin.X + 2; ++x)
            {
                var cell = new GridPosition(x, z);
                if (!Board.Contains(cell) || !Board.GetCell(cell).Walkable) continue;
                var point = World(cell); var d = Vector3.SqrMagnitude(from - point);
                if (d > 2.25f || d >= best || !NavMesh.CalculatePath(from, point, _filter, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                destination = cell; best = d; found = true;
            }
            return found;
        }
    }
}
