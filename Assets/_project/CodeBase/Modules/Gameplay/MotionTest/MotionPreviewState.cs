using System;
using System.Diagnostics;
using PetProject.Tactics;

namespace CodeBase.MotionTest
{
    internal sealed class MotionPreviewState
    {
        private long _hoverRevision = -1;

        public GridPath Path { get; private set; } = GridPath.Unreachable;
        public GridPosition? Hover { get; private set; }
        public double Milliseconds { get; private set; }

        public GridPath SetPreview(MotionTestSession session, GridPosition destination,
            Action<string> setNotice, Action refresh)
        {
            if (Hover == destination && _hoverRevision == session.Revision) return Path;
            var timer = Stopwatch.StartNew();
            Path = session.Preview(destination);
            Milliseconds = timer.Elapsed.TotalMilliseconds;
            Hover = destination;
            _hoverRevision = session.Revision;
            setNotice(!Path.Found ? "Нет пути" : Path.Cost > session.Actions
                ? $"Нужно {Path.Cost} ОД; не хватает {Path.Cost - session.Actions}"
                : $"Путь: {Path.Cost} ОД; останется {session.Actions - Path.Cost}");
            refresh();
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
