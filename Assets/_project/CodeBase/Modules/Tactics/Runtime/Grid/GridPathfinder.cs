using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    public sealed class GridPath
    {
        public static GridPath Unreachable { get; } = new GridPath(false, 0, Array.Empty<GridPosition>());
        public bool Found { get; }
        public long Cost { get; }
        public IReadOnlyList<GridPosition> Cells { get; }
        internal GridPath(bool found, long cost, GridPosition[] cells)
        {
            Found = found;
            Cost = cost;
            Cells = Array.AsReadOnly(cells);
        }
    }

    public static class GridPathfinder
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0), new GridPosition(0, 1), new GridPosition(-1, 0), new GridPosition(0, -1)
        };

        /// <summary>Dijkstra: terrain costs may differ. Equal costs are resolved by coordinates.</summary>
        public static GridPath FindPath(GridBoard board, GridPosition start, GridPosition goal,
            IEnumerable<GridPosition> occupied = null, int maxHeightStep = 1)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (maxHeightStep < 0) throw new ArgumentOutOfRangeException(nameof(maxHeightStep));
            if (!board.Contains(start) || !board.Contains(goal) || !board.GetCell(start).Walkable || !board.GetCell(goal).Walkable)
                return GridPath.Unreachable;
            var blocked = occupied == null ? new HashSet<GridPosition>() : new HashSet<GridPosition>(occupied);
            blocked.Remove(start);
            if (blocked.Contains(goal)) return GridPath.Unreachable;
            var costs = new Dictionary<GridPosition, long> { [start] = 0 };
            var previous = new Dictionary<GridPosition, GridPosition>();
            var frontier = new SortedSet<(long cost, int x, int y)> { (0, start.X, start.Y) };
            while (frontier.Count > 0)
            {
                var item = frontier.Min;
                frontier.Remove(item);
                var current = new GridPosition(item.x, item.y);
                if (costs[current] != item.cost) continue;
                if (current == goal) return BuildPath(start, goal, item.cost, previous);
                foreach (var direction in Directions)
                {
                    var next = new GridPosition(current.X + direction.X, current.Y + direction.Y);
                    if (!board.Contains(next) || blocked.Contains(next)) continue;
                    var tile = board.GetCell(next);
                    if (!board.CanTraverse(current, next, maxHeightStep)) continue;
                    var cost = item.cost + tile.MoveCost;
                    if (costs.TryGetValue(next, out var known) && known <= cost) continue;
                    costs[next] = cost;
                    previous[next] = current;
                    frontier.Add((cost, next.X, next.Y));
                }
            }
            return GridPath.Unreachable;
        }

        private static GridPath BuildPath(GridPosition start, GridPosition goal, long cost,
            IReadOnlyDictionary<GridPosition, GridPosition> previous)
        {
            var cells = new List<GridPosition> { goal };
            while (cells[cells.Count - 1] != start) cells.Add(previous[cells[cells.Count - 1]]);
            cells.Reverse();
            return new GridPath(true, cost, cells.ToArray());
        }
    }
}
