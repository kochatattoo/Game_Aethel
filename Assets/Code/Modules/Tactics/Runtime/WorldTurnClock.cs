using System;

namespace PetProject.Tactics
{
    public sealed class WorldTurnClock
    {
        private double _elapsed;
        public double RoundDuration { get; }
        public long Round { get; private set; }
        public double SecondsToNextRound => RoundDuration - _elapsed;

        public WorldTurnClock(double roundDuration = 6)
        {
            if (double.IsNaN(roundDuration) || double.IsInfinity(roundDuration) || roundDuration <= 0)
                throw new ArgumentOutOfRangeException(nameof(roundDuration));
            RoundDuration = roundDuration;
        }

        public long Advance(double deltaTime)
        {
            if (double.IsNaN(deltaTime) || double.IsInfinity(deltaTime) || deltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            _elapsed += deltaTime;
            var completed = (long)Math.Floor(_elapsed / RoundDuration);
            if (completed == 0) return 0;
            Round += completed;
            _elapsed -= completed * RoundDuration;
            return completed;
        }

        public void Reset()
        {
            Round = 0;
            _elapsed = 0;
        }
    }
}
