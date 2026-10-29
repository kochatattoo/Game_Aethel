# Отчёт D1 — тактическое ядро и технический полигон

**Дата:** 29.10.2026. **Статус:** этап подготовлен к review с указанными ограничениями.

## Реализовано

- Независимая PetProject.Tactics без Unity Engine references: квадратная сетка, стоимость пути, занятость, допустимый перепад высот, инициатива и очередь живых участников.
- Команды Move/Attack/EndTurn с проверками до изменения состояния, расходом ОД и однократным применением принятого command ID.
- Отдельный воспроизводимый RNG xorshift32-v1, базовая ближняя атака d20, попадание/промах, поражение и победа фракции.
- Неизменяемые определения и snapshots; одно приватное изменяемое состояние внутри Encounter. Анимация и кадры Unity не являются авторитетом правил.
- Изолированная сцена-полигон с двумя фигурами и Editor-окном управления обеими сторонами. Параметры прототипа задаются на компоненте, материалы/объекты имеют явного владельца.
- Автоматический Play Mode smoke с возвратом исходных сцен и проверкой сохранности данных.

## Границы изменения

Это первый технический этап перехода к GDD, а не полностью готовый тактический игровой режим. Профиль с шестью атрибутами, отряд из пяти героев, AI, игровой HUD, умения и кампания — дальнейшие этапы. Производственные состояния/сохранения текущей игры не переключались на Encounter.

Новая сцена не добавлялась в Build Settings. Пакеты и основная Input Actions asset не изменялись. Формат профиля v1, JSON-слоты и их миграция не менялись. Окончательные числа/криты/пространственные правила нужно уточнять в GD; значения D1 помечены прототипными.

## Ключевые файлы

| Путь от CodeBase | Назначение |
|---|---|
| Modules/Tactics/Runtime | C#-ядро, правила, сетка, команды и данные чтения |
| Modules/Tactics/Tests/Editor | 24 тестовых метода для правил, RNG и пути |
| Modules/Gameplay/TacticsSandbox | Unity-адаптер и владелец представлений |
| Editor/Tactics | Окно команд, безопасное создание сцены, runtime smoke |
| Editor/RefactoringValidation.cs | Новый набор подключён к общей регрессии |

В CodeBase теперь 28 asmdef и 532 C#-файла: добавлены 1 runtime-сборка и 1 Editor-тестовая сборка. Основных runtime-модулей стало 15, включая PetProject.Tactics. Это текущие числа; предыдущий отчёт рефакторинга фиксирует предыдущий этап с 26 asmdef.

## Проверка

| Проверка | Фактический результат |
|---|---|
| Native Unity Test Runner, EditMode, PetProject.Tactics.Tests | 24 passed, 0 failed, 0 skipped |
| Общий регрессионный runner | 62/62; 60 уникальных тестовых методов + агрегат 4 проверок адаптера и скан prefab |
| Изолированный runtime smoke | success=true, sceneRestored=true, saveIsolation=true |
| Граф asmdef | Пройдено; циклы 0, неразрешённые ссылки 0, CodeBase в predefined 0 |
| Domain boundary | noEngineReferences=true, references=[] |
| Windows x64 Development + CleanBuildCache | Succeeded, 0 ошибок, 33 предупреждений, 388 с. |

Runtime проверил: Standalone scene initializes two actor views without campaign services; Movement, view position, command retry and blocked destination are consistent; Action budget and explicit turn ending advance the round; d20 combat completes, declares a winner and hides the defeated actor; Ten resets preserve view count; idle frames do not advance simulation.

Количество новых тестовых методов подтверждено исходными fixtures и passCount/сообщением Native Test Runner. Сырое поле testCount в MCP-обёртке равно 32 и не использовано как число тестовых методов. Полный ответ сохранён в audit.

Полигон проверен функционально через реальные Unity-компоненты в Play Mode. Также визуально осмотрены модели и Editor-окно, выполнено настоящее нажатие «Переместить активного»: клетка героя (1,1) → (2,1), ОД 3 → 2, ревизия 0 → 1. Исправлена позиция плавающего окна для конфигураций с отключённым монитором. Проверка качества художественного оформления и самостоятельный запуск собранного player не заявляются. Windows build подтверждает компиляцию проекта с новым модулем; тестовый полигон в этом player не включён.

Предупреждения SDK/отсутствующие Wwise/SteamAudio-компоненты, обнаруженные при предыдущем этапе, не скрывались и не исправлялись удалением компонентов. Текущие сообщения сборки сохранены в отдельном артефакте; release-проверка звука остаётся открытой.

## Как посмотреть

Unity → Tools → PetProject → Tactics → Open Sandbox Scene → Play. Окно Aethel: тактика позволяет выбрать клетку, переместить активного, атаковать и завершить ход. После победы доступен сброс. Инструмент отказывается автоматически переключать несохранённые сцены.

Повторить проверки: Native Test Runner с фильтром PetProject.Tactics.Tests; Tools → PetProject → Tactics → Run Runtime Checks; общий runner Tools → PetProject → Refactoring → Run Regression Checks.

## Следующий этап

D2: согласовать создание шести атрибутов/кап базы и политику старых профилей; добавить versioned campaign DTO и адаптер конверсии. Старый CharacterProfileData.Restore всё ещё ожидает пять атрибутов — он намеренно не переписан молча. Затем D3: реальные пять участников, команды игрока/AI и переключение режимов исследования/тактики.

## Материалы

- [Контракт столкновения](Systems/GD-SYS-TAC-001.md).
- [Сетка и путь](Systems/GD-SYS-GRID-001.md).
- [Запуск полигона](Systems/GD-SYS-PROT-001.md).
- [План перехода](GDD-Migration-Plan.md).
- [Native Test Runner](_audit/tactics-d1-nunit-2026-10-29.json).
- [Runtime smoke](_audit/tactics-d1-runtime-2026-10-29.json).
- [Общая регрессия](_audit/tactics-d1-regression-2026-10-29.json).
- [Граф](_audit/tactics-d1-graph-2026-10-29.json).
- [Сборка](_audit/tactics-d1-player-build-2026-10-29.json).
- [Проверка реальной кнопки](_audit/tactics-d1-ui-check-2026-10-29.json).
