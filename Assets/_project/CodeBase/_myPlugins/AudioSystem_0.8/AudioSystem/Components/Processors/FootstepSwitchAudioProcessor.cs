using AudioSystem.Components.EquipmentConfigs;
using AudioSystem.Components.EquipmentConfigs.EquipmentResolver;
using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Components.Sensors;
using Infrastructure.AudioSystem.Events;
using Infrastructure.AudioSystem.Parameters.DTO;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Processors
{
    // TODO: Подумать над переводом с AK.Wwise на абстракции
    public class FootstepSwitchAudioProcessor: IFootstepAudioProcessor
    {
        private readonly IAudioFacade _audioFacade;
        private readonly IAudioEnviromentMaker _enviromentMaker;
        private readonly FootstepResolverData<AK.Wwise.Switch> _data;
        private readonly IFootstepResolver<AK.Wwise.Switch> _footstepResolver;
        private readonly IEquipmentResolver<BootsType> _equipmentResolver;
        private readonly IEquipmentAudioMaker<BootsType> _equipmentAudioMaker;

        public FootstepSwitchAudioProcessor(
            IAudioFacade audioFacade,
            IAudioEnviromentMaker enviromentMaker,
            FootstepResolverData<AK.Wwise.Switch> data,
            IPhysicAudioMaker physicAudioMaker,
            IEquipmentAudioMaker<BootsType> equipmentAudioMaker,
            BootsMapConfig equipmentMapData)
        {
            _audioFacade = audioFacade;
            _data = data;
            _footstepResolver = new FootstepResolver<AK.Wwise.Switch>(_data, physicAudioMaker);
            _enviromentMaker = enviromentMaker;
            _equipmentAudioMaker = equipmentAudioMaker;
            _equipmentResolver = new BootsEquipmentResolver(equipmentMapData);
        }

        /// <summary>
        /// Метод-обработчик, который вызывается напрямую из Animation Events в клипах анимации.
        /// Передает управление внутреннему резолверу.
        /// </summary>
        /// <param name="evt">Событие анимации с параметрами ноги и ассета звука.</param>
        public void PlayOneShot(AnimationEvent evt)
        {
            // Выбор типа обуви - происходит в IEquipmentAudioMaker<BootsType> и передается сюда через интерфейс
            AK.Wwise.Switch bootsEquipment = _equipmentResolver.GetEquipmentAudioKey(_equipmentAudioMaker.EquipmentType);
            AK.Wwise.Switch surfaceMaterial = _footstepResolver.GetResolvedKey(evt.intParameter, out Transform targetFoot);

            //Debug.Log($"Surface material: {surfaceMaterial}");

            if(bootsEquipment  == null || surfaceMaterial == null)
                return;

            AK.Wwise.Switch[] switches = { bootsEquipment, surfaceMaterial };
            AuxSendData auxData = _enviromentMaker.GetCurrentAuxSendData();

            if (surfaceMaterial == null)
                return;

            if(evt.objectReferenceParameter is AudioEventAsset asset)
            {

                _audioFacade.PlayBatchOneShot(asset, targetFoot.position, _enviromentMaker)
                            .WithAuxSendData(auxData)
                            .WithSwitches(switches)
                            .Play();
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
