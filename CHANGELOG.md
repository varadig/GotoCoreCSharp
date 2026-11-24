# Changelog

## [2.0.0] - 2026-11-24

Rewritten as a Unity package (`com.varadig.gotocore`).

### Added
- Typed `MessageBus` (`IPublisher` / `ISubscriber`) replacing string-named notifications.
- `ICommand<TMessage>` and `CommandMap` replacing named services with parameter dictionaries.
- `ObservableProperty<T>` for models, `ControllerBase`, and `ViewBase` for MonoBehaviour views.
- `IAppContext` with typed `ContextKey<T>`, replacing the `CoreContext` singleton.
- `ILog` with sinks (`UnityLogSink`, `FileLogSink`, `MemoryLogSink`, `ConsoleLogSink`).
- `IFileStorage` with atomic writes and root-confined paths.
- Optional VContainer integration: `RegisterGotoCore`, `RegisterCommand`, `RegisterController`, `GotoCoreLifetimeScope`.
- Counter sample and edit-mode tests.

### Removed
- Global singletons, the string-keyed service container, the XNA/MonoGame `CoreGame` class, UDP socket helpers and the bundled Newtonsoft.Json DLLs. The 1.x code is available under the `legacy-2017` tag.
