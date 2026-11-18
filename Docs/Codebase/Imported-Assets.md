# Импортированные ассеты и плагины Aethel

Снимок проекта: Unity `6000.3.5f2`, URP `17.3.0`. Документ описывает фактически найденные файлы и пакеты, а не только зарегистрированные зависимости.

## Быстрая навигация

| Что искать | Корневой путь |
|---|---|
| 3D-модели, персонажи, оружие, props, окружение | `Assets/Import/3D/` |
| Анимации, Animator Controller, Avatar Mask | `Assets/Import/Animation/` |
| Готовые наборы уровней и подземелий | `Assets/Import/Levels/` |
| Музыка и звуковые эффекты | `Assets/Import/SFX/` |
| Иконки, GUI, spinner | `Assets/Import/UI/` |
| VFX и decals | `Assets/Import/VFX/` |
| Сторонний код и editor-инструменты | `Assets/Plugins/` |
| Embedded/local UPM-пакеты | `Packages/` |
| UPM-зависимости и версии | `Packages/manifest.json`, `Packages/packages-lock.json` |
| NuGet-зависимости | `Assets/packages.config`, `Assets/Packages/` |
| Wwise Unity Integration | `Assets/Wwise/`; исходный звуковой проект — `Wwise/` |
| Google Mobile Ads | `Assets/GoogleMobileAds/` |
| Игровые ассеты проекта | `Assets/Art/`, `Assets/Prefabs/`, `Assets/Configs/`, `Assets/Scenes/` |

Наличие ассета в `Assets/Import` не означает его использование в игровых сценах. Demo/Example — справочный контент, не игровая зависимость.

## Правила обращения

- Для сцены брать готовый `.prefab`; `.fbx`/`.dae` — исходная mesh и, часто, вложенные Animation Clip.
- Анимации: `.anim` — отдельный клип, `.controller` — Animator Controller, `.overridecontroller` — вариант контроллера, `.mask` — Avatar Mask.
- В имени Kevin Iglesias `[RM]` означает Root Motion. Обычный и `[RM]`-вариант не смешивать в одном состоянии без необходимости.
- Для retargeting гуманоидов использовать Rig `Humanoid`; перед этим проверить Avatar исходной модели.
- Проект использует URP. Файлы `Built-in`, `HDRP`, `SRP` и запасные `.unitypackage` не импортировать без проверки: возможны дубли скриптов, шейдеров и материалов.
- Не переименовывать и не перемещать файлы вне Unity: `.meta` хранит GUID, по которому префабы и сцены ссылаются на ассеты.
- Игровой контент подключать через Addressables. Звук — через `AudioSystem`/Wwise, VFX — через `VFXSystem`, а не прямым инстанцированием из gameplay-кода.
- Импортированные demo-скрипты попадают в общий `Assembly-CSharp`; при добавлении нового пака проверять глобальные имена классов и enum.

## Плагины в `Assets/Plugins`

### Используются в архитектуре проекта

| Путь | Плагин | Назначение |
|---|---|---|
| `Zenject/` | Zenject 9.2.0 | DI, installers, фабрики, внедрение зависимостей. |
| `UniRx/` | UniRx 6.2.2 | `ReactiveProperty`, `Observable`, reactive-подписки моделей и сервисов. |
| `Sirenix/` | Odin Inspector / Serializer | Инспекторы, атрибуты и сериализация. |
| `Pixel Crushers/` | Dialogue System | Диалоги, триггеры, UI; расширения проекта — `Assets/Code/_myPlugins/DialogueSystemExtensions_0.1/`. |

### Установленные инструменты и SDK

| Путь | Инструмент | Назначение / ограничение |
|---|---|---|
| `AdvancedPolygonCollider/` | Advanced Polygon Collider 1.0.6 | Контур `PolygonCollider2D` по Sprite; только 2D. |
| `AnimationBaker/` | Animation Baker 1.6.5 | Editor-запись retargeted Mecanim-анимаций в legacy `.anim`; руководство — `Docs/Animation_Baker_1.6.5_Manual.pdf`. |
| `Ash Assets/Control Rig/` | Control Rig | Построение control rig для Humanoid через `Tools/Control Rig Builder`; требует `Animator`. |
| `BK/Pure_VolumetricFog/` | Pure Volumetric Fog | Raymarched ground fog для Unity 6 / URP 17+, terrain heightmap, realtime shadows; требует Compute Shader и SM 4.5. Не для Built-in/HDRP. |
| `CloudsToy/` | CloudsToy Free 2.2.5.1 | Процедурные облака, пресеты и демо; для URP предусмотрен `URP Addon`. |
| `Demigiant/DOTween/` | DOTween | Tweening через `DG.Tweening`; настройка — `Tools/Demigiant/DOTween Utility Panel`. |
| `graphy/` | Graphy 4.0.0 | FPS, память, аудио и отладочный overlay; не игровой HUD. |
| `HomebrewIK/` | Homebrew IK 1.6 | Foot IK, сглаживание и коррекция высоты на неровной поверхности. |
| `Kartograph/` | Kartograph | Процедурная генерация 2D/3D-уровней из prefab-секций; ядро — `Assets/Plugins/Kartograph/Packages/Kartograph-core.dll`. |
| `Kevin Iglesias/IKHelperTool/` | IK Helper Tool | Редактирование IK-поз и анимаций; руководство — `IKHelperTool Documentation.pdf`. |
| `KinematicCharacterController/` | Kinematic Character Controller | Кинематическое движение, moving platforms, `PhysicsMover`; не смешивать неявно с `NavMeshAgent`. |
| `Krearthur/GOPainter/` | GOPainter 1.4 | Editor-кисть объектов, surface snap, randomizer, physics simulation; `Krearthur/Utils/` — зависимость. |
| `KrisDevelopment/EnviroSpawn_CS/` | EnviroSpawn | Массовый scatter префабов по raycast: random/grid/uniform, слои, нормали. |
| `nTools/PrefabPainter/` | PrefabPainter 2.9 | Editor-кисть префабов, multibrush, grid, slope filter, pivot editor. |
| `RealStep-Dynamic_Foot_Placement/` | RealStep | Dynamic Foot Placement: runtime foot IK и editor-настройка; demo отделена от runtime. |
| `RedBjorn/ProtoTiles/` | ProtoTiles | Tile map, сетки, клетки и вспомогательные runtime-инструменты. |
| `RedBjorn/Utils/` | RedBjorn Utils | Общие утилиты RedBjorn; не самостоятельная игровая система. |
| `Rowlan/Terrain/Stamps/` | Ultimate StampIT Collection | Heightmap-штампы для Terrain/MicroVerse; данные, не runtime-плагин. |
| `Simple Spin Blur/` | Simple Spin Blur | Визуальный motion/spin blur для вращающихся mesh; URP/HDRP-варианты лежат в `RP Assets/*.unitypackage`. |
| `Android/`, `iOS/` | Native dependencies | Android/iOS native-зависимости рекламы; не перемещать, фильтры импорта привязаны к путям. |

`GOPainter`, `PrefabPainter`, `EnviroSpawn` частично дублируют размещение объектов. Выбирать по задаче: ручная кисть, пресеты или массовый scatter. `HomebrewIK` и `RealStep` оба относятся к постановке стоп; перед совместным применением нужен отдельный выбор интеграции.

## Дополнительные SDK и пакеты

### Отдельные интеграции в `Assets`

| Путь | Версия | Назначение |
|---|---|---|
| `Assets/Wwise/` | Integration 2025.1.5.4090, SDK 2025.1.5 Build 9095 | Аудио-движок, banks, events, switches, RTPC. Gameplay использует обёртку `Assets/Code/_myPlugins/AudioSystem_0.8/`. |
| `Assets/GoogleMobileAds/` | 10.6.0 | Google Mobile Ads; Android/iOS SDK и UMP-зависимости. |
| `Assets/ExternalDependencyManager/` | 1.2.186 по changelog Google Mobile Ads | Разрешение Android/iOS-зависимостей. |
| `Assets/TextMesh Pro/` | Поставка Unity | TMP text, шрифты, examples/extras. |
| `Assets/NuGet/`, `Assets/Packages/` | См. `Assets/packages.config` | NuGetForUnity и DLL: Google Sheets API, `System.Text.Json`, ZLinq и др. |

### Embedded/local UPM

| Пакет / путь | Версия | Назначение |
|---|---|---|
| `Packages/com.kybernetik.animancer/` | 8.4.1 | Animancer: проигрывание и управление анимациями из кода без обязательного Animator Graph. |
| `Packages/com.unity.cinemachine-3.1.5/` | 3.1.5 | Cinemachine: игровые камеры, follow/orbit, blending, impulse. |
| `Packages/com.jbooth.microverse/` | 1.7.0 | Генерация и редактирование terrain-сцен. |
| `Packages/com.jbooth.microverse.vegetation/` | 1.7.0 | Растительность на Terrain. |
| `Packages/com.jbooth.microverse.splines/` | 1.7.0 | Carving и texturing Terrain по сплайнам. |
| `Packages/com.jbooth.microverse.objects/` | 1.7.0 | Размещение GameObject в Terrain. |
| `Packages/com.jbooth.microverse.ambiance/` | 1.7.0 | Ambient sound в Terrain. |
| `Packages/com.jbooth.microverse.demo/` | 1.7.0 | Демо MicroVerse, не gameplay-зависимость. |
| `Packages/com.jbooth.microsplat.core/` | 3.9.0 | MicroSplat Terrain Shading, базовый модуль. |
| `Packages/bz-soft.ragdoll-helper/` | 1.6.0 | Ragdoll Helper. |
| `Packages/com.gamelovers.mcp-unity/` | 1.5.0 | MCP Unity Server: чтение редактора, консоль, тесты, editor-операции. |
| `com.cysharp.unitask` | Git-зависимость | UniTask, async/await для Unity. |
| `com.cysharp.zlinq` | Local `ZLinq-1.5.5` | ZLinq Unity; NuGet `ZLinq`/`ZLinq.Json` — 1.5.6. |

### Основные Unity UPM-пакеты

Актуальные версии — в `Packages/manifest.json` и `Packages/packages-lock.json`.

| Пакет | Версия | Назначение |
|---|---|---|
| URP / Shader Graph | 17.3.0 | Рендеринг и шейдеры проекта. |
| Addressables | 2.8.0 | Адресуемая загрузка контента. |
| Input System | 1.17.0 | Ввод, actions, control schemes. |
| AI Navigation | 2.0.9 | NavMesh и навигация. |
| Animation Rigging | 1.4.0 | Runtime rigs и constraints. |
| Timeline | 1.8.10 | Cutscenes и Playable sequences. |
| Terrain Tools | 5.3.3 | Terrain editing. |
| Test Framework | 1.6.0 | EditMode/PlayMode NUnit-тесты. |
| uGUI | 2.0.0 | Unity UI. |
| Visual Scripting | 1.9.9 | Visual Scripting; наличие пакета не требует использовать его в архитектуре. |
| Postprocessing | 3.5.1 | Post-processing stack; для URP проверять актуальный Volume workflow. |
| Profile Analyzer | 1.2.4 | Анализ Profiler-кадров. |
| Unity Ads / Analytics / Purchasing | 4.16.4 / 3.8.2 / 5.0.3 | Реклама, аналитика, покупки. |

## 3D-ассеты — `Assets/Import/3D`

| Пакет / путь | Содержимое и назначение |
|---|---|
| `AA_UniversalCharacter/` | Модульный low-poly персонаж: meshes, prefabs, materials; `Scripts/HowToUse - Modules Shaker.pdf` — инструкция к Modules Shaker. |
| `Axe Warrior/` | Персонаж воина с топором, модель, префабы, базовая анимация и controller. |
| `Blink/Art/Characters/LowPoly/` | Low-poly Human Male, броня и готовый character prefab. |
| `Blink/Art/Weapons/LowPoly/` | Free RPG Weapons и Free Swords: топоры, луки, булавы, щиты, копья, посохи, жезлы, мечи; варианты Basic/Medium/Epic. |
| `BrokenVector/LowPolyBrickHouses/` | Модульные low-poly дома и Model Customizer. |
| `BrokenVector/LowPolyCliffPack/` | Скалы и утёсы. |
| `BrokenVector/LowPolyDungeon/` | Dungeon-модули, props, demo scene. |
| `BrokenVector/LowPolyFencePack/` | Заборы, ворота и двери; demo door scripts. |
| `BrokenVector/LowPolyRockPack/` | Камни и скальные формации. |
| `BrokenVector/LowPolyShaders/` | Шейдеры пакета; `Readme.pdf` — настройка. |
| `Decimate/Grid Master/` | Grid Master: 25 моделей и prefabs, Shader Graph, URP-материалы; `Docs/Documentation.pdf`. |
| `DeepNest/Devil_Bulldog_Lite/` | Модель монстра Devil Bulldog и controller. |
| `Free medieval weapons/` | Средневековое оружие: 4 FBX, 6 prefabs, PBR-текстуры; `README.txt`. |
| `Hatogame_new/BossMonsterPack1/` | Набор low-poly boss-монстров, модели, prefabs, controllers. |
| `Hovl Studio/Magic sword/` | Magic Sword: mesh, эффектные материалы/текстуры, prefabs и demo. |
| `HQP Studios/Low Poly 3D Icons - Pack Lite/` | 154 low-poly 3D-иконки и prefabs; документация в `Document/`, URP-конвертация в `URP/`. |
| `Inguz Media Studio/The Beauty Medieval War Banners and PROPS/` | Средневековые знамёна и props, 40 FBX, 70 prefabs, demo scenes. |
| `KatsBits-fantasy-base/` | Большой fantasy-набор: 124 FBX, 346 prefabs, materials/textures/scenes; `Docs/KatsBits_Fantasy_ReadMe.txt`. |
| `Low Poly Fruits/` | Фрукты: 10 prefabs, общая FBX и demo tester. |
| `Low Poly Pack - Environment Lite/` | Low-poly окружение: 43 модели, 44 prefabs. |
| `Low Polygon Potions Pack/` | 50 prefabs зелий, общая mesh и материалы; `Readme.txt`. |
| `Low-Poly Weapons/` | 16 моделей и prefabs оружия; документация `Documentation/ReadMe.odt`. |
| `LowPoly Environment Pack/` | Природное окружение: 31 FBX/prefab, Timeline-демо. |
| `Lowpoly_Crates/` | 15 вариантов ящиков, meshes и materials; готовых prefabs нет. |
| `LowpolyBakersHouse/` | Дом пекаря, 2 модели/prefabs, textures; `Readme.txt` и запасной `.unitypackage`. |
| `LowPolyMedievalPropsLite/` | 48 средневековых props/prefabs; содержит URP/HDRP-пакеты конвертации. |
| `Mini Legion Footman PBR HP Polyart/` | Footman: модель, варианты prefabs, animations/controller, PBR-текстуры. |
| `Mini Legion Lich PBR HP Polyart/` | Lich: модель, prefabs, animations/controller, PBR-текстуры. |
| `Mini Legion Rock Golem PBR HP Polyart/` | Rock Golem: модель, prefabs, 7 `.anim`, controller, PBR-текстуры. |
| `nnj3de_CartoonFantasyWeaponMinipack/` | 6 cartoon fantasy weapons, materials/textures; `README.txt`. |
| `Pandazole_Lowpoly_Asset_Bundle/Pandazole City Town Pack/` | Городские здания и окружение. |
| `Pandazole_Lowpoly_Asset_Bundle/Pandazole Farm Ranch Pack/` | Ферма, ranch, постройки и props. |
| `Pandazole_Lowpoly_Asset_Bundle/Pandazole Home Interior/` | Интерьеры и мебель. |
| `Pandazole_Lowpoly_Asset_Bundle/Pandazole Kitchen Food/` | Кухня, еда и посуда. |
| `Pandazole_Lowpoly_Asset_Bundle/Pandazole Nature Environment Pack/` | Природа и environment props. |
| `Pandazole_Lowpoly_Asset_Bundle/Pandazole Survival Crafting Pack/` | Survival/crafting tools и props. |
| `polyperfect/Poly Halloween/` | Halloween characters/props/environment; `README_PolyHalloween.pdf`, запасные pipeline-пакеты. |
| `Polytope Studio/Lowpoly_Characters/` | Модульные персонажи, броня, priest; документация `PT_Lowpoly_Modular_Armors_Doc.pdf`, `PT_Lowpoly_Priest_doc.pdf`. |
| `Polytope Studio/Lowpoly_Environments/`, `Lowpoly_Village/` | Окружение и деревня. |
| `Polytope Studio/Lowpoly_Props/` | Props и предметы окружения. |
| `Polytope Studio/Lowpoly_Weapons/` | Мечи, топоры, луки, стрелы, щиты, копья, посохи. |
| `Pure Poly/Free Low Poly Nature Pack/` | Деревья, камни, растения, Terrain Layers; `Read Me.pdf`. |
| `PurePoly/Free Fantasy RPG Weapons/` | Fantasy RPG weapons; `Read Me.pdf`. |
| `PurePoly/Mining_Free_Assets/` | Mining props/resources; `Read Me.pdf`. |
| `RPG Food & Drinks Pack/` | 15 моделей/prefabs еды и напитков, materials/textures; demo scripts не нужны для использования моделей. |
| `RPGHero/` | RPG hero: meshes/prefabs, 9 `.anim`, controller, SRP-варианты. |
| `RPGPP_LT/` | RPG Poly Pack Lite: 90 environment models/prefabs, materials/textures. |
| `SazenGames/Skeleton/` | Skeleton character, controller и 18 override controllers. |
| `SimpleNaturePack/` | 24 модели/prefabs природы, materials/textures; pipeline-конвертеры в `.unitypackage`. |
| `StylizedStoneUI_Free/` | Stylized Stone UI: 5 моделей/prefabs, materials; `ReadMe.txt`. |
| `Training_dummy/` | Тренировочный манекен: mesh/prefab, texture и controller. |
| `TreasureChestMaker/` | Конструктор сундуков, материалы и префабы; `TCM/Documentation/ReadMe.txt`. |
| `URP GanzSe Free Modular Character Pack/` | URP modular character: 217 meshes/prefabs; `Documentation/GanzSe FREE Low Poly Modular Character - README.pdf`. |
| `WizardPolyArt/` | Wizard Poly Art: модель, prefabs, animations/controller, SRP/shader-файлы. |

## Анимации — `Assets/Import/Animation`

### Основные наборы

| Путь | Содержимое / назначение |
|---|---|
| `Blink/Art/Animations/Animations_Starter_Pack/Combat/` | Базовые боевые клипы. |
| `Blink/Art/Animations/Animations_Starter_Pack/Gathering/` | Gathering/работа с ресурсами. |
| `Blink/Art/Animations/Animations_Starter_Pack/Movement/` | Базовое передвижение. |
| `DoubleL/FBX Unity/` | RPG Animations Pack: FBX для Unity. |
| `DoubleL/FBX Unreal/` | Исходники для Unreal; в Unity-проекте не использовать для игровых контроллеров. |
| `DoubleL/Demo/` | 65 `.anim`, 66 controllers, demo scene; документация — `RPG_Animations_Pack Document.pdf.pdf`. |
| `ExplosiveLLC/` | RPG Character, Crafting, Warrior packs, demo controllers и SuperCharacterController. Детали ниже. |
| `Kevin Iglesias/` | Human, Dwarf, Skeleton, Humanoid Giant; Humanoid clips, masks, controllers. Детали ниже. |
| `TransformAnimation SxB_V2/` | Transform-анимации, модель, Avatar Mask, controller и demo scene. |

### ExplosiveLLC

| Путь | Назначение |
|---|---|
| `RPG Character Mecanim Animation Pack FREE/Animations/Unarmed/` | Unarmed combat/movement. |
| `RPG Character Mecanim Animation Pack FREE/Animations/2Hand-Sword/` | Двуручный меч. |
| `RPG Character Mecanim Animation Pack FREE/Code/` | Demo character controllers, actions, weapon/IK helpers; не игровая архитектура Aethel. |
| `Crafting Mecanim Animation Pack FREE/Animations/` | Crafting/перенос предметов. |
| `Warrior Pack Bundle 1 FREE/` | Brute, Karate, Ninja. |
| `Warrior Pack Bundle 2 FREE/` | 2 Handed, Archer, Knight, Mage. |
| `Warrior Pack Bundle 3 FREE/` | Crossbow, Hammer, Spearman, Swordsman. |
| `SuperCharacterController/` | Физический demo controller; документация — `Documentation/SuperCharacterControllerV2_README.pdf`. Не подключать к герою без отдельного решения. |

### Kevin Iglesias

| Путь | Назначение |
|---|---|
| `Human Animations/Animations/Male/`, `Female/` | Базовое движение, walk/run/sprint, strafe, turn, jump, idles, combat, work, social. |
| `Human Animations/Animations/*/Combat/Bow/` | Archer/bow combat. |
| `Human Animations/Animations/*/Combat/Spellcasting/` | Spellcasting. |
| `Human Animations/Animations/*/Combat/Thrown/` | Throwing. |
| `Human Animations/Animations/*/Work/` | Crafting/work. |
| `Human Animations/Animations/Masked Poses/` | Позы рук, хваты, книга; использовать с Avatar Mask. |
| `Human Animations/Models/Avatar Masks/` | Full/Upper Body, Head, Arms, Hands; для смешивания слоёв. |
| `Dwarf Animations/` | Dwarf Basic Motions и Dwarf Melee; PDF-руководства в корне пакета. |
| `Skeleton Animations/` | Skeleton movement/combat, models и animations. |
| `Characters/Humanoid Giant/` | Giant idle/action, unarmed attack, damage, walk/run; есть обычные и `[RM]`-варианты. |

PDF-руководства Human Animations: Basic Motions 2.4, Melee 2.0, Archer 2.0, Crafting 2.2, Spellcasting 2.0, Throwing 2.0 — в `Human Animations/*.pdf`. ZIP с Blender-файлами — исходники, для Unity не обязательны.

## Уровни — `Assets/Import/Levels`

| Путь | Содержимое / назначение |
|---|---|
| `Dungeon/` | Большой dungeon-набор: 395 FBX, 788 prefabs; `URP/` — нужные URP-материалы/варианты, `Build-In/` — legacy, `Animations_and_particle/` — сопутствующие эффекты. |
| `Dungeon2/raw/` | Второй dungeon-набор: 18 FBX, 19 prefabs, textures/materials. |
| `ithappy/Inferno_World_Free/` | Inferno World Free: 76 FBX, 83 prefabs, demo scenes и Shader Graph. |
| `LowPolyDungeonsLite/` | Low Poly Dungeons Lite: 17 моделей, 19 prefabs; pipeline-пакеты в корне. |

## Аудио — `Assets/Import/SFX`

| Путь | Содержимое / назначение |
|---|---|
| `Action RPG Music 1.6/` | 77 актуальных WAV-треков; `Outdated tracks/` — старые версии; список — `Action RPG Music FREE.pdf`. |
| `Battle Music Pack DEMO/` | 5 боевых музыкальных WAV. |
| `Energy Hard Rock Pack DEMO/` | 10 hard-rock WAV. |
| `Multiple Solutions/Basic RPG Sounds/` | 200 базовых RPG SFX. |
| `SFX Packs/General RPG SFX Pack/` | 32 общих RPG SFX. |

Для игры импортировать/настраивать через Wwise и `AudioSystem`; не вызывать Wwise напрямую из gameplay.

## UI — `Assets/Import/UI`

| Путь | Содержимое / назначение |
|---|---|
| `Free_Icons/Icons/` | 51 PNG-иконка. |
| `GUI CARTOON/TEXTURE/` | 138 PNG для cartoon GUI. |
| `GUI CARTOON/FONTS/`, `PSD/`, `SOUND/`, `ANIMATION/` | Шрифт, исходники PSD, звук и demo-анимация GUI. |
| `GUI_Parts/Gui_parts/`, `Icons/` | 44 PNG: элементы интерфейса и иконки. |
| `SimpleSpinner/Spinners/` | 19 PNG spinner-вариантов, `SimpleSpinner.cs`; `Readme.txt`. |

## VFX — `Assets/Import/VFX`

| Путь | Содержимое / назначение |
|---|---|
| `Decal/` | 3 decals: PNG и materials. |
| `JMO Assets/Cartoon FX Remaster/` | Cartoon FX Remaster: 64 prefabs, particles/materials/textures, CFXR shaders и scripts. Подключение в игре — через `VFXSystem`. |

## Исправление ошибок импорта ExplosiveLLC

Исправлено 18.11.2026:

- `SuperCharacterController.cs`: класс сделан `partial`; удалены повторные определения `IgnoredCollider`, `SuperGround`, `CollisionSphere`, `SuperCollision`, оставлены отдельные одноимённые файлы; удалён недопустимый `[SerializeField]` у `Ground`.
- `RPGCharacterController.cs`: `IKHands` подключён точечным alias из `WarriorAnimsFREE`, без импорта всего конфликтующего namespace.
- `IKHands.cs`: добавлены совместимые с RPG API `isUsed` и `BlendIK(..., Weapon)`.
- `CrafterControllerFREE.cs`: глобальный `CharacterState` переименован в `CrafterCharacterState`, чтобы не перекрывать `RPGCharacterAnims.Lookups.CharacterState`.

Проверка: повторная компиляция `Assembly-CSharp` компилятором Unity прошла без ошибок. Остались 13 неблокирующих предупреждений CS0618:

- `Low Poly Fruits/resources/fruits_tester.cs` — 10 вызовов устаревшего `Transform.RotateAround` в demo.
- `Low Polygon Potions Pack/Other Assets/Scripts/CreatePrefabFromSelected.cs` — 3 устаревших editor API создания/замены prefab.

При повторном импорте ExplosiveLLC эти локальные исправления могут быть перезаписаны. Сначала сравнить версии файлов, затем переносить исправления осознанно.

## Обновление справочника

После импорта/удаления пакета:

1. Добавить или убрать строку в нужной категории, указать путь и назначение.
2. Проверить README/PDF, pipeline-варианты, наличие demo scripts и `.unitypackage`.
3. Для анимаций указать Rig, Root Motion и Avatar Masks, если они есть.
4. Проверить компиляцию и консоль Unity.
5. Не считать импорт интеграцией: фактические ссылки из сцен, префабов и `Assets/Code` проверять отдельно.
