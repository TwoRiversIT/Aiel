# Aiel.Logging

[![CI](https://github.com/TwoRiversIT/Aiel/actions/workflows/ci.yml/badge.svg)](https://github.com/TwoRiversIT/Aiel/actions)
[![NuGet](https://img.shields.io/nuget/v/Aiel.Logging.Analyzers.svg)](https://nuget.org/packages/Aiel.Logging.Analyzers)

Roslyn source analyzers + code fixes that enforce the **Aiel structured-logging convention** at compile time — zero runtime overhead, IDE-integrated, `FixAll`-capable.  For more information, see the [Aiel.Logging](https://github.com/TwoRiversIT/Aiel/blob/main/src/Aiel.Logging/README.md) documentation.

---

## Quick Start

```bash
dotnet add package Aiel.Logging.Analyzers
```

After installation every `[LoggerMessage]`-decorated partial method in the project is checked
against the five rules below.  Violations appear as compiler errors/warnings with one-click
code fixes in Visual Studio, VS Code, and Rider.

---

## Rules

| ID      | Rule                    | Severity | Description                                                                                                                   |
| ------- | ----------------------- | -------- | ----------------------------------------------------------------------------------------------------------------------------- |
| AIEL00008 | UseAielEventIds         | Error    | `EventId` must use `(int)<EventIdsEnum>.Member`, not a raw integer or a cast from a foreign enum                              |
| AIEL00009 | MissingEventIdParameter | Error    | Every `[LoggerMessage]` partial method must declare `<EventIdsEnum> eventId = <EventIdsEnum>.Member` as an optional parameter |
| AIEL00010 | MissingEventIdInMessage | Error    | The `Message` string must contain the `[{EventId}]` placeholder                                                               |
| AIEL00011 | NoDirectILoggerCalls    | Warning  | Do not call `ILogger.LogXxx(...)` directly; use `[LoggerMessage]` partial methods                                               |
| AIEL00012 | EventIdMismatch         | Error    | The `EventId` in the attribute and the default of the `eventId` parameter must refer to the same enum member                  |

### Compliant example

```csharp
[LoggerMessage(
    EventId = (int)AielEvent.ServiceStart,    // AIEL00008
    Level   = LogLevel.Information,
    Message = "[{EventId}] Service started")]     // AIEL00010
public static partial void ServiceStarted(
    this ILogger logger,
    AielEvent eventId = AielEvent.ServiceStart); // AIEL00009 + AIEL00012
```

---

## Configuration — Custom EventIds Enum

By default the analyzers look for `Aiel.Logging.AielEvent`.  You can substitute **any** enum by setting a single MSBuild property or `.editorconfig` key — no code changes needed.

### Option 1: MSBuild property *(recommended)*

Add to your `.csproj` or a shared `Directory.Build.props`:

```xml
<PropertyGroup>
  <AielEventIdsType>Acme.Logging.AcmeEventIds</AielEventIdsType>
</PropertyGroup>
```

> The `Aiel.Logging.Analyzers.props` file (shipped inside the NuGet package) automatically declares this property as compiler-visible — no manual import required.

### Option 2: `.editorconfig`

```ini
[*.cs]
aiel_event_ids_type = Acme.Logging.AcmeEventIds
```

Useful for per-folder overrides in a monorepo.

### Priority order

```text
MSBuild <AielEventIdsType>   (highest)
  — fallback
.editorconfig aiel_event_ids_type
  — fallback
Aiel.Logging.AielEvent    (built-in default)
```

The resolved type name is stamped into every reported `Diagnostic.Properties` entry so that code fixes reconstruct the correct enum name without re-reading configuration independently.

---

## Code Fixes

All five rules have IDE code fixes.  AIEL00011 and AIEL00012 each offer **two fix alternatives**.
Every fix supports **Fix All in Document / Project / Solution** via the Batch fixer.

| Fix                              | Rule    | What it does                                                   |
| -------------------------------- | ------- | -------------------------------------------------------------- |
| `UseAielEventIdsCodeFix`         | AIEL00008 | Replaces raw int / wrong cast with `(int)<Enum>.FirstMember`   |
| `MissingEventIdParameterCodeFix` | AIEL00009 | Appends `<Enum> eventId = <Enum>.Member` to the parameter list |
| `MissingEventIdInMessageCodeFix` | AIEL00010 | Prepends `[{EventId}]` to the message literal                  |
| `NoDirectILoggerCallsCodeFix`    | AIEL00011 | Replace with TODO comment, or Remove the statement         |
| `EventIdMismatchCodeFix`         | AIEL00012 | Sync attribute and parameter, or Sync parameter and attribute  |

---

## Severity Overrides

Configure per project in `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.AIEL00008.severity = error
dotnet_diagnostic.AIEL00009.severity = error
dotnet_diagnostic.AIEL00010.severity = error
dotnet_diagnostic.AIEL00011.severity = warning
dotnet_diagnostic.AIEL00012.severity = error
```

---

## Building & Testing

```bash
# Clone
git clone https://github.com/TwoRiversIT/Aiel
cd Aiel

# Build
dotnet restore Aiel.Logging.sln
dotnet build   Aiel.Logging.sln -c Release

# Test
dotnet test Aiel.Logging.sln --logger "console;verbosity=normal"

# Pack
dotnet pack src/Aiel.Logging.Analyzers/Aiel.Logging.Analyzers.csproj -c Release
```

### Repository layout

```text
aiel-analyzers/
├── src/
│   ├── Aiel.Logging.Analyzers/        # Analyzer library (netstandard2.0)
│   │   ├── Analyzers/                 # DiagnosticDescriptors + 5 analyzers
│   │   ├── Configuration/             # AnalyzerConfiguration + EventIdsTypeConfig
│   │   ├── Helpers/                   # WellKnownTypes, AnalyzerHelpers
│   │   └── build/                     # .props (CompilerVisibleProperty)
├── Aiel.Logging.CodeFixes/            # Code-fix library (netstandard2.0)
│   ├── CodeFixes/                     # 5 code-fix providers
│   ├── Aiel.Logging.Template/         # Sample project (net10.0)
│       ├── AielEvent.cs               # Full event-id enum example
│       ├── SampleCompliant.cs         # … All rules
│       └── Verifiers/                 # AielAnalyzerVerifier + AielCodeFixVerifier
├── docs/
│   └── LoggingAnalyzer.md             # Full developer reference
├── .github/workflows/ci.yml           # Build + test + pack on every push
├── .editorconfig                      # Severity overrides + code style
```

---

## CI / CD

GitHub Actions runs on every push and pull request:

- **Build**: `dotnet build`  
- **Test**: `dotnet test` with JUnit results uploaded as an artifact  
- **Pack** *(Release branch only)*: `dotnet pack`  NuGet artifact  

See [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

---

## Extending

See [docs/LoggingAnalyzer.md](docs/LoggingAnalyzer.md#extending-the-analyzers) for a step-by-step guide to adding new rules and wiring them into the configuration system.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
