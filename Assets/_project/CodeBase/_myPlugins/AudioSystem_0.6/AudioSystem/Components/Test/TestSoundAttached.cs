using Infrastructure.AudioSystem.Events;
using UnityEngine;
using Zenject;

namespace Infrastructure.AudioSystem.Components.Test
{
    public class TestSoundAttached: MonoBehaviour
    {
        [SerializeField]
        private AudioEventAsset eventAsset;

        private IAudioFacade _facade;

        [Inject]
        public void Construct(IAudioFacade facade)
        {
            _facade = facade;
        }

        private void Start()
        {
            _facade.PlayAttached(eventAsset,transform);
        }
    }
}
