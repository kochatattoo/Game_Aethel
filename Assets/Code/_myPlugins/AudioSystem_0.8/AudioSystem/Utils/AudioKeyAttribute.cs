using UnityEngine;

namespace Infrastructure.AudioSystem.Utils
{
    /// <summary>
    /// Атрибут для полей строкового типа или AudioKey. 
    /// Превращает обычное текстовое поле в выпадающий список (Dropdown) с ключами из Wwise проекта.
    /// </summary>
    public class AudioKeyAttribute : PropertyAttribute
    {
        /// <summary> Целевой тип ключа (Event, Parameter, Switch, State), определяющий источник данных для списка. </summary>
        public AudioKeyType Type;

        /// <summary>
        /// Помечает поле для отображения в виде списка ключей Wwise.
        /// </summary>
        /// <param name="type">Тип ключей, которые должны быть доступны для выбора.</param>
        public AudioKeyAttribute(AudioKeyType type) => Type = type;
    }
}
