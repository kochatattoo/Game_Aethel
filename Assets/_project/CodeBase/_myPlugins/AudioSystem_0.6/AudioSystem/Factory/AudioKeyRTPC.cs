using Infrastructure.AudioSystem.Parameters.DTO;

namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Контейнер данных для управления параметром RTPC через строковый ключ.
    /// Связывает уникальное имя параметра в Wwise с его числовым значением.
    /// </summary>
    public struct AudioKeyRTPC
    {
        public AudioKey key;
        public float value;

        public AudioKeyRTPC(AudioKey key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
