# AssemblyBuilder архитектура

- `.asmdef` не редактируются вручную в поле `references`. Зависимости задаются через
  `AssemblyBuilder` в этом каталоге и применяются `Tools/AssemblyBuilder/Build All`.
- Один builder владеет одним `.asmdef`. Внешние сборки подключаются readonly-builder.
- Существующие сборки мигрированы в режиме `Inherit`: он сохраняет только исходные прямые
  зависимости. Новые Core-сборки используют явные public/private parents по назначению API.
- `PublicParents` передаются зависимым сборкам; `PrivateParents` нужны только самой сборке.
- Core не зависит от Unity, UI, MonoBehaviour, ScriptableObject и конкретных Unity-моторов.
- Unity-адаптеры и компоновщики зависят от Core API, а Core никогда не зависит от компоновщиков.
- После добавления сборки builder включается в `AssemblyGraph.asset`; затем выполняется
  `Build All`, компиляция и связанные тесты.
