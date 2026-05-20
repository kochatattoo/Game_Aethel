using UnityEngine;

namespace Infrastructure.AudioSystem
{
    public interface IAudioRegistrator
    {
        void Register(GameObject gameObject);
        void UnRegister(GameObject gameObject);
    }
}