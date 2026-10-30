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
}
