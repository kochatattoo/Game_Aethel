using System;

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
}
