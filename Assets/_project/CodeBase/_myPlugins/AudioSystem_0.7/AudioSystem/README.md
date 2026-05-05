# Audio System Architecture

Кастомная аудиосистема на базе **Wwise** и **Zenject** с поддержкой динамического окружения, пула звуковых объектов и асинхронной загрузки банков.
Изменения от 22.04.2026 (в разработке):

##  Основные модули

### AudioFacade & AudioService
- `IAudioFacade` – единая точка входа для воспроизведения звуков и управления параметрами Wwise.
- `AudioService` – низкоуровневая обёртка над `AkSoundEngine`, содержит кэшированный `AkAuxSendArray` для избежания аллокаций.

### AudioEntity (пул объектов)
- `AudioEntity` – MonoBehaviour с `AkGameObj`, управляемый через **Zenject MemoryPool**.
- Поддерживает `AudioRequest` (билдер) для гибкой настройки: событие, Aux‑шина, Switch, RTPC.
- Автоматически регистрируется/дерегистрируется в Wwise при спавне/деспавне.

### Система окружения

#### EnvironmentAudioComponent
- Компонент зоны с коллайдером‑триггером.
- Содержит `AudioAuxBusAsset` (ScriptableObject) и приоритет.
- Уведомляет `EnviromentResolver` объекта о входе/выходе.

#### EnviromentResolver
- Хранит список активных зон и выбирает шину с наивысшим приоритетом.
- Предоставляет `GetCurrentAuxBus()` / `GetCurrentAuxSendData()`.

#### PortalResolver
- Отвечает за нахождение в портале (`EnvironmentPortalComponent`).
- Вычисляет вес смешивания двух `AuxBus` вдоль оси портала.

#### EnvironmentPortalComponent
- Автоматически находит две пересекающиеся `EnvironmentAudioComponent`.
- Через `PortalResolver` применяет смешанные `AuxSend` для объектов внутри.

### Загрузка банков

- `BankLoaderService` – асинхронная загрузка/выгрузка банков с подсчётом ссылок.
- `SceneBankLoader` – компонент на сцене, загружает банки из `BankSceneMapping`.
- `WwiseEngineInitializer` – инициализация движка и загрузка `Init.bnk`.


## Нововведения

### 1. Динамические порталы с поддержкой UniRx
- Замена `Update` на реактивные подписки, обновление `AuxSend` только при изменении позиции.
- Автоматический выбор ближайшего портала при пересечении нескольких.

### 2. Единый API для окружения
- Методы `IAudioEnviromentMaker.GetCurrentAuxSendData()` возвращают структуру `AuxSendData`.
- Фасадные методы `PlayOneShot(..., environmentSource)` автоматически применяют текущее окружение.

### 3. Оптимизация через кэширование
- `AkAuxSendArray` переиспользуется внутри `AudioService`.
- `AudioRequest` не создаёт лишних объектов.

### 4. Визуализация и отладка
- `AudioEditorGizmos` – методы расширения для отрисовки зон и порталов в Scene View.
- `AudioEnvironmentDebugger` – компонент для отображения текущего окружения над объектом.

### 5. Заложены интерфейсы для будущих модулей
- `IRoomComponent`, `IRoomAware` – для `AkRoom`.
- `ISurfaceReflector` – для `AkSurfaceReflector`.


## Интеграция с Wwise

- Все `AuxBus` задаются через `AudioAuxBusAsset` (ScriptableObject).
- Для корректной работы окружения в Wwise Authoring у событий должна быть включена опция **Use game‑defined auxiliary sends**.
- Банки загружаются через `AkSoundEngine.LoadBank` с использованием `UniTask`.


## Пример использования

### Настройка зон и портала
1. Создать два объекта с `EnvironmentAudioComponent`, задать им разные `AuxBusAsset` и коллайдеры.
2. Создать объект с `EnvironmentPortalComponent` и коллайдером, пересекающим обе зоны.
3. Настроить ось (`Axis`) портала.

### Воспроизведение звука с окружением
```csharp
public class FootstepProcessor : IFootstepAudioProcessor
{
    private IAudioFacade _audioFacade;
    private IAudioEnviromentMaker _environmentMaker;

    public void PlayFootstep(AudioEventAsset asset, Vector3 position)
    {
        _audioFacade.PlayOneShot(asset, position, _environmentMaker)
            .WithSwitch(surfaceSwitch)
            .Play();
    }
} 
```

### Загрузка банков для сцены
1. Создать BankSceneMapping, добавить в него нужные WwiseBankAsset.
2. На SceneContext повесить SceneBankLoader и указать маппинг.

### Отладка
1. Включить AudioEnvironmentDebugger на персонаже – над ним будет отображаться текущая Aux‑шина или смесь.
2. В Scene View зоны отрисовываются зелёным, порталы – голубым с жёлтой осью.

### Производительность
1. Пул AudioEntity исключает аллокации при частом воспроизведении.
2. UniTask + CancellationToken для асинхронной загрузки банков без блокировки основного потока.
3. Реактивные подписки UniRx снижают нагрузку в порталах.

### Планы по расширению
1. Реализация AkRoom и AkRoomPortal с аналогичным резолвером.
2. Добавление AkSurfaceReflector для акустических отражений.
3. Инструмент для массовой валидации аудиокомпонентов в сцене.