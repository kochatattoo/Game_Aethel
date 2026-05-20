using Infrastructure.AudioSystem.Components.MaterialConfigs;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.Sensors
{
    // TODO: Возможно стоит переделать на структуру

    /// <summary>
    /// Контейнер данных для инициализации и работы резолверов шагов.
    /// Хранит ссылки на кости персонажа, корневой объект и конфигурацию параметров сканирования.
    /// </summary>
    /// <typeparam name="T">Тип ключа Wwise Switch, используемый для смены аудио-материала.</typeparam>
    public class FootstepResolverData<T>
    {
        /// <summary>
        /// Корневой объект персонажа (Root). Используется для базовых расчетов позиции и отладочных надписей.
        /// </summary>
        public Transform Root { get; }

        /// <summary>
        /// Точка вылета луча (Raycast) для левой ноги. Обычно это кость ступни или Toe.
        /// </summary>
        public Transform LeftFoot { get; }

        /// <summary>
        /// Точка вылета луча (Raycast) для правой ноги. Обычно это кость ступни или Toe.
        /// </summary>
        public Transform RightFoot { get; }

        /// <summary>
        /// Общая конфигурация параметров: дистанция каста, слои коллизий и маппинг материалов.
        /// </summary>
        public BaseSurfaceResolverConfig<T> Config { get; }

        /// <summary>
        /// Инициализирует набор данных, необходимых для корректного поиска поверхности под ногами.
        /// </summary>
        /// <param name="root">Трансформ-родитель всего персонажа.</param>
        /// <param name="leftFoot">Трансформ левой ступни.</param>
        /// <param name="rightFoot">Трансформ правой ступни.</param>
        /// <param name="config">Объект настроек системы определения поверхностей.</param>
        public FootstepResolverData(
            Transform root, 
            Transform leftFoot, 
            Transform rightFoot, 
            BaseSurfaceResolverConfig<T> config)
        {
            Root = root;
            LeftFoot = leftFoot;
            RightFoot = rightFoot;
            Config = config;
        }
    }
}
