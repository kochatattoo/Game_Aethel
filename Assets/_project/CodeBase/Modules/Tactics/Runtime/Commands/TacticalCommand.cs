using System;

namespace PetProject.Tactics
{
    public enum TacticalCommandKind { Move, Attack, EndTurn }

    public sealed class TacticalCommand : IEquatable<TacticalCommand>
    {
        public string Id { get; }
        public string ActorId { get; }
        public TacticalCommandKind Kind { get; }
        public GridPosition Destination { get; }
        public string TargetId { get; }

        private TacticalCommand(string id, string actorId, TacticalCommandKind kind, GridPosition destination, string targetId)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Command ID is required", nameof(id));
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor ID is required", nameof(actorId));
            Id = id;
            ActorId = actorId;
            Kind = kind;
            Destination = destination;
            TargetId = targetId;
        }

        public static TacticalCommand Move(string id, string actorId, GridPosition destination) =>
            new TacticalCommand(id, actorId, TacticalCommandKind.Move, destination, null);
        public static TacticalCommand Attack(string id, string actorId, string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("Target ID is required", nameof(targetId));
            return new TacticalCommand(id, actorId, TacticalCommandKind.Attack, default, targetId);
        }
        public static TacticalCommand EndTurn(string id, string actorId) =>
            new TacticalCommand(id, actorId, TacticalCommandKind.EndTurn, default, null);

        public bool Equals(TacticalCommand other) => other != null && Id == other.Id && ActorId == other.ActorId &&
            Kind == other.Kind && Destination == other.Destination && TargetId == other.TargetId;
        public override bool Equals(object obj) => obj is TacticalCommand other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Id);
    }
}
