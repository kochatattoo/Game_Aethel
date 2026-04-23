using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Interfaces
{
    public interface IAudioMaker
    {
        float CurrentSpeed { get; }
        GameObject AudioMakerObject { get; }
    }
}
