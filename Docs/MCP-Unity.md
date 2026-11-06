# MCP Unity — PetProject

Установлено 28.10.2026 из `D:/UnityPlugins/Инфраструктурные/mcp-unity-1.5.0`.

## Установка в проекте

- Embedded UPM package: `Packages/com.gamelovers.mcp-unity`, версия `1.5.0`.
- Manifest: `com.gamelovers.mcp-unity = file:com.gamelovers.mcp-unity`.
- Node MCP server: `Packages/com.gamelovers.mcp-unity/Server~/build/index.js`.
- Unity bridge: `ws://localhost:8091/McpUnity`, автозапуск включён.
- Unity settings: `ProjectSettings/McpUnitySettings.json`.
- Project-scoped Codex config: `.codex/config.toml`, сервер `mcp-unity-petproject`.
- В этой установке bridge слушает IPv6 loopback; `UNITY_HOST='[::1]'` закреплён в проектном env.
- Глобальная конфигурация Codex не изменялась. Проект уже отмечен доверенным.

Пакет работает только в Editor. Удалённые подключения, запуск в batch mode и установка других Unity-пакетов через MCP выключены. Токен создаёт Unity в `Library/McpUnity/bridge-token`; значение не хранится в конфигурации и не коммитится.

## Запуск

1. Открыть PetProject в Unity 6000.3.5f2 и дождаться компиляции.
2. При необходимости проверить `Tools > MCP Unity > Server Window`.
3. В Codex переподключить MCP-серверы после изменения конфигурации. Текущая сессия может сохранять прежний каталог инструментов до переподключения.

При переносе проекта на другую машину обновить абсолютные пути `command`, `cwd` и env в `.codex/config.toml`, а также `NpmExecutablePath` в Unity settings. Эта конфигурация подготовлена для текущей рабочей копии Windows.

## Восстановление Node-зависимостей

Из корня проекта в PowerShell:

```powershell
Push-Location 'Packages/com.gamelovers.mcp-unity/Server~'
try {
    npm.cmd ci --ignore-scripts --no-audit --no-fund
    npm.cmd run build
} finally {
    Pop-Location
}
```

Использованы Node.js `24.18.0` и npm `11.16.0`. `node_modules`, `build` и токен игнорируются Git; исходники и lockfile npm входят в пакет.

## Проверено при установке

- Unity Package Manager зарегистрировал embedded package и зависимость Editor Coroutines.
- `npm ci` и TypeScript build завершились успешно.
- `codex mcp get mcp-unity-petproject --json` распознал проектную запись.
- MCP SDK client выполнил initialize, resources/list, tools/list, чтение `unity://scenes_hierarchy`, `get_scene_info` и `get_console_logs`.
- Обнаружено 34 инструмента. Активная сцена: `Assets/_project/Scenes/Bootstrap.unity`, 3 корневых объекта, без несохранённых изменений.
- Пробный запрос ошибок Console вернул `[]`. Это результат конкретного запроса, а не утверждение об отсутствии всех ошибок проекта.
- Play mode, тесты игры и player build не запускались. Сцены и игровой код установка не редактировала.

Локальный журнал проверки: `Library/McpUnity/verification-result.json`; проверочный клиент: `Library/McpUnity/verify-mcp.mjs`. Оба временные и игнорируются Git.

При переносе CharacterMenu соединение повторно проверено через MCP SDK: выполнялись компиляция, EditMode-тесты, чтение Console, управление PlayMode и вызовы редакторских команд. В текущем чате Unity-инструменты не появились в нативном каталоге автоматически, поэтому использовался SDK-клиент `Library/McpUnity/mcp-client.mjs`. При импорте ассетов и перезагрузке домена соединение временно прерывается; перед повтором изменяющей команды нужно проверить фактическое состояние Unity. Результаты игрового сценария описаны в CharacterMenu-Migration.md.

## Отключение

Выключить `AutoStartServer` в окне MCP Unity и удалить/отключить запись `mcp-unity-petproject` в проектном config.toml. Для удаления пакета использовать Package Manager; сначала сохранить нужные локальные изменения в embedded package.
