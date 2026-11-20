using System.Diagnostics;
using PetProject.Tactics;

namespace PetProject.Encounter
{
    public sealed class EncounterPreviewState
    {
        private long _hoverRevision = -1;

        public GridPath Path { get; private set; } = GridPath.Unreachable;
        public GridPosition? Hover { get; private set; }
        public double Milliseconds { get; private set; }

        public GridPath SetPreview(TacticalMovementSession session, GridPosition destination)
        {
            if (Hover == destination && _hoverRevision == session.Revision) return Path;
            var timer = Stopwatch.StartNew();
            Path = session.Preview(destination);
            Milliseconds = timer.Elapsed.TotalMilliseconds;
            Hover = destination;
            _hoverRevision = session.Revision;
            return Path;
        }

        public void SetEncounterPreview(GridPath path, GridPosition cell)
        {
            Path = path;
            Hover = cell;
        }

        public void SetAcceptedPath(GridPath path, GridPosition destination)
        {
            Path = path;
            Hover = destination;
        }

        public void Clear()
        {
            _hoverRevision = -1;
            Path = GridPath.Unreachable;
            Hover = null;
        }
    }
}
