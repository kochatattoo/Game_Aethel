using Infrastructure.AudioSystem.Components.Interfaces;
using UnityEngine;


namespace Infrastructure.AudioSystem.Components.Triggers
{
    /// <summary>
    /// Компонент-триггер по тегу, который принудительно устанавливает состояние переключателя (Wwise Switch) для входящего объекта.
    /// </summary>
    public class SwitchTriggerAudioComponent : BaseSwitchTriggerAudioComponent
    {

        protected override void OnEnter(Collider other)
        {
            if (other.TryGetComponent<IAudioMaker>(out IAudioMaker maker))
            {
                _audioFacade.SetSwitch(_audioSwitchAsset.WwiseSwitch, other.gameObject);
            }
        }

        protected override void OnExit(Collider other)
        {
            if (other.TryGetComponent<IAudioMaker>(out IAudioMaker maker))
            {
                // TODO: Реализовать функционал авто-хранения прошлого свича
                // Требуется хранить свияи всех объектов в отдельном хранилище и получать через фасад инфу
                // По API Wwise получить значения Switch не получиться, он Fire & Forget
                // Пока что ручками ставим прошлый свитч

                _audioFacade.SetSwitch(_defaultSwitchAsset.WwiseSwitch, other.gameObject);
            }
        }
    }
}
