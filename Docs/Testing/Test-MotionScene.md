# Test_MotionScene

Проверочная сцена основного gameplay-контура, без загрузки кампании и пользовательских слотов. Используемые runtime-сборки входят в player build.
Сцена: `Assets/Scenes/TEST/Test_MotionScene.unity`. В основной список сцен сборки не добавлена.

## Запуск и управление

1. Сохранить текущую рабочую сцену.
2. Unity → **Tools → PetProject → Motion → Open Test Motion Scene**.
3. Нажать Play и щёлкнуть внутри **Game View**.

| Действие | Управление |
|---|---|
| Свободное движение / подтверждение клеточного пути | ЛКМ по допустимой поверхности |
| Выбор скелета / атака в бою | ЛКМ по скелету |
| Предпросмотр тактического пути | Навести мышь на клетку |
| Завершение хода | Кнопка в левой панели |
| Вращение камеры | ПКМ + движение мыши |
| Поворот на 45° | Q / E |
| Масштаб | Колесо мыши |
| Исходный ракурс | F |

В обычном режиме клик строит полный маршрут по NavMesh, показывает ломаную линию по всем углам пути и VFX точки назначения.
`NavigationPathView` использует переиспользуемый массив `GetCornersNonAlloc`: пройденные сегменты обрезаются, линия начинается у героя и сокращается до цели без ежекадровых аллокаций corners.
Используется `CFXR3 LightGlow A (Loop)`; без назначенного prefab создаётся примитивная точка. Линия и маркер исчезают после arrival или перехода в бой.

## Площадка и столкновения

- Габариты — **140×140** по X/Z, клетка — **1×1 unit**, 19 600 логических позиций.
- Открытая зона, кубы/сферы, тонкая стена, проходы разной ширины, пандусы и дорогие клетки.
- Обычное движение использует NavMeshAgent и полный `NavMeshPath`.
- Герой получает физический `CapsuleCollider` и kinematic `Rigidbody`.
- Тактический motor перед каждым шагом проверяет опору, NavMesh и capsule clearance. Прямого прохождения через препятствия между клетками нет.
- Сетка и preview используют общие mesh, не отдельные GameObject на клетку.

## Границы Core и Unity

- `PetProject.Encounter.Core`: переходы exploration/encounter, preview, состав группы и чистый AI planner.
- `PetProject.CharacterStats.Core`: атрибуты, модификаторы, производные и `DiceDefinition`.
- `PetProject.EnemyArchetypes.Core`: чистые определения архетипов, без prefab и Unity.
- `PetProject.Navigation.Core`: остаток маршрута, lifecycle и `ICharacterMovement`.
- `PetProject.Tactics.Grid.Core`: доска, клетки и поиск пути.
- `PetProject.Tactics`: бой, атаки, ОД, инициатива и lifecycle столкновения.

Все Core-сборки имеют `noEngineReferences=true`. Unity SO и MonoBehaviour выполняют только адаптацию данных,
ввод/вывод и представление. `EncounterController` создаёт `ICharacterMovement` и `INavigationLifecycle`; определения противников
предоставляет `EnemyArchetypeConfig`. Зависимости генерируются через `AssemblyGraph.asset` и `Build All`.

## Конфигурация

### `EncounterRulesConfig`

Путь: `Assets/Configs/Tests/Motion/EncounterRulesConfig.asset`.
Открытие: **Tools → PetProject → Motion → Combat Rules Settings**.

Содержит только общие правила столкновения:

- ОД на ход — 6; атака — 3 ОД.
- Радиус связывания группы — 12 units.
- Пауза перед действием противника — 0,75 с, минимум — 0,5 с.
- Шесть характеристик героя, производные HP, броня, инициатива, бонус атаки и фиксированный урон.

### `MeleeTacticsAIConfig`

Путь: `Assets/Configs/AI/MeleeTacticsAIConfig.asset`.
Открытие: **Tools → PetProject → AI → Melee Tactics Settings**.

Это только конфиг поведения AI: приоритеты атаки, сближения и завершения хода, лимит ОД на одно перемещение.

### `EnemyArchetypeConfig`

Путь: `Assets/Configs/Enemies/Archetypes`. Открытие обычного скелета:
**Tools → PetProject → Enemies → Skeleton Archetype Settings**; остальные активы выбираются в Project.

Каждый спавн ссылается на собственный архетип. Editor настраивает ранг, имя, prefab, AI, шесть характеристик,
характеристику атаки, HP, броню, инициативу и кубики. Созданы:

| Архетип | Ранг | Особенности |
|---|---|---|
| Скелет | Обычный | 1d4 урона |
| Скелет-лучник | Обычный | Атака от ловкости, 1d6 |
| Скелет-маг | Обычный | Атака от интеллекта, 1d8 |
| Скелет-капитан | Необычный | Усиленные HP/характеристики, 1d8+1 |
| Скелет-генерал | Босс | 30 базовых HP, 2d8+2 |

Лучник и маг пока используют общий ближний AI planner; архетипы подготавливают данные для следующих боевых действий.

Модификатор характеристики: `floor((значение - 10) / 2)`.
Производные скелета:

- HP = базовые HP + модификатор телосложения × коэффициент.
- Инициатива = модификатор ловкости.
- Бонус атаки = модификатор силы + бонус мастерства.
- Броня = базовая броня + бонус доспеха + модификатор ловкости.

## Группы противников

На сцене 6 скелетов в 3 группах:

| GroupId | Число | Архетипы и позиции |
|---|---|---|
| `skeleton-patrol` | 3 | Скелеты `(-6, 0, -18)`, `(0, 0, -20)`; лучник `(4, 0, -12)` |
| `skeleton-crypt` | 2 | Маг `(-22, 0, 12)`; капитан `(-16, 0, 14)` |
| `skeleton-general` | 1 | Генерал `(22, 0, 22)` |

При обнаружении героя или внезапной атаке в бой входят живые противники с тем же `GroupId` внутри `GroupLinkRadius`.
Противники вне радиуса и других групп не добавляются. При выборе `EncounterController` в Scene View показываются радиусы и связи;
Inspector сообщает число союзников в радиусе каждого спавна.

Перед каждым действием AI соблюдает `EnemyDecisionDelay`. Пауза не продвигает симуляцию боя и отменяется сменой presentation version.

## Атака и кубики

Для скелета по умолчанию:

1. Проверка попадания: `1d20 + модификатор силы + бонус мастерства > броня цели`.
2. При равенстве с бронёй — промах.
3. При попадании урон: `1d4 + DamageBonus`.
4. Нанесённый урон ограничивается оставшимися HP цели.

Количество граней атаки и урона задаётся в `EnemyArchetypeConfig`. Герой полигона пока использует фиксированный урон из общего конфига.
Каждая атака тратит ОД независимо от результата. Отклонённая команда не расходует ОД и не продвигает RNG.

## UI

- Левая панель **316×220**: режим, завершение хода, ОД, HP-полоса героя и текущее сообщение.
- Под ней панель характеристик героя: шесть атрибутов с модификаторами, HP, броня, атака, урон и инициатива.
- Над каждым живым скелетом — world-space Canvas с числовым HP и заполнением полосы. Цвет зависит от ранга; полоса поворачивается к камере и скрывается после смерти.
- Верхняя центральная панель показывает раунд и очередь. Завершившие ход перемещаются в конец и сереют до нового раунда.
- Правая нижняя панель содержит последние 6 записей истории. Выводятся маршруты, группы, паузы AI, перемещения,
  завершения ходов, проверки попадания, кубики урона, изменение HP, победа и поражение.

## Стартовые окна плагинов

Автопоказ ExplosiveLLC `Load Input and Tag Presets` отключён удалением его asset-postprocessor из `SetupInputLayers.cs`.
DOTween настроен штатно через **Tools → Demigiant → DOTween Utility Panel → Setup DOTween**;
`DOTweenUpgradeManager.dll` после этого удалён самим установщиком. Не возвращать эти автозапуски при обновлении плагинов без отдельного решения.

## Основные файлы

| Область | Решение |
|---|---|
| Общие правила | `EncounterRulesConfig`, `EncounterRulesConfigEditor` |
| Группы/Editor | `EnemySpawnDefinition`, `EncounterControllerEditor`, `EncounterDirector` |
| AI | `MeleeTacticsAIConfig`, `EnemyTurnPolicy` |
| Архетипы/характеристики | `EnemyArchetypeConfig`, `EnemyArchetypeConfigEditor` |
| Кубики | `ActorDefinition`, `MeleeAttackResolver`, `Encounter`, `TacticalEvent` |
| Маршрут/VFX | `NavigationPathView`, `EncounterController`, `MotionTestSceneTools` |
| Коллизии | `HeroMotor`, `GridPathMovement`, `SurfaceGridMap` |
| UI/история/HP | `EncounterHudView`, `WorldHealthBarView`, `MotionContracts`, `EncounterDirector` |

Пути кода указаны от `Assets/Code/Modules` или `Assets/Code/Editor` по смыслу таблицы.

## Проверки

- Unity Test Runner, EditMode: фильтр **PetProject.Tactics.Tests**.
- **Tools → PetProject → Motion → Run Skeleton Combat Checks**: реальные клики, вход группы, HUD, AI, смерть, reset и внезапная атака.
- **Tools → PetProject → Motion → Run Runtime Checks**: независимый запуск, ввод, HUD, NavMesh, тактические ОД, пандусы и 20 циклов смены режима. Результат: `Library/MotionTest/runtime-checks.json`.
- **Tools → PetProject → Encounter → Validate Runtime**: campaign-проверка создания героя, противников и команды движения на Level_2. Результат: `Library/Encounter/campaign-checks.json`.
- **Tools → PetProject → Encounter → Run Asset Checks**: отсутствие Missing Scripts в игровых prefab и сценах. Результат: `Library/Encounter/asset-checks.txt`.
- Результат проверки боя: `Library/MotionTest/skeleton-combat-checks.json`.
- Ошибки Unity Services/Token Exchange при текущем сетевом окружении относятся к внешним сервисам Unity и не входят в проверку полигона.

## Ограничения

Это полигон, не полная D&D-система: нет критических 1/20, спасбросков, укрытий, LOS, дальнего боя и заклинаний.
Пять атрибутов профиля кампании преобразуются в шесть боевых характеристик героя через адаптер персонажа.
Мир статичен; одна X/Z-позиция поддерживает одну поверхность. Runtime-сборки Encounter/Navigation/Hero/Enemies входят в player build и используются основными Level_1–4.
