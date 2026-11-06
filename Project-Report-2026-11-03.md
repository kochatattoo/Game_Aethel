# Отчёт по проекту PetProject (Aethel)

**Дата анализа:** 03.11.2026
**Путь:** `D:\UnityPetProject\PetProject`
**Ветка git:** `refactore/systems` (158 коммитов, последний — 30.10.2026, «work on tactical logic for move»)

---

## 1. Общий обзор

| Параметр | Значение |
|---|---|
| Версия Unity | **6000.3.5f2** (Unity 6.3) |
| Рендер-пайплайн | **URP 17.3.0**, Linear color space |
| Система ввода | **Input System 1.17.0** (только новая, legacy отключён) |
| Целевая платформа | Windows Standalone (собранный билд ~1 ГБ, D3D12) + Android (min SDK 25, ARM64) |
| Собственный код | **561 файл .cs**, ~28 000 строк в `Assets/_project` |
| Сборок asmdef | **28** (15 основных runtime, 4 библиотеки, 4 Editor, 5 тестовых) |
| Сцены | 9 собственных + ~60 из пакетов/демо |
| Тесты | **75 NUnit-тестов** ([Test]), архитектурные тесты, smoke-проверки |
| Размер `Assets` | 2,2 ГБ |

Жанрово проект развивается как **RPG с тактической боевой системой**: есть герой, враги, добыча, прогресс, меню персонажа, диалоги, и активно разрабатываемое независимое тактическое ядро (сетка, команды, инициатива).

---

## 2. Технологический стек

### Ядро и код
- **Zenject** — DI-контейнер, Composition Root (`BootstrapInstaller`, фабрики мира/героя/врагов)
- **UniTask** — асинхронность без аллокаций
- **UniRx** — реактивные потоки
- **ZLinq 1.5.5** — zero-alloc LINQ (локальный пакет, подключён как auto-referenced DLL)
- **Odin Inspector** (Sirenix) — расширенные инспекторы

### Рендер и контент
- **URP 17.3.0**, Post Processing 3.5.1
- **Cinemachine 3.1.5** — камеры
- **MicroVerse + MicroSplat** (JBooth) — террейн и вегетация
- **CFXR** — VFX-эффекты
- **Addressables 2.8.0** — адресация контента (идёт миграция с Resources: папки `Resources` и `Resources_moved` существуют параллельно)
- **TextMesh Pro**, uGUI

### Аудио
- **Wwise** (полная интеграция + исходники плагина в корне)
- **Steam Audio** — пространственный звук
- Собственная `Game.Infrastructure.AudioSystem` поверх Wwise

### Геймплей и сервисы
- **AI Navigation 2.0.9** — NavMesh
- Собственный AI-модуль: **Blackboard + Behaviour Tree** + Arbiter-логика
- **Pixel Crushers Dialogue System** — диалоги (+ свои расширения `DialogueSystemExtensions`)
- **Timeline**, Visual Scripting (установлен)
- **Unity Test Framework 1.6**, MCP-Unity (управление Editor через агента)

### Монетизация (мобильное направление)
- **Unity Ads 4.16.4**, **Google Mobile Ads**, **Yandex Mobile Ads**
- **Unity IAP 5.0.3**

---

## 3. Архитектура

Проект прошёл масштабный рефакторинг (29–30.10.2026) и организован по принципам **чистой/слоистой архитектуры** с направленным графом зависимостей asmdef:

```
Composition ──► Application ──► App.Contracts ──► Progress.Model ──► Character
     │  ├──► UI ────────┘              ▲
     │  ├──► Gameplay ──┤──► Tactics   │
     │  │               ├──► AI        │
     │  │               ├──► Input     │
     │  │               ├──► Pooling   │
     │  │               └──► Assets    │
     │  └──► Progress.Runtime ──► SaveStorage
     └──► Commerce
```

Ключевые свойства:

- **Обратных ссылок на Composition нет** — зависимости направлены вниз, runtime не ссылается на Editor-сборки и `Assembly-CSharp`.
- **`PetProject.Tactics` — чистое C#-ядро** (`noEngineReferences=true`): сетка (`GridBoard`, `GridPathfinder`), команды (`TacticalCommand`, `CommandResult`), детерминированный RNG, правила столкновений. Тестируется без Unity.
- **Порты и адаптеры**: `App.Contracts` объявляет порты (прогресс, мир, интеграции), `Commerce`, `Progress.Runtime`, `UI` — адаптеры.
- **Сохранения**: `SaveStorage` — файловые слоты с checksum, backup и архивированием.
- Известное ограничение (задокументировано): старые namespace могут не совпадать с новым расположением файлов — осознанная плата за поэтапную миграцию.

Распределение кода по модулям:

| Модуль | Файлов | Строк |
|---|---|---|
| Gameplay (герой, враги, мир, полигоны) | 90 | ~4 330 |
| AI (BT, blackboard, стратегии) | 44 | ~1 350 |
| Character (доменная модель) | 24 | ~620 |
| SaveStorage | 3 | ~315 |
| Остальные (UI, Application, Composition, Tactics, Commerce, Editor…) | ~400 | ~21 400 |

---

## 4. Сцены и контент

**Игровые сцены** (в Build Settings включена только `Bootstrap`):
- `Bootstrap.unity` — точка входа
- `Level_1`–`Level_4` — игровые уровни
- `Reload.unity`
- `Prototypes/TacticsSandbox.unity` — технический полигон тактики
- `TEST/TEST_DialogueUI.unity`, `TEST/Test_MotionScene.unity` — тестовые полигоны

**Контент** (`Assets/Import`, 2,2 ГБ всего): 3D-модели, анимации, уровни, SFX, UI, VFX; 2 террейн-ассета в корне Assets; `StreamingAssets`, Addressables-группы.

**Билды**: в `Builds/` лежат два smoke-билда после рефакторинга (`RefactoringSmoke`, `RefactoringCleanSmoke`, ~1 ГБ, Windows/D3D12).

---

## 5. Тестирование и CI

- **75 юнит-тестов** (NUnit `[Test]`), покрывают Tactics, Character, SaveStorage, архитектуру
- Отдельные тестовые сборки: `PetProject.Architecture.Tests`, `Character.Tests`, `SaveStorage.Tests`, `Tactics.Tests`, `TestSupport`
- **Архитектурные тесты** — контроль границ зависимостей asmdef
- Editor-инструменты проверок: `MotionEditorChecks`, `MotionRuntimeChecks`, `TacticsRuntimeSmoke`, `RefactoringValidation`
- **CI**: `.github/workflows/Build.yml`
- Утилиты в `Tools/`: `check-tactics.mjs`, `export-system-code.mjs`

---

## 6. Документация

Документация необычно хорошо развита для пет-проекта:

- `Docs/Architecture/` — актуальная карта архитектуры, отчёт рефакторинга, аудиты (JSON-снимки сборок, компиляции, билдов)
- `Docs/Codebase/` — полный справочник систем с исходниками (`Aethel-Systems-Codebase.md`)
- `Docs/Design/` — GDD-воркфлоу, шаблоны системных записей, контракты систем (GRID, PROT, TAC), отчёт по этапу Tactics D1
- `Docs/Testing/` — протоколы тестовых сцен

---

## 7. Сильные стороны

1. **Зрелая архитектура**: слоистые asmdef, Composition Root, порты/адаптеры, отсутствие обратных зависимостей — редкий уровень дисциплины.
2. **Движоко-независимое тактическое ядро** с детерминированным RNG — легко тестировать и переносить.
3. **Культура проверки**: архитектурные тесты, smoke-билды, аудит-JSON на каждый этап рефакторинга.
4. **Документация синхронизирована с кодом** и датирована.
5. Современный стек: Unity 6.3, URP 17, Input System, UniTask/ZLinq (внимание к аллокациям).

---

## 8. Замечания и риски

| # | Наблюдение | Рекомендация |
|---|---|---|
| 1 | `companyName: DefaultCompany`, `productName: PetProject` — не настроены | Задать реальные имя компании и продукта в Project Settings |
| 2 | Мусор в корне репозитория: `*.csproj`/`*.sln` (генерируемые), `ComputeCommandBuffer.cs` и др. .cs-файлы, `logRunSetup.txt` (500 КБ), `WwiseUnityIntegration_*_Src.zip` (21 МБ), `obj/`, `ZLinq-1.5.5/` | Проверить `.gitignore`, вынести исходники плагинов из корня |
| 3 | Параллельно существуют `Resources` и `Resources_moved` — миграция на Addressables не завершена | Завершить миграцию и удалить дубли |
| 4 | **Три рекламных SDK одновременно** (Unity Ads + Google Mobile Ads + Yandex Mobile Ads) | Оставить одну сеть или обосновать медиацию — лишний вес билда и поддержка |
| 5 | В Build Settings включена только `Bootstrap` — уровни грузятся через Addressables/SceneLoader | Проверить, что Addressables-группы уровней собираются в билд |
| 6 | 2,2 ГБ `Assets` при смешении собственного контента, демо-пакетов (MicroVerse Demo) и 60+ чужих сцен | Удалить неиспользуемые демо-ассеты |
| 7 | 75 тестов на ~28 К строк — покрытие умеренное; нет `[UnityTest]` (playmode) | Добавить playmode-тесты на геймплейный цикл |
| 8 | Настройки Android заполнены (min SDK 25, ARM64), но целевой билд — Windows | Определить приоритет платформы, чтобы не тянуть мобильные SDK в ПК-билд |
| 9 | Пакеты `com.jbooth.*` и `com.unity.cinemachine-3.1.5` лежат embedded в `Packages/` | Зафиксировать причину (локальные правки?) или вернуться к registry-версиям |
| 10 | `Ignore.conf` и `.plastic` — Plastic SCM рядом с git | Убедиться, что используется одна VCS |

---

## 9. Итог

Проект находится в зрелом состоянии активной разработки: после крупного рефакторинга архитектуры (октябрь 2026) кодовая база структурирована, задокументирована и частично покрыта тестами. Текущий фокус — тактическая боевая система (этап D1 завершён, ведётся работа над движением; подключение полного отряда запланировано на D2/D3). Главные технические долги — незавершённая миграция Resources → Addressables, избыток рекламных SDK и неубранный корень репозитория.
