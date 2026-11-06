namespace PetProject.Tactics
{
    public enum TacticalEventKind { Moved, AttackHit, AttackMiss, ActorDefeated, TurnEnded, TurnStarted, EncounterCompleted }

    public readonly struct TacticalEvent
    {
        public TacticalEventKind Kind { get; }
        public string ActorId { get; }
        public string TargetId { get; }
        public GridPosition Position { get; }
        public int Amount { get; }
        public int Roll { get; }

        internal TacticalEvent(TacticalEventKind kind, string actorId, string targetId = null,
            GridPosition position = default, int amount = 0, int roll = 0)
        {
            Kind = kind;
            ActorId = actorId;
            TargetId = targetId;
            Position = position;
            Amount = amount;
            Roll = roll;
        }
    }
}
