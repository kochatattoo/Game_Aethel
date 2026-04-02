using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Sensors;
using Infrastructure.AudioSystem.Events;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Processors
{
    public class FootstepSwitchAudioProcessor: IFootstepAudioProcessor
    {
        // TODO: Можно обобщить класс, но пока в этом нет смысла, как будет больше параметров для обращения к фасаду - сделаю
        private readonly IAudioFacade _audioFacade;
        private readonly FootstepResolverData<AK.Wwise.Switch> _data;
        private readonly IFootstepResolver<AK.Wwise.Switch> _footstepResolver;

        public FootstepSwitchAudioProcessor(IAudioFacade audioFacade, FootstepResolverData<AK.Wwise.Switch> data, IPhysicAudioMaker physicAudioMaker)
        {
            _audioFacade = audioFacade;
            _data = data;
            _footstepResolver = new FootstepResolver<AK.Wwise.Switch>(_data, physicAudioMaker);
        }

        /// <summary>
        /// Метод-обработчик, который вызывается напрямую из Animation Events в клипах анимации.
        /// Передает управление внутреннему резолверу.
        /// </summary>
        /// <param name="evt">Событие анимации с параметрами ноги и ассета звука.</param>
        public void PlayOneShot(AnimationEvent evt)
        {
            AK.Wwise.Switch surfaceMaterial = _footstepResolver.GetResolveredKey(evt.intParameter, out Transform targetFoot);

            if(surfaceMaterial == null)
                return;

            if (evt.objectReferenceParameter is AudioEventAsset asset)
            {
                _audioFacade.PlayOneShot(asset, surfaceMaterial, targetFoot.position);
            }
        }

        /// <summary>
        /// Отрисовка отладочной информации в редакторе при выделении объекта.
        /// Позволяет видеть лучи каста и текущий определенный материал поверхности.
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            _footstepResolver.OnDrawGizmosSelected();
        }
    }
}
