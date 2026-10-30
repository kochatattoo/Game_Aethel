namespace PetProject.Tactics
{
    public sealed class MotionMoveResult
    {
        public GridPosition Destination { get; }
        public long ExpectedRevision { get; }
        public GridPath Path { get; }
        public string Error { get; }
        public bool Accepted => Error == null;
        public bool IsReplay { get; }

        internal MotionMoveResult(GridPosition destination, long revision, GridPath path, string error, bool replay)
        {
            Destination = destination;
            ExpectedRevision = revision;
            Path = path;
            Error = error;
            IsReplay = replay;
        }

        internal MotionMoveResult Replay() => new MotionMoveResult(Destination, ExpectedRevision, Path, Error, true);
    }
}
