using System.Collections.Generic;

namespace VFXSystem.Parameters.Mapping
{
    public interface IMapConfigs<Tkey, Tvalue>
    {
        public IReadOnlyList<IValue<Tkey, Tvalue>> Definitions { get; }
    }
}
