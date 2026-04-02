using Infrastructure.AudioSystem.Parameters.DTO;
using System;
using UniRx;
using UnityEngine;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Фабрика для управления динамическими аудио-объектами (AudioEntity). 
    /// Обеспечивает эффективное создание, 
    /// привязку и автоматический возврат объектов в пул после завершения проигрывания.
    /// </summary>
    public class AudioComponentFactory : IAudioComponentFactory, IDisposable
    {
        private readonly AudioEntity.Pool _pool;
        private readonly DictionaryDisposable<AudioEntity, IDisposable> _disposables = new();

        public AudioComponentFactory(AudioEntity.Pool pool)
        {
            _pool = pool;
        }

        public void CreateOneShot(AudioKey eventName, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName);
        }

        public void CreateOneShot(AK.Wwise.Event eventName, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName);
        }

        public void CreateOneShot(AudioKey eventName, AudioKey switchName, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName, switchName);
        }

        public void CreateOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName, switchName);
        }

        public void CreateOneShot(AudioKey eventName, AudioKey paramName, float value, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName, paramName, value);
        }

        public void CreateOneShot(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName, paramName, value);
        }

        public void CreateOneShot(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName,switchName, paramName, value);
        }

        public void CreateOneShot(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Vector3 position)
        {
            AudioEntity entity = GetEntityToPosition(position);
            entity.Play(eventName,switchName,paramName, value);
        }

        public void CreateAttached(AudioKey eventName, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName);
        }

        public void CreateAttached(AK.Wwise.Event eventName, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName);
        }

        public void CreateAttached(AudioKey eventName, AudioKey switchName, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName, switchName);
        }

        public void CreateAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName, switchName);
        }

        public void CreateAttached(AudioKey eventName, AudioKey paramName, float value, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName, paramName, value);
        }

        public void CreateAttached(AK.Wwise.Event eventName, AK.Wwise.RTPC paramName, float value, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName,paramName, value);
        }

        public void CreateAttached(AudioKey eventName, AudioKey switchName, AudioKey paramName, float value, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName, switchName, paramName, value);
        }

        public void CreateAttached(AK.Wwise.Event eventName, AK.Wwise.Switch switchName, AK.Wwise.RTPC paramName, float value, Transform parent)
        {
            AudioEntity entity = GetEntityToParent(parent);
            entity.Play(eventName, switchName, paramName, value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private AudioEntity GetEntityToPosition( Vector3 position)
        {
            AudioEntity entity = SpawnEntity();

            entity.transform.position = position;
            _disposables[entity] = entity.SoundFinished.Subscribe(OnSoundFinished);
            return entity;
        }

        private AudioEntity GetEntityToParent(Transform parent)
        {
            AudioEntity entity = SpawnEntity();

            entity.transform.SetParent(parent);
            entity.transform.localPosition = Vector3.zero;
            _disposables[entity] = entity.SoundFinished.Subscribe(OnSoundFinished);
            return entity;
        }

        private AudioEntity SpawnEntity()
        {
            AudioEntity entity = _pool.Spawn();
            entity.RegisterObject();
            return entity;
        }

        private void OnSoundFinished(AudioEntity entity)
        {
            _disposables[entity].Dispose();
            _disposables.Remove(entity);
            _pool.Despawn(entity);
        }
    }
}
