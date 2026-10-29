using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    public enum CommandError
    {
        None, CommandIdConflict, EncounterCompleted, UnknownActor, ActorDefeated, WrongTurn,
        InvalidDestination, AlreadyAtDestination, NoPath, NotEnoughActions,
        UnknownTarget, TargetDefeated, FriendlyTarget, TargetOutOfRange
    }

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

    public sealed class CommandResult
    {
        public bool Accepted => Error == CommandError.None;
        public bool IsReplay { get; }
        public CommandError Error { get; }
        public long Revision { get; }
        public IReadOnlyList<TacticalEvent> Events { get; }

        internal CommandResult(CommandError error, long revision, TacticalEvent[] events, bool replay = false)
        {
            Error = error;
            Revision = revision;
            Events = Array.AsReadOnly(events);
            IsReplay = replay;
        }

        internal CommandResult AsReplay() => new CommandResult(Error, Revision, Array.Empty<TacticalEvent>(), true);
    }
}
