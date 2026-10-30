using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    /// <summary>Single-actor movement fixture. It shares terrain/path rules with Encounter, without inventing a second faction.</summary>
    public sealed class MotionTestSession
    {
        private readonly GridBoard _board;
        private readonly int _heightStep;
        private readonly Dictionary<string, MotionMoveResult> _accepted = new Dictionary<string, MotionMoveResult>();
        public GridPosition Position { get; private set; }
        public int Actions { get; private set; }
        public int Budget { get; }
        public long Revision { get; private set; }

        public MotionTestSession(GridBoard board, GridPosition start, int budget, int maxHeightStep = 4)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            if (!board.Contains(start) || !board.GetCell(start).Walkable) throw new ArgumentException("Invalid spawn", nameof(start));
            if (budget < 1 || maxHeightStep < 0) throw new ArgumentOutOfRangeException(nameof(budget));
            Position = start; Actions = Budget = budget; _heightStep = maxHeightStep;
        }

        public GridPath Preview(GridPosition destination) => GridPathfinder.FindPath(_board, Position, destination, null, _heightStep);

        public MotionMoveResult Move(string id, GridPosition destination, long expectedRevision)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Command ID required", nameof(id));
            if (_accepted.TryGetValue(id, out var previous))
                return previous.Destination == destination && previous.ExpectedRevision == expectedRevision
                    ? previous.Replay() : Reject(destination, expectedRevision, "CommandIdConflict");
            if (Revision != expectedRevision) return Reject(destination, expectedRevision, "StalePreview");
            if (!_board.Contains(destination)) return Reject(destination, expectedRevision, "OutsideBoard");
            if (destination == Position) return Reject(destination, expectedRevision, "AlreadyAtDestination");
            var path = Preview(destination);
            if (!path.Found) return Reject(destination, expectedRevision, "NoPath");
            if (path.Cost > Actions) return Reject(destination, expectedRevision, "NotEnoughActions");
            Position = destination;
            Actions -= (int)path.Cost;
            ++Revision;
            var result = new MotionMoveResult(destination, expectedRevision, path, null, false);
            _accepted.Add(id, result);
            return result;
        }

        public void NewTurn() { Actions = Budget; ++Revision; }
        private static MotionMoveResult Reject(GridPosition destination, long revision, string error) =>
            new MotionMoveResult(destination, revision, GridPath.Unreachable, error, false);
    }

    public sealed class MotionMoveResult
    {
        public GridPosition Destination { get; }
        public long ExpectedRevision { get; }
        public GridPath Path { get; }
        public string Error { get; }
        public bool Accepted => Error == null;
        public bool IsReplay { get; }
        internal MotionMoveResult(GridPosition destination, long revision, GridPath path, string error, bool replay)
        { Destination = destination; ExpectedRevision = revision; Path = path; Error = error; IsReplay = replay; }
        internal MotionMoveResult Replay() => new MotionMoveResult(Destination, ExpectedRevision, Path, Error, true);
    }
}
