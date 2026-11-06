using UnityEngine;

namespace Infrastructure.AudioSystem.Components
{
    public class WwiseBackgroundAudio : MonoBehaviour
    {
        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                // Возвращаем звук, если он был приостановлен
                AkUnitySoundEngine.WakeupFromSuspend();
                //Debug.Log("Wwise: Audio Resumed");
            }
            else
            {
                // Если Run In Background включен, можно закомментировать строку ниже,
                // чтобы звук НЕ выключался в фоне.
                //AkUnitySoundEngine.Suspend();
                //Debug.Log("Wwise: Audio focus lost, but keeping alive if Suspend is commented");
            }
        }
    }
}
