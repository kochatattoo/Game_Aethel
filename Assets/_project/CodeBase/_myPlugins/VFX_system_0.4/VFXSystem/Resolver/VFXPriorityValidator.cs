namespace VFXSystem.Resolver
{
    // Для него надо все подготовить, а то он пуст
    public class VFXPriorityValidator<T> : IVFXValidator<T> 
    {
        //private readonly IVFXCountProvider _countProvider; // Нужен доступ к общему числу активных эффектов
        private const int GLOBAL_THRESHOLD = 40; // После 40 эффектов запрещаем спавн Low Priority

        public bool CanSpawn(VFXSpawnContext<T> context)
        {
            // Данная логика еще не реализована

            //int totalActive = _countProvider.TotalActiveCount;

            // Если эффектов много, спавним только критически важные (High)
            //if (totalActive > GLOBAL_THRESHOLD && context.Definition.Priority == VFXPriority.Low)
            //{
            //    return false;
            //}

            return true;
        }

        public void OnEffectSpawned(T definition) { }
        public void OnEffectDespawned(T definition) { }
    }
}
