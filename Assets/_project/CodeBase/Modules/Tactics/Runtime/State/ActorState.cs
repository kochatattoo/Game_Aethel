using System;

namespace PetProject.Tactics
{
    /// <summary>Mutable combat state, accessible only inside the encounter assembly.</summary>
    internal sealed class ActorState
    {
        public ActorDefinition Definition { get; }
        public GridPosition Position { get; private set; }
        public int Health { get; private set; }
        public int ActionPoints { get; private set; }
        public long Initiative { get; private set; }
        public bool IsAlive => Health > 0;

        public ActorState(ActorDefinition definition)
        {
            Definition = definition;
            Position = definition.Position;
            Health = definition.MaxHealth;
        }

        public void AssignInitiative(long initiative) => Initiative = initiative;
        public void BeginTurn(int budget) => ActionPoints = budget;
        public void EndTurn() => ActionPoints = 0;
        public void SpendActions(int cost) => ActionPoints -= cost;

        public void MoveTo(GridPosition position, int cost)
        {
            Position = position;
            SpendActions(cost);
        }

        public int TakeDamage(int damage)
        {
            var applied = Math.Min(Health, damage);
            Health -= applied;
            return applied;
        }

        public ActorSnapshot Snapshot() => new ActorSnapshot(Definition, Position, Health, ActionPoints, Initiative);
    }
}
