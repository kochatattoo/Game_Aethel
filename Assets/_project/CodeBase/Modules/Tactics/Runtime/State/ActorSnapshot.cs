namespace PetProject.Tactics
{
    public sealed class ActorSnapshot
    {
        public ActorDefinition Definition { get; }
        public string Id => Definition.Id;
        public string FactionId => Definition.FactionId;
        public GridPosition Position { get; }
        public int Health { get; }
        public int ActionPoints { get; }
        public long Initiative { get; }
        public bool IsAlive => Health > 0;

        internal ActorSnapshot(ActorDefinition definition, GridPosition position, int health, int actionPoints, long initiative)
        {
            Definition = definition;
            Position = position;
            Health = health;
            ActionPoints = actionPoints;
            Initiative = initiative;
        }
    }
}
