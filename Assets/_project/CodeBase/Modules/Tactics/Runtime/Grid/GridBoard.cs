using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    public readonly struct GridCell
    {
        public GridPosition Position { get; }
        public bool Walkable { get; }
        public int Height { get; }
        public int MoveCost { get; }

        public GridCell(GridPosition position, bool walkable = true, int height = 0, int moveCost = 1)
        {
            if (moveCost < 1) throw new ArgumentOutOfRangeException(nameof(moveCost));
            Position = position;
            Walkable = walkable;
            Height = height;
            MoveCost = moveCost;
        }
    }

    /// <summary>Immutable terrain. Actor occupancy belongs to the encounter, not this definition.</summary>
    public sealed class GridBoard
    {
        private readonly GridCell[] _cells;
        public int Width { get; }
        public int Height { get; }

        public GridBoard(int width, int height, IEnumerable<GridCell> overrides = null)
        {
            if (width < 1 || height < 1 || (long)width * height > 1_000_000)
                throw new ArgumentOutOfRangeException(nameof(width), "Board must contain 1..1,000,000 cells");
            Width = width;
            Height = height;
            _cells = new GridCell[width * height];
            for (var y = 0; y < height; ++y)
                for (var x = 0; x < width; ++x)
                    _cells[y * width + x] = new GridCell(new GridPosition(x, y));
            if (overrides == null) return;
            var seen = new HashSet<GridPosition>();
            foreach (var cell in overrides)
            {
                if (!Contains(cell.Position) || cell.MoveCost < 1 || !seen.Add(cell.Position))
                    throw new ArgumentException("Invalid or duplicate terrain override", nameof(overrides));
                _cells[Index(cell.Position)] = cell;
            }
        }

        public bool Contains(GridPosition position) => position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        public GridCell GetCell(GridPosition position)
        {
            if (!Contains(position)) throw new ArgumentOutOfRangeException(nameof(position));
            return _cells[Index(position)];
        }
        private int Index(GridPosition position) => position.Y * Width + position.X;
    }
}
