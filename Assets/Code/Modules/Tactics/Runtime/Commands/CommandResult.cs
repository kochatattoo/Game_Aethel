using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
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
