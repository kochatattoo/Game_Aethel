using UnityEngine;

namespace CodeBase.Configs
{
    [CreateAssetMenu(fileName = nameof(AudioConfig), menuName = "StaticData/"+ nameof(AudioConfig))]
    public class AudioConfig: ScriptableObject
    {
        [field: SerializeField]
        public AudioClip Clip {  get; private set; }    
        [field: SerializeField, Range(0, 1)] 
        public float Volume { get; private set; }
        [field: SerializeField, Range(0, 2)] 
        public float PitchMin { get; private set; }
        [field: SerializeField, Range(0, 2)] 
        public float PitchMax { get; private set; }

        public void ApplyTo(AudioSource source)
        {
            source.clip = Clip;
            source.volume = Volume;
            source.pitch = UnityEngine.Random.Range(PitchMin, PitchMax);
        }
    }
}
