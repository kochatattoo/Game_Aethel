using System.Collections.Generic;

namespace VFXSystem.Parameters.Mapping
{
    public interface IMap<Tkey, Tvalue>
    {
        public IReadOnlyDictionary<Tkey, Tvalue> Effects { get; }
    }
}
