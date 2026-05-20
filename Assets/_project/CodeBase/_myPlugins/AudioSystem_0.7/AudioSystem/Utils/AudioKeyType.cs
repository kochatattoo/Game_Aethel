namespace Infrastructure.AudioSystem.Utils
{
    /// <summary>
    /// Тип ключа Wwise, определяющий источник данных для выпадающего списка в инспекторе.
    /// Используется совместно с <see cref="AudioKeyAttribute"/>.
    /// </summary>
    public enum AudioKeyType
    {
        Event,
        Parameter,
        Switch,
        State,
        AuxBus
    }
}
