namespace PetProject.Tactics
{
    public interface IRandomSource
    {
        uint State { get; }
        int NextInt(int minimumInclusive, int maximumExclusive);
    }
}
