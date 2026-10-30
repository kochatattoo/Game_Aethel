namespace PetProject.Tactics
{
    public enum CommandError
    {
        None, CommandIdConflict, EncounterCompleted, UnknownActor, ActorDefeated, WrongTurn,
        InvalidDestination, AlreadyAtDestination, NoPath, NotEnoughActions,
        UnknownTarget, TargetDefeated, FriendlyTarget, TargetOutOfRange
    }
}
