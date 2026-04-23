namespace Infrastructure.AudioSystem.Factory
{
    /// <summary>
    /// Контейнер данных для типизированного управления параметром RTPC.
    /// Связывает объект RTPC из движка Wwise с его числовым значением.
    /// Обеспечивает строгую типизацию и выбор параметров через инспектор Unity.
    /// </summary>
    public struct AudioRTPC
    {
        public AK.Wwise.RTPC key;
        public float value;

        public AudioRTPC(AK.Wwise.RTPC key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
