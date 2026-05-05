using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Interfaces
{
    public interface IPhysicAudioMaker 
    {
        float CurrentSpeed { get; }
        CharacterController CharacterController { get; }
    }
}
