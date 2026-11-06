# Отчёт о рефакторинге Aethel

**Дата:** 29.10.2026. **Проект:** D:\UnityPetProject\PetProject. **Ветка:** refactore/systems. **Исходный commit:** fa8698456e5113a04ee91c821daa9424bb579269.

## Результат

**Статус валидации:** Готово к ревью с ограничениями. Чистая Windows Development build прошла; release-проверка аудио и самостоятельный запуск player не выполнены.

Выполнен архитектурный проход рефакторинга: код распределён по ответственности и asmdef, устранены основные связи между меню, представлением, созданием мира и сохранениями. Сохранён текущий игровой цикл прототипа и совместимость профилей. Целевое развитие описано в текущем GDD: пошаговая RPG с отрядом из пяти героев. Подготовлен отдельный план перехода; реализация нового игрового цикла в этот рефакторинг не включена.

Это подготовка архитектуры к развитию. Алгоритмы AI, баланс боя, сторонние SDK и все legacy-классы не переписывались целиком.

| Показатель | До | После |
|---|---:|---:|
| C#-файлы в CodeBase | 494 | 514 |
| asmdef в CodeBase | 10 | 26 |
| Основные игровые runtime-сборки | Character + SaveStorage; остальной код в общей сборке | 14 |
| Проектный CodeBase в Assembly-CSharp / Editor | 249 / 8 | 0 / 0 |
| Неразрешённые ссылки / циклы asmdef | Были проблемные ссылки на predefined/Editor и отсутствующий GUID | 0 / 0 |

Рост числа файлов связан с выделением контрактов, специализированных фабрик, проверок и Editor-адаптеров. Сторонние скрипты вне CodeBase могут продолжать использовать predefined-сборки.

Подробная карта: [Architecture-Current.md](Architecture-Current.md).

## Что изменено

### Модули и DI

- Выделены Progress.Model, App.Contracts, AI, Input, Pooling, Assets, Progress.Runtime, Gameplay, UI, Application, Commerce, Composition. Character и SaveStorage сохранены.
- Editor, runtime и тесты имеют отдельные сборки; generated Input Actions подключён через asmref без переноса generated-файла.
- Исправлены ссылки DialogueSystemExtensions, Game.Utils и GoogleSheetConfig.Editor; Editor-код VFX выделен отдельно. Ссылки на asmdef используют GUID; SDK DLL остаются precompiled/auto-referenced.
- Удалены IServiceFactory/ServiceFactory. Runtime-классы получают явные зависимости. StateFactory остаётся адаптером Composition для состояний Zenject.

### Меню, атрибуты и прикладные сценарии

- CharacterMenuService управляет сценариями и переходами; CharacterMenuPresentation создаёт и закрывает экраны; CharacterSession восстанавливает профиль и ведёт подписки характеристик.
- Представления получают ICharacterMenuActions. HeroHealth зависит от ICharacterStatsSource, без зависимости от конкретного меню.
- Окружение сохранений и выход из приложения передаются через интерфейсы. SessionState и EditorApplication находятся в Editor-адаптерах.
- Каталог окон отделён от gameplay-конфигов; прогрев окон типизирован, reflection-регистрация убрана.

### Мир и фабрики

- GameWorldBuilder задаёт порядок создания и восстановления мира. Application использует IWorldBuilder и ILoadingScreen.
- HeroFactory, EnemyFactory и LootFactory отвечают за свои сущности; WorldAssetFactory объединяет создание Addressables-экземпляра и регистрацию прогресса. GameFactory остался фасадом сборки мира.
- Создание HUD перенесено в UIFactory. Gameplay не создаёт конкретные экраны меню.
- Добыча и спавнеры получают узкие ILootSpawnFactory/IEnemySpawnFactory, без зависимости от всей фабрики игры.
- Восстановление врагов завершается до WorldLoaded: добавлен IAsyncProgressReader и последовательный ApplyAsync.

### Сохранения

- ProgressRegistry отделён от фабрик. Он устраняет двойную регистрацию и обходит snapshot списка, поэтому участник может отписаться во время capture/apply.
- SaveLoadService захватывает прогресс через IProgressRegistry и использует ISaveEnvironment. UI работает с SaveSlotInfo/SaveSlotStatus.
- Формат CharacterSchemaVersion и файлового envelope не изменён; 3 слота, checksum, backup, архивирование и legacy import сохранены.

### Исправления жизненного цикла

- Pool отслеживает активные экземпляры; двойной возврат и возврат в чужой пул не дублируют свободные объекты. Bounded spawn переиспользует старейший активный экземпляр, expandable сохраняет остальные.
- Убрана неоднозначная перегрузка SpawnExpandableAsync; у методов спавна единый порядок parent → initializer.
- Исправлено создание лишнего контейнера пулов. Dispose очищает объекты; прогрев не вызывает игровые OnSpawned/OnDespawned до инициализации.
- Повторная инициализация/освобождение ввода защищены; кнопка открытия окна восстанавливает подписку после повторного включения. Базовое pooled-окно снимает собственный обработчик закрытия.
- Спавнер отписывается от смерти врага, не добавляет ID повторно и отсеивает устаревший асинхронный результат. LootSpawner завершает подписку при уничтожении.
- Возврат добычи в пул снимает регистрацию и останавливает coroutine. Подбор удаляет уже сохранённую запись добычи: она не появится снова после reload.
- Неуспешная загрузка ассета удаляется из кэша с освобождением handle; закрытый мир не принимает поздно созданный Addressables-экземпляр.

### Найденная проблема данных Level_2

В исходном Level_2.asset было 8 записей спавнеров, но только 4 уникальных: повторялись ID, тип и координаты. Это воспроизводилось как два врага на один save ID.

Добавлена LevelDataValidation: точные дубли объединяются с сохранением прежнего ID; один ID с разными координатами/типом вызывает явную ошибку. Эта же проверка используется при сборе конфига в Editor. Сам asset и scene-маркеры не переписаны; новая игра создаёт 4 уникальных спавнера. Старые save ID продолжают работать.

## Применение принципов

- **SRP:** сценарий, экран, профиль, реестр прогресса и создание сущностей имеют отдельных владельцев.
- **DIP/ISP:** зависимости проходят через порты мира, UI, окружения и узкие фабрики; конкретные реализации связывает Composition.
- **DRY:** общий путь создания/регистрации сущностей, единый реестр прогресса, одна обработка жизненного цикла пулов.
- **KISS:** сохранены Zenject/UniRx/UniTask и единый Gameplay-модуль. Универсальные event bus, ECS и собственный DI не добавлялись.

## Совместимость и сохранность

- Из 494 исходных скриптов сохранён GUID 491: 246 перенесены и 245 остались по прежнему пути.
- Удалены 3 неиспользуемых plain C# типа: IServiceFactory, ServiceFactory, IPoolableSpawnDespawn; ссылок на их GUID в prefab/scene/asset нет.
- Namespaces и сериализуемые поля существующих компонентов в основном сохранены. Публичные JSON-поля не переименованы. Массовая подмена Assembly-CSharp в YAML не проводилась.
- Проверены отсутствующие MonoBehaviour scripts во всех prefab под Assets/_project.
- Smoke использовал отдельный каталог Library/SaveStorageMigration/Smoke и тестовый ключ PlayerPrefs. Проверка Finish Smoke подтвердила неизменность основного Progress; исходное значение тестового ключа восстановлено.
- Исходные заметки Obsidian оставлены как описание состояния до рефакторинга. Новые документы отмечены актуальными.
- Удалены 65 опустевших папок прежней структуры вместе с их folder meta; GUID существующих скриптов сохранены.
- Изменения не закоммичены. Служебные изменения Plastic, SDK importers и JSON Performance Testing в рабочем дереве не откатывались; часть этих файлов появилась вне текущего продолжения работы.

## Проверки

| Проверка | Результат |
|---|---|
| Текущая Editor-компиляция | 0 ошибок, 0 предупреждений по MCP recompile после возобновления |
| Атрибуты и создание героя | 8/8 синхронных NUnit fixtures |
| Файловое хранилище | 14/14 fixtures |
| Пулы и реестр прогресса | 11/11 новых fixtures |
| Стабильность ID спавнеров | 3/3 новых fixtures |
| Игровой save adapter | 4/4 проверок round-trip, отсутствующей позиции, данных уровня/покупок, прерванного забега и отклонения некорректных данных |
| Editor runner в сумме | 38/38: 36 тестовых методов, агрегат адаптера и скан prefab |
| Граф asmdef | Нет циклов, неразрешённых ссылок, обратной зависимости на Composition или проектного CodeBase в predefined |
| Legacy import → продолжение | Пройдено через MCP |
| Backup recovery, сохранение, удаление слота | Пройдено через MCP |
| Отмена замены/создания, новый профиль в slot-3 | Пройдено через реальные Unity Button.onClick; slot-2 сохранён |
| Hero/NavMesh/характеристики | Level_2, класс 1, HP 50, урон 7 |
| 10 attribute open/close cycles: one view, no retained input blocks or progress readers | Пройдено |
| 10 открытий/закрытий pooled OptionWindow | Пройдено |
| Смерть врага, drop, подбор сохранённой добычи | Пройдено |
| Reload мира, убитый спавнер, сохранённый drop | Пройдено; один drop, прежняя ценность, без дубликатов читателей |
| Обычная Windows x64 Development | Succeeded; 2 ошибок, 13 предупреждений; 104 с. Не считается чистой при errors > 0 |
| Windows x64 Development с CleanBuildCache | Succeeded; 0 ошибок, 33 предупреждений; 386 с. Проверка пройдена |

Fixtures выполнены Editor-инструментом через вызов отмеченных NUnit-методов и их SetUp/TearDown; это не отчёт запуска Unity Test Runner. Создаваемые проверками объекты изолированы во временной сцене. Gameplay smoke проверяет зависимости и восстановление через реальные игровые компоненты; подбор вызывается через обработчик триггера, без имитации движения мышью.

## Player build: ошибки, предупреждения и ограничения

В обычной сборке были две ошибки PluginImporter → MonoScript. При CleanBuildCache они не повторились. Это результат повторной проверки, а не окончательное доказательство первопричины.

Для локализации дополнительно выполнен read-only аудит: 1430 YAML-ассетов и 8607 meta под Assets; дубликатов GUID и прямых ссылок m_Script с fileID 11500000 на PluginImporter не найдено. Аудит не заменяет AssetDatabase и не охватывает package GUID или YAML больше 12 MiB. [Результат](_audit/refactoring-serialized-plugin-references-2026-10-29.json).

На более ранней повторной сборке также возникали ошибки записи metadata Wwise x86 Profile; финальный результат каждой попытки нужно читать отдельно. Перед паузой один запуск был отменён — отмена не считается успешной проверкой.

Из ранее записанных предупреждений полной сборки и проверки исходных ссылок:

- Устаревшие API Unity Purchasing в существующих IAPProvider/IAPService.
- Неиспользуемые поля отладчиков и запроса AudioSystem.
- В Reload, Level_1, Level_3, Level_4 компонент Wwise InitBankHolder ссылается на отсутствующий script GUID ed39f2b237e573f4f85990bb5d1440ac; InitBank.asset — на отсутствующий WwiseInitBankReference 9289c9e765809ec408877f7c7b8d296d.
- В Level_2/Main Camera отсутствует SteamAudioListener GUID 6fd9ebf1401392e4bbd47fea32f47642. Это не перенесённые CameraFollow/CameraOcclusionFade: их GUID сохранены и компоненты работают в smoke.
- Эти три SDK GUID отсутствуют также в metadata исходного Git commit; они не удалялись текущим рефакторингом. [Проверка снимка](_audit/refactoring-known-sdk-references-2026-10-29.json).
- Resources-prefab интеграционных тестов Zenject с недоступным тестовым компонентом, native symbol upload, копирование банков в StreamingAssets и пустой каталог SoundBanks.

Инкрементальные отчёты могут содержать только новые предупреждения. Полные текущие сообщения обычной и clean-проверки сохранены отдельными JSON. Отсутствующие SDK-компоненты не удалялись автоматически. Перед выпуском необходимо восстановить/перенастроить Wwise/SteamAudio и проверить звук в player. Самостоятельный запуск собранного exe здесь не выполнялся.

**Артефакты:** Builds/RefactoringSmoke/PetProject.exe и, если clean-сборка создала файл, Builds/RefactoringCleanSmoke/PetProject.exe. Обязательны соседние *_Data, MonoBleedingEdge и UnityPlayer.dll.

## Ограничения результата

- Скорость компиляции до/после не замерялась одинаковым сценарием. Процент ускорения не заявляется. R0 зафиксировал структуру и GUID, но полного baseline player build до изменений нет.
- IL2CPP/AOT и Android здесь не проверены. Замена SDK, reflection baking Zenject и платформенные разрешения не входили в эту работу.
- Проверен полный сценарий Level_2 и повторная загрузка этого мира. Переходы по всем остальным уровням, качество боя/AI, диалоги и звучание Wwise требуют отдельного игрового прогона; создание соответствующих зависимостей в проверенном мире работает.
- В исходных заметках встречались примеры и утверждения, не совпадающие с кодом. Решения принимались по фактическим классам и runtime-проверкам.
- Некоторые legacy-имена, две WindowBase в разных namespaces и старое форматирование отдельных алгоритмов сохранены. Они не означают общую ответственность: окна Creation/Attributes и pooled Option/Shop имеют разные lifecycle. Массовое косметическое переименование остаётся отдельной задачей.

## Передача на review

1. Ознакомиться с картой модулей и разделением CharacterMenuService / CharacterMenuPresentation / CharacterSession.
2. Проверить фабрики, IProgressRegistry и сохранность prefab/scene в обычном игровом запуске.
3. Подтвердить обработку точных дублей Level_2 и оставшиеся границы проверок.
4. После review согласовать следующую задачу по текущему GDD: решения D0 и минимальное тактическое ядро D1. См. [план перехода](../Design/GDD-Migration-Plan.md).

Повторные проверки доступны в Unity: Tools → PetProject → Refactoring → Run Regression Checks / Build Windows Development; изолированные сценарии слотов — в Save Storage. Validate Gameplay Lifecycle запускается после подготовленного slot-3 из Slot UI Flow и перезапуска изолированного Play mode; это сценарий с определёнными предусловиями, не кнопка для пользовательского сохранения.

## Материалы

- [Текущая архитектура](Architecture-Current.md).
- [Исходный план](Asmdef-Refactoring-Plan.md).
- [Карта GUID и перемещений](_audit/refactoring-guid-map-2026-10-29.json).
- [Инвентаризация после переноса](_audit/asmdef-inventory-after-2026-10-29.json).
- [Результаты проверок](_audit/refactoring-validation-2026-10-29.json).
- [Проверка графа](_audit/refactoring-assembly-graph-2026-10-29.json).
- [Игровой smoke](_audit/refactoring-gameplay-smoke-2026-10-29.json).
- [Обычная Windows build](_audit/refactoring-player-build-2026-10-29.json).
- [Clean Windows build](_audit/refactoring-player-build-clean-2026-10-29.json).
- [Отменённая перед паузой проверка](_audit/refactoring-player-build-cancelled-before-resume-2026-10-29.json).
- [Editor compile после возобновления](_audit/refactoring-editor-compile-resume-2026-10-29.json).
- [План перехода к GDD](../Design/GDD-Migration-Plan.md).
- [Исходная версия GDD и её hash](../Design/GDD-Source-Reference.json).
