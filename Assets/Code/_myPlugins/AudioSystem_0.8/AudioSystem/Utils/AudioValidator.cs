using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.AudioSystem.Utils
{
    /// <summary>
    /// Статический валидатор для обеспечения корректности данных и состояния объектов в аудиосистеме.
    /// Предотвращает передачу невалидных имен, значений RTPC и незарегистрированных объектов в Wwise.
    /// </summary>
    public partial class AudioValidator
    {
        private const float MinRtpc = 0f;
        private const float MaxRtpc = 100f;

        // Храним ID объектов, которые уже "представились" Wwise
        private static readonly HashSet<int> RegisteredObjects = new(); // TODO: Выдать в сервис список (если потребуется)

        /// <summary>
        /// Проверяет строковое имя на пустоту и обрезает пробелы.
        /// </summary>
        /// <param name="name">Исходное имя ключа.</param>
        /// <param name="context">Контекст вызова для логирования ошибок.</param>
        /// <param name="sanitized">Очищенное имя на выходе.</param>
        /// <returns>True, если имя валидно.</returns>
        public static bool IsValidName(string name, string context, out string sanitized)
        {
            sanitized = name?.Trim();

            if (string.IsNullOrEmpty(sanitized))
            {
                Debug.LogWarning($"[AudioValidator] {context}: Имя пустое или null.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Ограничивает значение RTPC параметра в диапазоне 0-100.
        /// Логирует предупреждение, если исходное значение вышло за пределы.
        /// </summary>
        public static float ClampRtpc(float value)
        {
            if (value < MinRtpc || value > MaxRtpc)
            {
                Debug.LogFormat("[AudioValidator] RTPC '{0}' value {1} out of range (0-100). Clamped.", value);
            }
            return Mathf.Clamp(value, MinRtpc, MaxRtpc);
        }

        /// <summary>
        /// Проверяет, зарегистрирован ли GameObject в движке Wwise. 
        /// Если нет — пытается зарегистрировать его немедленно.
        /// </summary>
        /// <param name="target">Целевой объект (эмиттер).</param>
        /// <param name="context">Контекст для отладки.</param>
        /// <returns>True, если объект готов к воспроизведению звука.</returns>
        public static bool IsGameObjectReady(GameObject target, string context = null)
        {
            if (target == null) 
                return true; // Глобальные вызовы (null) всегда валидны

            int instanceId = target.GetInstanceID();

            if (!RegisteredObjects.Contains(instanceId))
            {
                // Пытаемся зарегистрировать на лету, если забыли
                var result = AkUnitySoundEngine.RegisterGameObj(target, target.name);
                if (result == AKRESULT.AK_Success)
                {
                    RegisteredObjects.Add(instanceId);
                    return true;
                }

                Debug.LogWarning($"[AudioValidator] {context}: Не удалось зарегистрировать {target.name}. Звук проигнорирован.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удаляет объект из внутреннего реестра валидатора. 
        /// Должен вызываться при деактивации или уничтожении объекта.
        /// </summary>
        public static void UnregisterObject(GameObject target)
        {
            if (target == null) 
                return;

            int instanceId = target.GetInstanceID();

            if (RegisteredObjects.Contains(instanceId))
            {
                AkUnitySoundEngine.UnregisterGameObj(target);
                RegisteredObjects.Remove(instanceId);
            }
        }

        /// <summary>
        /// Регестрируем объект внутри движка Wwise
        /// </summary>
        /// <param name="target">Объект регистрации в движке</param>
        /// <param name="context">Контекст описания объекта</param>
        public static void RegisterObject(GameObject target, string context = null)
        {
            if (target == null)
                return; 

            int instanceId = target.GetInstanceID();

            if (!RegisteredObjects.Contains(instanceId))
            {
                var result = AkUnitySoundEngine.RegisterGameObj(target, target.name);
                if (result == AKRESULT.AK_Success)
                {
                    RegisteredObjects.Add(instanceId);
                    return;
                }

                Debug.LogWarning($"[AudioValidator] {context}: Не удалось зарегистрировать {target.name}. Звук проигнорирован. Результат {result}");
            }
        }

        /// <summary>
        /// Принудительно обновляет имя игрового объекта в профайлере Wwise.
        /// Используется, если имя GameObject изменилось в Runtime, чтобы в Capture Log отображалось актуальное название.
        /// </summary>
        /// <param name="target">Игровой объект, имя которого нужно обновить.</param>
        public static void UpdateObjectName(GameObject target)
        {
            if (target == null) 
                return;

            int instanceId = target.GetInstanceID();


            var result = AkUnitySoundEngine.RegisterGameObj(target, target.name);

            if (result == AKRESULT.AK_Success)
            {
                if (!RegisteredObjects.Contains(instanceId))
                {
                    RegisteredObjects.Add(instanceId);
                }
            }
            else
            {
                Debug.LogWarning($"[AudioValidator] Не удалось обновить имя для {target.name}");
            }
        }
    }
}
