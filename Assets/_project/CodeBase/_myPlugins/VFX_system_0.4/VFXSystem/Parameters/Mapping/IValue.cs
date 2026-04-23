namespace VFXSystem.Parameters.Mapping
{
    public interface IValue<Tkey, Tvalue>
    {
        Tkey Key { get; }
        Tvalue Value { get; }
    }
}
