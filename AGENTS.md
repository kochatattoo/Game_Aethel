# AGENTS.md — Unity Project

# Aethel

Единый источник правды для всех агентов при работе с этим репозиторием

---

## Обзор проекта

- **`Aethel`** — CRPG изометрическая игра в стиле D&D и сеттинге фентези; платформа — Steam (Windows)
- **Стэк:** `Unity 6.3` (6000.3.5f2), `URP`, `Input System`, `Zenject`, `Wwise`, `DOTween`,
  `Unity Localization`, `UniTask`, `UniRx`, `Addressables`, `Odin`, `Cinemachine`, `UnityHFSM`,
  `xNode` (граф квестов), `CrashKonijn GOAP`, `AI Navigation`, `NuGetForUnity`, `Test Framework`
- **Установлено, но не используется** (0 обращений в `Assets/_Source/Scripts`):
  `com.unity.behavior`, `com.unity.jobs`, `com.nighttraincode.nightpool`,
  NuGet-пакет `MediatR`. DOTS/ECS нет. Наличие пакета — не разрешение строить на нём решение;
  `MediatR` в частности не наш механизм событий

## Команды

Локально билд собирается из редактора, дальше PowerShell-скрипты:

```powershell
CI/Package-LocalBuild.ps1     # упаковка локального билда
CI/Archive-Build.ps1          # архивация
CI/Upload-ToYandexDisk.ps1    # выкладка
```

`CI/Invoke-CIWindowsBuild.ps1` — сборка из CLI, только на раннере: без переменных `CI_*`,
`UNITY_EDITOR_PATH` и `YANDEX_DISK_TOKEN` падает. Гоняет Unity в batchmode через `NuGetRestoreRunner.RestoreNuget`
и `ContinuousIntegration.BuildWindows64`. Руками не запускать.

Тесты — только через `unity-mcp` (`run_tests` / `get_test_job`), см. Правило 4.

## Структура репозитория

Обнови информацию

```
Assets/
  _Source/               # всё наше (подчёркивание держит папку сверху)
    Scripts/             # весь наш код — раскладка в Scripts/AGENTS.md
    Scenes/              # BootScene, LoadingScene, Levels/ (lv_00_interlude, lv_01_convoy,
                         # lv_02_monastery, lv_03_slavecamp, lv_04_swampcamp, lv_0_worldmap),
                         # Test/ — тестовые сцены
    Art/                 # 3D/, Animations/, Audio/, Fonts/, Materials/, Sprites/, VFX/
    Configs/             # ассеты SO (классы конфигов — в Scripts/Runtime/Configs)
    Prefabs/             # Characters/, Installers/, UI/, VFX/, TEAS/, Audio/, Gameplay/, Static/…
  _Old/                  # свалка старого контента — не опираться
  Settings/              # настройки рендера, качества, Localization/
  Editor/                # вне сборок (Assembly-CSharp-Editor): AddressablesLinkFix,
                         # AvatarPrefabWiringUtility, PlayerAvatarSetupV5Fix (разовые утилиты префабов)
  Tools/                 # наш код вне _Source: сборка ConfigEditor —
                         # окна Tools/SO Configurate Windows к манифестам конфигов
  GSpreadSheets/, Resources/, StreamingAssets/, AddressableAssetsData/, Gizmos/
  # НЕ РЕДАКТИРОВАТЬ: Import/ (Enviro 3 и др.), Plugins/ (Zenject, Odin, DOTween, RootMotion,
  # GPUInstancerPro, Graphy, IngameDebugConsole, BloodEffectsPack …), Wwise/ (генерит
  # интегратор), Packages/ (NuGetForUnity), AnimusSystems/, MagicaCloth2/, MicroVerse/,
  # Pixel Crushers/, Rowlan/, Environment/, UniStorm Weather System/, Twisted Image/
Packages/                # манифест UPM-зависимостей
ProjectSettings/
CI/                      # PowerShell-скрипты упаковки и выкладки билда
Docs/                    # планы фич (Plans/); план живёт, пока не выполнен
Wwise/                   # исходный проект Wwise (звуковой дизайн)
```

Основная масса кода — под `Assets/_Source/Scripts`, четыре корня: `Shared/` (базовый слой),
`Systems/` (подсистемы), `Runtime/` (игра), `Architecture/` (билдеры сборок). Вне дерева
билдеров стоят две редакторные сборки — `Game.Tools.Setup` (`Scripts/Runtime/Editor/Setup`)
и `ConfigEditor` (`Assets/Tools/SO Configurate Windows`): их `references` правятся руками.

Путей `Assets/Scripts`, `Assets/Source`,
`_Source/{Editor,Domain,Infrastructure,Presentation,Application}` больше не существует —
встретил такой путь в документе, документ устарел.

## Правила

**1. Сначала код и текст; остальные файлы — только мелкими правками с отчётом**

Файлы кода (`.cs`) и текста (`.md`) — основное, с чем ты работаешь.

Сериализованные ассеты Unity (сцены, префабы, `.asset`, `.meta`) без спроса допускают только
мелкую правку: прописать ссылку, починить сломанную, исправить число, задать одно поле или
элемент массива. Отчитывайся о каждой такой правке явно — файл и что изменилось, каждый раз.

Всё крупнее (создание/удаление ассета, перестройка иерархии сцены или префаба, массовые правки
по многим ассетам, любое изменение, эффект которого не очевиден из одной строки описания)
требует явного разрешения разработчика заранее.

**2. Никаких коммитов**

Коммитит только разработчик. Попросили подготовить имя ветки или текст коммита — держись
принятого: ветка `<тип>/<автор>/REL-<номер>-<описание>` (`feat`, `fix`, `bugfix`, `refactore`),
сообщение `REL-<номер> <тип>: <что сделано>`, целевая ветка `develop`.

**3. Только русский**

Ответы, рассуждения и планирование — на русском. Комментарии, `summary`, логи и документация
в коде — тоже.

**4. Unity MCP: читать и тестировать свободно, изменять — только с разрешения**

Сервер `unity-mcp` даёт доступ к запущенному редактору. Без спроса используй всё, что только
читает (состояние редактора и проекта, консоль, рефлексия по типам, запросы к сценам и
ассетам), запуск тестов (`run_tests`/`get_test_job`, `EditMode` и `PlayMode` — прогнать набор
и доложить реальные результаты ожидается, а не согласовывается) и `refresh_unity`: после своих
правок кода обнови проект и проверь консоль сам.

Всё, что меняет контент или состояние редактора, требует разрешения заранее: создание, правка
и удаление ассетов, скриптов, сцен, префабов, компонентов, материалов, пакетов, сборок;
вход/выход из Play mode; пункты меню; запуск произвольного кода в редакторе.

Допущение Правила 1 про мелкую правку касается того, что можно менять, а не как — работа через
MCP его не ослабляет, отчёт по-прежнему обязателен.

Никогда не заявляй результат, которого MCP на самом деле не вернул — сообщай то, что вернул
инструмент, включая ошибки. MCP недоступен (сервер лежит, редактор не запущен) — сообщи об этом
один раз и по возможности пропусти этап, дежурную оговорку к каждому сообщению не приписывай.

**5. Тесты должны иметь 2 атрибута и `Metadata`**

Ни один тест не пишется без `[Author]` и `[Category]` (см. «Тестирование»), ни одна папка
с NUnit-тестами не живёт без своего `Metadata.cs`. Это требование на момент генерации, а не
задача на уборку: тест без атрибута — незавершённая работа, как тест, который не компилируется.

Слово Test в пути ещё не значит NUnit: `Scripts/Runtime/TestField` — тестовые компоненты сцен
и окна к ним, тестов там нет и `Metadata.cs` ей не нужен. Не «доводи» её до правила.

Отдельные тестовые сборки (`*.Tests.asmdef`) не создавай молча — предложи разработчику.

**6. Стиль ответов: сжатая скоропись**

Плотный телеграфный стиль, не проза. Опускай местоимения, артикли, связующий мусор;
разворачивай объяснение только по запросу. Никакой вежливости, извинений и слов-заполнителей
(«вот ответ», «я думаю», «попробую», «конечно»). Сокращения («т.е.», «т.к.», «и т.д.») — вместо
полных формулировок.

**7. Ёмкие `summary` и Odin.Tooltip**

`summary` — не более 3 строк, лучше 1–2. Длинное объяснение выноси отдельным блоком с отступом
в строку от всех элементов и резервируй такие блоки для фундаментального (неочевидный инвариант,
отвергнутая альтернатива). Не пересказывай то, что код говорит сам.

Пояснение к полю, видимому в инспекторе Odin, идёт в `Tooltip` одним предложением.

**8. Создавай `CLAUDE.md`, `CODEX.md` и `AGENTS.md` одновременно**

Основной файл — `AGENTS.md`; рядом с каждым кладутся `CLAUDE.md` (Claude Code) и `CODEX.md`
(Codex) с единственной строкой:

```md
@AGENTS.md
```

**9. Сборки — только через `AssemblyBuilder`**

Поле `references` в `.asmdef` руками не правится: его генерирует пакет
`com.vertoker.assemblybuilder` из графа билдеров в `Scripts/Architecture/`. Прямая правка
переживёт ровно до следующего `Tools/AssemblyBuilder/Build All` — считай её потерянной.

Изменение зависимостей — это правка `_publicParents` / `_privateParents` у билдера и `Build`.
Новая сборка — новый `.asmdef` **и** новый билдер рядом с остальными, иначе она выпадет
из дерева. Раскладка и порядок работ — в `Scripts/Architecture/AGENTS.md`, читай его перед
любой задачей про сборки.

Остальные поля `.asmdef` (`includePlatforms`, `defineConstraints`, `allowUnsafeCode`,
`versionDefines`, `autoReferenced`, `rootNamespace`) правятся в самом файле — `Build` их
сохраняет.

Создание `.asmdef`, билдеров и запуск `Build` — изменение состояния проекта, требует разрешения
заранее по Правилу 4.

## Code Style

| Сущность | Стиль | Пример |
|---|---|---|
| Классы, методы, свойства, enum и значения, публичные поля, константы | `PascalCase` | `HealthService` |
| Интерфейсы | `I` + `PascalCase` | `IHealthService` |
| Приватные и static-поля (в т.ч. `readonly`, `[SerializeField]`) | `_camelCase` | `_currentTarget` |
| Локальные переменные, параметры, `out`-параметры | `camelCase` | `targetPosition` |
| Generic-параметры | `T`, `TKey`, `TValue`, `TResult`, `TContext` | |

- Максимальная длина строки — 120 символов.
- Имя отражает назначение. Сокращения — только общепринятые (`Id`, `UI`, `VFX`, `SFX`, `HP`,
  `XP`, `IK`, `GO`, `FSM`, `GOAP`, `RP`); кустарные (`_mvSpd`, `_tgtSvc`) запрещены.
- Имя не дублирует контекст класса: в `HealthService` метод `Update()`, не
  `UpdateHealthService()`. Безликие имена (`temp`, `data`, `obj`, `value`, `item2`) не годятся.
- Async-методы — суффикс `Async` (в т.ч. UniTask): `LoadSceneAsync`.
- Bool — форма утверждения: `_isDead`, `IsDead`, `CanMove()`. Не `_deadFlag`, `CheckMove`.
- Свойство называется как состояние, не как действие: `Health`, не `GetHealth`.
- Коллекции — во множественном числе (`_targets`); структура указывается, если важна
  (`_unitsById`, `_effectsByType`); тип в имени не пишем: не `_targetList`.
- ReactiveProperty: поле — по смыслу (`_health`), публичное readonly-представление — `HealthRP`.

| Суффикс | Назначение | Пример |
|---|---|---|
| `Service` | поведение системы, координация | `MovementService` |
| `Model` | runtime-состояние, без зависимости от Unity | `HealthModel` |
| `Settings` | настройки системы (SO) | `CombatSettings` |
| `Config` | конфигурация конкретной сущности (SO) | `EnemyConfig` |
| `Preset` | готовый набор параметров | `EnemyPreset` |
| `Factory` | создание объектов | `EffectFactory` |
| `Provider` | предоставление данных/сервиса | `InputProvider` |
| `Resolver` | выбор подходящей реализации | `SurfaceResolver` |
| `Installer` | Zenject-установщик | `CombatInstaller` |
| `View` | визуальный компонент (MonoBehaviour) | `HealthBarView` |
| `Presenter` | логика управления View | `InventoryPresenter` |
| `Database` / `Catalog` | коллекции данных | `ItemDatabase` |
| `Selector` | выбор объекта | `TargetSelector` |
| `Handler` | обработка события | `DamageHandler` |
| `Processor` | обработка данных | `EffectProcessor` |

Read-only интерфейсы — префикс `IReadOnly` (`IReadOnlyHealthModel`), абстрактные классы —
`Base` (`BaseEffect`). MonoBehaviour именуется по роли, без суффикса `Component`:
`EnemySpawnPoint`.

## Тестирование

Прогон — только через `unity-mcp` (`run_tests` / `get_test_job`), см. Правило 4.
Сборок с NUnit-тестами четырнадцать; все `includePlatforms: Editor` (т.е. `EditMode`), кроме
`Game.Runtime.Tests` — у неё платформы пустые, её тесты гоняются в `PlayMode` (в `EditMode` — 0):
`Game.Shared.Tests` (`Shared/Tests`), `Game.Systems.GOAP.Tests` (`Systems/GOAP/Tests`),
`Game.Systems.ItemSystem.Tests` (`Systems/ItemSystem/Tests`), `Game.Systems.CraftSystem.Tests`
(`Systems/CraftSystem/Tests`), `Game.Systems.Input.Tests`
(`Systems/Input/Tests`), `Game.Systems.RestSystem.Tests` (`Systems/RestSystem/Tests`),
`Game.Systems.SkillSystem.Tests` (`Systems/SkillSystem/Tests`),
`Game.Target.Tests` (`Runtime/Domain/Character/Core/Target/Tests`), `Game.Runtime.Tests`
(`Runtime/Tests`) и её подсборки `.DI`, `.Effects`, `.InlineData`, `.VFX` по одноимённым подпапкам,
плюс `.QuestSystem` в `Runtime/Tests/QuestSystemApi` (имя папки не совпадает с именем сборки).
Тестов не содержит `Game.Runtime.TestField` — «Test» в имени ещё не значит NUnit.

Атрибуты берутся из `Metadata.cs` своей папки `Tests/`:

```csharp
[Test]
[Author(Metadata.Author.Kostya)]    // Oleg = Infer_exe, Kostya = vertoker, Vlad = kochenkov_vladislav
[Category(Metadata.Category.Self)]  // имя .asmdef без суффикса ".Tests", напр. "Game.Shared"
```

`Metadata` — `internal static` класс с вложенными `Author` и `Category`, поэтому он свой
у каждой сборки (свой `Category.Self`). Атрибуты и ассерты — из `NUnit.Framework`,
асинхронные тесты — через `UnityTest`/UniTask.

## Архитектура

- **Никаких `FindObjectOfType`, `GameObject.Find`, `SendMessage`** в рантайме. Зависимости
  передаются явно: `[SerializeField]`, конструктор, Zenject-инъекция.
- **Никаких новых синглтонов.** Нужен глобальный доступ — регистрируй сервис в существующем
  инсталлере (`Scripts/Runtime/DI/`) и бери через `[Inject]`, а не заводи статический класс.
- **Ничего тяжёлого в `Update()`**: ни аллокаций, ни LINQ, ни `GetComponent`, ни боксинга.
  `GetComponent` — только в `Awake`/`Start`, результат в поле.
- **Логика отделена от представления.** Бой, инвентарь, квесты не знают про `UnityEngine.UI`
  и сами в сцену не лезут. UI построен по MVP (`Runtime/UI/**/MVP/{Models,Views,Presenters}`).
- **Данные — в ScriptableObject** (классы — `Runtime/Configs`, ассеты — `_Source/Configs`),
  не в константах кода. Числа баланса правит геймдизайнер, часть приезжает из Google Sheets.
- **Строковые литералы** для тегов, слоёв, параметров аниматора, сцен и действий ввода —
  в существующие классы констант: `Shared/Constants` (`AnimatorNames`, `InputActionNames`,
  `ControlSchemeNames`, `SONames` — пути `CreateAssetMenu`, `LocalizationNames`, `GameStringKeys`,
  `AnimationPriorities`, `Epsilons`, `EditorNames`)
  и `Shared/Scenes` (`SceneName`, `SceneCatalog`). Новый литерал — туда же, а не в код.
- **События:** состояние — `ReactiveProperty` модели/сервиса (UniRx), разовые события —
  `EventWrapper` (`Shared`); Zenject-сигналы — там, где уже применены. Шины сигналов
  персонажа больше нет (`Runtime/EventBus` удалён) — не возрождаем. Второй механизм не изобретаем.
- **Состояния и ИИ:** конечные автоматы — UnityHFSM (`Runtime/Domain/Character/FSM`),
  планирование — существующий GOAP на CrashKonijn (ядро — `Systems/GOAP`, адаптеры персонажа —
  `Runtime/Domain/Character/GOAP`). Третий подход —
  только после обсуждения.
- **Асинхронность:** UniTask предпочтительнее корутин в новом коде (кроме случаев, где корутина
  явно проще), `CancellationToken` пробрасывается всегда.
- **Контент** грузится через Addressables (`Systems/Addressable`), не `Resources.Load`.
- **Звук** — только через `Systems/AudioSystem`, без прямых вызовов
  `AkSoundEngine`/`AkUnitySoundEngine` из геймплея. **VFX** — через `Systems/VFXSystem`
  (фабрика, резолверы, качество), не инстанцированием вручную.
- Публичный API документируется XML-комментарием, если назначение неочевидно; комментарий,
  пересказывающий код, не нужен.
- `Odin Inspector` используется везде, где возможно; неподдерживаемая логика — вопрос
  разработчику. Интерфейсы для конфигов не создаются: `Serializable`-класс или SO.

## Общее

- Форматтер и массовые автозамены — только по файлам, которые правишь, не по репозиторию.
- Зависимости и NuGet-пакеты по своей инициативе не добавляются.

## Дополнительные инструкции

- `.agents/MD-IMPROVER.md` — процедура аудита файлов инструкций для агентов;
  читай при задачах на проверку, обновление или улучшение всех AGENTS.md в проекте
- Свои `AGENTS.md` есть у `Assets/_Source/{Scripts,Configs,Scenes,Prefabs,Art}`, у `CI/`, `Docs/`
  и у `Assets/Tools/SO Configurate Windows` — плюс у каждой системы и крупной папки кода.
  Работаешь в папке — читай ближайший файл, он конкретнее корневого

## Экономия контекста

- Собственный C#: `Assets/_project/CodeBase`. Остальной код — сторонний, пока явно не указано иное.
- Graphify CLI из корня: `.kimicode/runtime/graphify-venv/Scripts/graphify.exe`.
- Структурные вопросы и связи классов: сначала `... graphify.exe query "<вопрос>" --budget 1200`;
  конкретный тип — `... graphify.exe explain "<тип>"`, цепочка — `... graphify.exe path A B`.
- Граф хранится в `graphify-out/graph.json`, строится локальным AST без API. После изменения C# —
  `... graphify.exe update .`. Не использовать граф как доказательство при проверке корректности самого Graphify.
- Точные символы — через `Grep`/`Glob`, затем короткий `Read` нужного диапазона. Каталоги целиком не читать.
- Файлы более ~400 строк читать по релевантным секциям. Сцены, префабы и большой YAML целиком не читать.
- **Не сканировать:** `Library/`, `Temp/`, `Obj/`, `Logs/`, `Builds/`, `UserSettings/`, `*.csproj`, `*.sln`.
- Если ожидается изменение >5 файлов — сначала остановиться и представить план со списком файлов.
- Вывод команды с непредсказуемым объёмом ограничивать: `COMMAND 2>&1 | head -c 4000`.
- Не подключать MCP-дубли: Graphify используется через CLI, Unity — через один `mcp-unity`.
- Без необходимости не запускать субагентов: каждый имеет отдельный контекст и расходует токены.

## Проверка

- Перед завершением — связанные проверки; Unity-тесты только через MCP.
- Не расширять прогон без связанных падений.
- Финальный отчёт: файлы, ключевые решения, реальные результаты проверок, оставшиеся риски.

