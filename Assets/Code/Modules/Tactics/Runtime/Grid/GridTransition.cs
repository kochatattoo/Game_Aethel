using System;

namespace PetProject.Tactics
{
    /// <summary>A directed, immutable terrain edge. Unity adapters can forbid physically impossible steps.</summary>
    public readonly struct GridTransition : IEquatable<GridTransition>
    {
        public GridPosition From { get; }
        public GridPosition To { get; }
        public GridTransition(GridPosition from, GridPosition to) { From = from; To = to; }
        public bool Equals(GridTransition other) => From == other.From && To == other.To;
        public override bool Equals(object obj) => obj is GridTransition other && Equals(other);
        public override int GetHashCode() => unchecked(From.GetHashCode() * 397 ^ To.GetHashCode());
    }
}
