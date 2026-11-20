namespace PetProject.Tactics
{
    public sealed class TacticalMoveResult
    {
        public GridPosition Destination { get; }
        public long ExpectedRevision { get; }
        public GridPath Path { get; }
        public string Error { get; }
        public bool Accepted => Error == null;
        public bool IsReplay { get; }

        internal TacticalMoveResult(GridPosition destination, long revision, GridPath path, string error, bool replay)
        {
            Destination = destination;
            ExpectedRevision = revision;
            Path = path;
            Error = error;
            IsReplay = replay;
        }

        internal TacticalMoveResult Replay() => new TacticalMoveResult(Destination, ExpectedRevision, Path, Error, true);
    }
}
