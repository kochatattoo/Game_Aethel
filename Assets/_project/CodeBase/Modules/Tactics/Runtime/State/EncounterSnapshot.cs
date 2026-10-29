using System;
using System.Collections.Generic;

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

    /// <summary>Read-only view data. Disk persistence and campaign schema migration are separate work.</summary>
    public sealed class EncounterSnapshot
    {
        public long Revision { get; }
        public int Round { get; }
        public string CurrentActorId { get; }
        public string WinningFactionId { get; }
        public bool IsCompleted => WinningFactionId != null;
        public uint RandomState { get; }
        public IReadOnlyList<ActorSnapshot> Actors { get; }

        internal EncounterSnapshot(long revision, int round, string currentActorId, string winningFactionId,
            uint randomState, ActorSnapshot[] actors)
        {
            Revision = revision;
            Round = round;
            CurrentActorId = currentActorId;
            WinningFactionId = winningFactionId;
            RandomState = randomState;
            Actors = Array.AsReadOnly(actors);
        }
    }
}
