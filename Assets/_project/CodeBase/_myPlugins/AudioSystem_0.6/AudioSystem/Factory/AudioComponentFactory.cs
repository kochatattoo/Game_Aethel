using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Фабрика для управления динамическими аудио-объектами (AudioEntity). 
    /// Обеспечивает эффективное создание, 
    /// привязку и автоматический возврат объектов в пул после завершения проигрывания.
    /// </summary>
    public class AudioComponentFactory : IAudioComponentFactory
    {
        private readonly AudioEntity.Pool _pool;

        public AudioComponentFactory(AudioEntity.Pool pool) => _pool = pool;

        public AudioRequest CreateOneShot(AK.Wwise.Event ev, Vector3 position)
        {
            var entity = GetEntityToPosition(position);
            return entity.Setup().WithEvent(ev);
        }

        public AudioKeyRequest CreateOneShot(AudioKey ev, Vector3 position)
        {
            var entity = GetEntityToPosition(position);
            return entity.SetupByKey().WithEvent(ev);
        }

        public AudioBatchRequest CreateBatchOneShot(AK.Wwise.Event ev, Vector3 position)
        {
            var entity = GetEntityToPosition(position);
            return entity.SetupBatch().WithEvent(ev);
        }

        // --- Точки входа для Attached (родитель) ---

        public AudioRequest CreateAttached(AK.Wwise.Event ev, Transform parent)
        {
            var entity = GetEntityToParent(parent);
            return entity.Setup().WithEvent(ev);
        }

        public AudioKeyRequest CreateAttached(AudioKey ev, Transform parent)
        {
            var entity = GetEntityToParent(parent);
            return entity.SetupByKey().WithEvent(ev);
        }

        public AudioBatchRequest CreateBatchAttached(AK.Wwise.Event ev, Transform parent)
        {
            var entity = GetEntityToParent(parent);
            return entity.SetupBatch().WithEvent(ev);
        }

        private AudioEntity GetEntityToPosition( Vector3 position)
        {
            AudioEntity entity = SpawnEntity();

            entity.transform.position = position;
            return entity;
        }

        private AudioEntity GetEntityToParent(Transform parent)
        {
            AudioEntity entity = SpawnEntity();

            entity.transform.SetParent(parent);
            entity.transform.localPosition = Vector3.zero;

            return entity;
        }

        private AudioEntity SpawnEntity() => _pool.Spawn(_pool);
    }
}
