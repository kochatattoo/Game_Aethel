namespace VFXSystem.Resolver
{
    public interface IVFXValidator<T>
    {
        bool CanSpawn(VFXSpawnContext<T> context);
        void OnEffectSpawned(T effectId);
        void OnEffectDespawned(T effectId);
    }

}
