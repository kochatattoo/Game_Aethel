using System;

namespace Infrastructure.AudioSystem.Parameters.DTO
{
    /// <summary>
    /// Универсальный ключ-идентификатор для аудио-событий и параметров Wwise.
    /// Обеспечивает безопасную работу со строковыми ID без лишних аллокаций.
    /// </summary>
    [Serializable]
    public readonly struct AudioKey : IEquatable<AudioKey>
    {
        private readonly string _value;

        /// <summary> Возвращает строковое значение ключа или пустую строку, если значение не задано. </summary>
        public string Value => _value ?? string.Empty;

        public AudioKey(string value) => _value = value;

        /// <summary> Сравнивает два ключа на идентичность строковых значений. </summary>
        public bool Equals(AudioKey other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AudioKey other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        /// <summary> Позволяет автоматически преобразовывать строку в AudioKey. </summary>
        public static implicit operator AudioKey(string value) => new AudioKey(value);

        /// <summary> Позволяет использовать AudioKey там, где ожидается строка. </summary>
        public static implicit operator string(AudioKey key) => key.Value;
    }
}
