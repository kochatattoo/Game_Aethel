using System;
using System.Collections.Generic;

namespace PetProject.Navigation
{
    public readonly struct NavigationPoint
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public NavigationPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static float Distance(NavigationPoint a, NavigationPoint b) =>
            MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z));
    }

    public readonly struct RemainingPath
    {
        public int FirstCornerIndex { get; }
        public NavigationPoint Start { get; }
        public float Length { get; }

        public RemainingPath(int firstCornerIndex, NavigationPoint start, float length)
        {
            FirstCornerIndex = firstCornerIndex;
            Start = start;
            Length = length;
        }
    }

    public static class RemainingPathCalculator
    {
        public static RemainingPath Calculate(IReadOnlyList<NavigationPoint> corners, NavigationPoint? actorPosition)
        {
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (corners.Count < 2) throw new ArgumentException("Path requires at least two corners", nameof(corners));
            var first = 0;
            var start = actorPosition ?? corners[0];
            if (actorPosition.HasValue && corners.Count > 2)
            {
                var actor = actorPosition.Value;
                var bestDistance = float.MaxValue;
                for (var i = 0; i < corners.Count - 1; ++i)
                {
                    var point = ClosestPoint(actor, corners[i], corners[i + 1]);
                    var distance = DistanceSquared(point, actor);
                    if (distance >= bestDistance) continue;
                    bestDistance = distance;
                    first = i;
                    start = point;
                }
            }
            var length = 0f;
            var previous = start;
            for (var i = first + 1; i < corners.Count; ++i)
            {
                length += NavigationPoint.Distance(previous, corners[i]);
                previous = corners[i];
            }
            return new RemainingPath(first, start, length);
        }

        private static NavigationPoint ClosestPoint(NavigationPoint point, NavigationPoint start, NavigationPoint end)
        {
            var x = end.X - start.X;
            var y = end.Y - start.Y;
            var z = end.Z - start.Z;
            var lengthSquared = x * x + y * y + z * z;
            if (lengthSquared < .0001f) return start;
            var t = ((point.X - start.X) * x + (point.Y - start.Y) * y + (point.Z - start.Z) * z) / lengthSquared;
            t = Math.Clamp(t, 0, 1);
            return new NavigationPoint(start.X + x * t, start.Y + y * t, start.Z + z * t);
        }

        private static float DistanceSquared(NavigationPoint a, NavigationPoint b) =>
            (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z);
    }
}
