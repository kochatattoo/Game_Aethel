using System.Collections.Generic;

namespace PetProject.Tactics
{
    public static class GridAdjacency
    {
        public static bool IsAdjacent(GridPosition first, GridPosition second)
        {
            var deltaX = (long)first.X - second.X;
            var deltaY = (long)first.Y - second.Y;
            return (deltaX != 0 || deltaY != 0) &&
                deltaX >= -1 && deltaX <= 1 && deltaY >= -1 && deltaY <= 1;
        }

        public static IEnumerable<GridPosition> GetNeighbors(GridPosition center)
        {
            for (var y = -1; y <= 1; ++y)
            for (var x = -1; x <= 1; ++x)
            {
                if (x == 0 && y == 0) continue;
                yield return new GridPosition(center.X + x, center.Y + y);
            }
        }
    }
}
