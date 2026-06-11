using Infrastructure.AudioSystem.Components;
using UnityEngine;

namespace Infrastructure.AudioSystem
{
    public interface IAudioRegistrator
    {
        void Register(GameObject gameObject);
        void UnRegister(GameObject gameObject);

        /// <summary>
        /// Регистрирует комнату в Wwise, используя данные из RoomComponent.
        /// </summary>
        void RegisterRoom(RoomComponent room);

        /// <summary>
        /// Удаляет регистрацию комнаты (выгружает из Wwise).
        /// </summary>
        void UnregisterRoom(RoomComponent room);
    }
}