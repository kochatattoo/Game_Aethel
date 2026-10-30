using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
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
