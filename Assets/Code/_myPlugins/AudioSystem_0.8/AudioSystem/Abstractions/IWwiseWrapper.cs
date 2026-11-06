namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IWwiseWrapper<out TWwise>
    {
        TWwise WwiseObject { get; }
    }
}
