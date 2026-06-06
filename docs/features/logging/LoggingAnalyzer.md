# Aiel Logging Analyzers — Developer Reference

> **Version:** 2.0 · **Target SDK:** .NET 10 · **Roslyn:** netstandard2.0

---

## Table of Contents

1. [Overview](#overview)
2. [Rules at a Glance](#rules-at-a-glance)
3. [Rule Details](#rule-details)
4. [Configuration — Custom EventIds Enum](#configuration--custom-eventids-enum)
5. [Code Fixes](#code-fixes)
6. [Severity Overrides](#severity-overrides)
7. [Running Locally](#running-locally)
8. [Extending the Analyzers](#extending-the-analyzers)

---

## Overview

The `Aiel.Logging.Analyzers` NuGet package ships a set of Roslyn source analyzers that
enforce the Aiel structured-logging convention at compile time.  Every rule has a matching
code fix so violations can be corrected with a single IDE action.

### Logging pattern recap

```csharp
// ✅ Fully compliant Aiel logging method
[LoggerMessage(
    EventId = (int)AielEventIds.ServiceStart,   // AIEL001: must use enum cast
    Level   = LogLevel.Information,
    Message = "[{EventId}] Service started")]    // AIEL003: placeholder required
public static partial void ServiceStarted(
    this ILogger logger,
    AielEventIds eventId = AielEventIds.ServiceStart);  // AIEL002 + AIEL005
```

---

## Rules at a Glance

| ID       | Title                       | Severity | Has Fix |
|----------|-----------------------------|----------|---------|
| AIEL001  | UseAielEventIds             | Error    | ✅      |
| AIEL002  | MissingEventIdParameter     | Error    | ✅      |
| AIEL003  | MissingEventIdInMessage     | Error    | ✅      |
| AIEL004  | NoDirectILoggerCalls        | Warning  | ✅      |
| AIEL005  | EventIdMismatch             | Error    | ✅      |

---

## Rule Details

### AIEL001 — UseAielEventIds

**The `EventId` argument of `[LoggerMessage]` must use `(int)<EventIdsEnum>.Member`.**

| ❌ Violation | ✅ Compliant |
| --- | --- |
| `EventId = 1000` | `EventId = (int)AielEventIds.ServiceStart` |
| `EventId = (int)SomeOther.Foo` | `EventId = (int)AielEventIds.ServiceStart` |

Code fix: replaces the expression with `(int)<ConfiguredEnum>.FirstMember`.

---

### AIEL002 — MissingEventIdParameter

**Every `[LoggerMessage]` partial method must declare an optional `<EventIdsEnum> eventId` parameter.**

```csharp
// ❌ Missing parameter
[LoggerMessage(EventId = (int)AielEventIds.ServiceStart, ...)]
public static partial void ServiceStarted(this ILogger logger);

// ✅ Parameter present
[LoggerMessage(EventId = (int)AielEventIds.ServiceStart, ...)]
public static partial void ServiceStarted(this ILogger logger,
    AielEventIds eventId = AielEventIds.ServiceStart);
```

Code fix: appends the parameter with the correct type and default.

---

### AIEL003 — MissingEventIdInMessage

**The `Message` string must contain `[{EventId}]`.**

```csharp
// ❌ Placeholder missing
Message = "Service started"

// ✅ Placeholder present
Message = "[{EventId}] Service started"
```

Code fix: prepends `[{EventId}]` to the existing string literal.

---

### AIEL004 — NoDirectILoggerCalls

**Do not call `ILogger.LogXxx(…)` extension methods directly.**  Use `[LoggerMessage]` partial
methods instead for compile-time verified, allocation-free, structured logging.

```csharp
// ❌ Direct ILogger call
public static void Foo(ILogger logger) => logger.LogInformation("msg");

// ✅ [LoggerMessage] partial method
[LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information,
               Message = "[{EventId}] msg")]
public static partial void Foo(this ILogger logger,
    AielEventIds eventId = AielEventIds.ServiceStart);
```

Code fixes (two options):

1. **Replace with TODO comment** — preserves intent; developer migrates manually.
2. **Remove the statement** — useful when the call is redundant.

---

### AIEL005 — EventIdMismatch

**The `EventId` in the attribute and the default value of the `eventId` parameter must refer
to the same enum member.**

```csharp
// ❌ Mismatch
[LoggerMessage(EventId = (int)AielEventIds.ServiceStart, ...)]
public static partial void ServiceStarted(this ILogger logger,
    AielEventIds eventId = AielEventIds.ServiceStop);  // ← different member

// ✅ Consistent
[LoggerMessage(EventId = (int)AielEventIds.ServiceStart, ...)]
public static partial void ServiceStarted(this ILogger logger,
    AielEventIds eventId = AielEventIds.ServiceStart);
```

Code fixes (two options):

1. **Update attribute** to match the parameter default.
2. **Update parameter** to match the attribute `EventId`.

---

## Configuration — Custom EventIds Enum

By default the analyzers expect `Aiel.Logging.AielEventIds`.  Any project can substitute its
own enum via one of three mechanisms — evaluated in priority order:

### Priority 1 — MSBuild property (recommended)

Add to your project file or a shared `Directory.Build.props`:

```xml
<PropertyGroup>
  <AielEventIdsType>Acme.Logging.AcmeEventIds</AielEventIdsType>
</PropertyGroup>
```

The value must be the **fully-qualified type name** of your enum.

### Priority 2 — `.editorconfig`

```ini
[*.cs]
aiel_event_ids_type = Acme.Logging.AcmeEventIds
```

This is useful when you want per-folder overrides.

### Priority 3 — Default

If neither option is set the analyzers fall back to `Aiel.Logging.AielEventIds`.

### Resolution order summary

```text
build_property.AielEventIdsType   (MSBuild)
        ↓ fallback
aiel_event_ids_type               (.editorconfig)
        ↓ fallback
Aiel.Logging.AielEventIds         (built-in default)
```

### How it works internally

The `AnalyzerConfiguration` class reads the property once per compilation in
`RegisterCompilationStartAction`, resolves the enum `INamedTypeSymbol` via
`EventIdsTypeConfig.GetTypeSymbol(compilation)`, and stamps the resolved names into
every `Diagnostic.Properties` dictionary so code fixes can reconstruct the same type
name without independently re-reading `AnalyzerConfigOptions`.

| Diagnostic property key    | Example value               |
|----------------------------|-----------------------------|
| `EventIdsFullTypeName`     | `Acme.Logging.AcmeEventIds` |
| `EventIdsShortName`        | `AcmeEventIds`              |

Code fixes call `AnalyzerConfiguration.ReadFromDiagnostic(diagnostic)` to obtain an
`EventIdsTypeConfig` with `.FullTypeName` and `.ShortName` already resolved.

---

## Code Fixes

All fixes implement `GetFixAllProvider()` returning `WellKnownFixAllProviders.BatchFixer`,
so **Fix All in Document / Project / Solution** works out of the box.

| Fix class                        | Fixes    | Description                                                  |
| -------------------------------- | -------- | ------------------------------------------------------------ |
| `UseAielEventIdsCodeFix`         | AIEL001  | Replaces raw int / wrong cast with `(int)<Enum>.FirstMember` |
| `MissingEventIdParameterCodeFix` | AIEL002  | Appends `<Enum> eventId = <Enum>.Member` parameter           |
| `MissingEventIdInMessageCodeFix` | AIEL003  | Prepends `[{EventId}]`  to the message string                |
| `NoDirectILoggerCallsCodeFix`    | AIEL004  | Replace with TODO comment **or** remove statement            |
| `EventIdMismatchCodeFix`         | AIEL005  | Sync attribute → parameter **or** parameter → attribute      |

---

## Severity Overrides

Override per-project in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.AIEL001.severity = error
dotnet_diagnostic.AIEL002.severity = error
dotnet_diagnostic.AIEL003.severity = error
dotnet_diagnostic.AIEL004.severity = warning   # downgrade if needed
dotnet_diagnostic.AIEL005.severity = error
```

---

## Running Locally

```bash
# Restore + build
dotnet restore Aiel.Logging.sln
dotnet build   Aiel.Logging.sln

# Run all tests
dotnet test Aiel.Logging.sln --logger "console;verbosity=normal"

# Pack the analyzer NuGet
dotnet pack src/Aiel.Logging.Analyzers/Aiel.Logging.Analyzers.csproj -c Release
```

The resulting `.nupkg` includes the `build/Aiel.Logging.Analyzers.props` file which
automatically declares the `AielEventIdsType` MSBuild property as compiler-visible — no
manual `.props` import is required by consuming projects.

---

## Extending the Analyzers

### Adding a new diagnostic

1. Add a `DiagnosticDescriptor` to `DiagnosticDescriptors.cs`.
2. Create `MyNewAnalyzer.cs` implementing `DiagnosticAnalyzer`.
3. Call `AnalyzerConfiguration.Resolve(context.Options)` in `RegisterCompilationStartAction`.
4. Stamp `AnalyzerConfiguration.BuildDiagnosticProperties(config)` into every reported diagnostic.
5. Add a matching `MyNewCodeFix.cs` that reads config via `AnalyzerConfiguration.ReadFromDiagnostic(diagnostic)`.
6. Add tests using `AielAnalyzerVerifier<T>` and `AielCodeFixVerifier<T, TFix>`.

### Plugging in a different EventIds type

See [Configuration — Custom EventIds Enum](#configuration--custom-eventids-enum) above.
The configuration layer is entirely transparent to the analyzer logic — all you change is the
property value; no code changes are needed.
