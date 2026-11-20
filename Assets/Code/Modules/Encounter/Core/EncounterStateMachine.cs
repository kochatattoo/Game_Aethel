using System;

namespace PetProject.Encounter
{
    public enum EncounterState
    {
        Exploration,
        Following,
        Aligning,
        Combat,
        Defeat
    }

    /// <summary>Допустимые переходы между свободным исследованием и столкновением.</summary>
    public sealed class EncounterStateMachine
    {
        public EncounterState State { get; private set; } = EncounterState.Exploration;

        public void Reset() => State = EncounterState.Exploration;

        public void TransitionTo(EncounterState next)
        {
            if (State == next || CanTransition(State, next))
            {
                State = next;
                return;
            }
            throw new InvalidOperationException($"Недопустимый переход encounter: {State} → {next}");
        }

        private static bool CanTransition(EncounterState current, EncounterState next)
        {
            if (next == EncounterState.Exploration) return true;
            if (current == EncounterState.Exploration)
                return next == EncounterState.Following || next == EncounterState.Aligning;
            if (current == EncounterState.Following)
                return next == EncounterState.Aligning;
            if (current == EncounterState.Aligning) return next == EncounterState.Combat;
            return current == EncounterState.Combat && next == EncounterState.Defeat;
        }
    }
}
