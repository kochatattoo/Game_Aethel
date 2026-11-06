using CodeBase.Components.Audio;
using CodeBase.Configs;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Audio
{
    public class AudioService : IAudioService
    {
        private readonly AudioPoolItem.Pool _pool;

        public AudioService(AudioPoolItem.Pool pool)
        {
            _pool = pool;
        }

        public void PlayAtTarget(AudioConfig config, Transform target, bool follow = true)
        {
            if (config == null || config.Clip == null) return;

            var audioItem = _pool.Spawn(_pool);

            if (follow)
            {
                audioItem.transform.SetParent(target);
                audioItem.transform.localPosition = Vector3.zero;
            }
            else
            {
                audioItem.transform.position = target.position;
            }

            audioItem.Play(config, config.Clip.length);
        }
    }
}
