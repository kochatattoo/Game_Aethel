using CodeBase.Configs;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Audio
{
    public interface IAudioService
    {
        void PlayAtTarget(AudioConfig config, Transform target, bool follow = true);
    }
}
