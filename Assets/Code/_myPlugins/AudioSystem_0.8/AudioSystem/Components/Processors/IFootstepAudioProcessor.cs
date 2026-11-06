using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Processors
{
    public interface IFootstepAudioProcessor
    {
        void PlayOneShot(AnimationEvent evt);
        void OnDrawGizmosSelected();
    }
}
