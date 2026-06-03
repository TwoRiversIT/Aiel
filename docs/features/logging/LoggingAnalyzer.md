# Aiel Logging Analyzer — Developer Reference

> **Package:** `Aiel.Logging.Analyzers` · `Aiel.Logging.CodeFixes`  
> **Target SDK:** .NET Standard 2.0 (analyzer) · .NET 8 (tests/template)  
> **Rules:** AIEL001 – AIEL005  

---

## Table of Contents

1. [Overview](#overview)
2. [Installation](#installation)
3. [Rule Reference](#rule-reference)
   - [AIEL001 – UseAielEventIds](#aiel001--useaieleventids)
   - [AIEL002 – MissingEventIdParameter](#aiel002--missingeventidparameter)
   - [AIEL003 – MissingEventIdInMessage](#aiel003--missingeventidinmessage)
   - [AIEL004 – NoDirectILoggerCalls](#aiel004--nodirectiloggercalls)
   - [AIEL005 – EventIdMismatch](#aiel005--eventidinmismatch)
4. [Code-Fix Behaviour](#code-fix-behaviour)
5. [Adding a New Event ID](#adding-a-new-event-id)
6. [Extending the Analyzer](#extending-the-analyzer)
7. [EditorConfig Snippet](#editorconfig-snippet)
8. [Running Locally](#running-locally)
9. [Architecture Notes](#architecture-notes)

---

## Overview

The Aiel logging analyzers enforce a consistent structured-logging contract
across all Aiel framework modules:

```
[LoggerMessage(EventId = (int)AielEventIds.ModuleStart,
               Level   = LogLevel.Information,
               Message = "[{EventId}] Module started: {ModuleName}")]
public static partial void LogModuleStart(
    this ILogger logger,
    string moduleName,
    AielEventIds eventId = AielEventIds.ModuleStart);   // ← the contract
```

Every rule has a **code fix** so violations can be repaired with a single
Alt+Enter / Ctrl+. action in any Roslyn-aware IDE.

---

## Installation

### Via NuGet (recommended)

```xml
<ItemGroup>
  <PackageReference Include="Aiel.Logging.Analyzers" Version="1.0.0" />
  <PackageReference Include="Aiel.Logging.CodeFixes"  Version="1.0.0" />
</ItemGroup>
```

Both packages set `PrivateAssets="all"` and `DevelopmentDependency="true"` by
default, so they are compile-time-only and are never shipped as transitive
dependencies.

### Via project reference (monorepo / source build)

```xml
<ItemGroup>
  <ProjectReference Include="path/to/Aiel.Logging.Analyzers.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="path/to/Aiel.Logging.CodeFixes.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

---

## Rule Reference

### AIEL001 – UseAielEventIds

| Property | Value |
|---|---|
| ID | AIEL001 |
| Severity | Warning |
| Category | AielLogging |

**Problem:** The `EventId` argument of a `[LoggerMessage]` attribute is a
raw integer literal.

```csharp
// ❌ Triggers AIEL001
[LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "...")]
```

**Solution:** Use `(int)AielEventIds.MemberName`.

```csharp
// ✔ Compliant
[LoggerMessage(EventId = (int)AielEventIds.ModuleStart, ...)]
```

**Code fix:** *Replace with (int)AielEventIds member* — automatically selects
the matching enum member when the integer maps to a known `AielEventIds` value.

---

### AIEL002 – MissingEventIdParameter

| Property | Value |
|---|---|
| ID | AIEL002 |
| Severity | Warning |
| Category | AielLogging |

**Problem:** A `[LoggerMessage]`-decorated method does not expose an optional
`AielEventIds eventId` parameter, preventing callers from overriding the ID.

```csharp
// ❌ Triggers AIEL002
[LoggerMessage(EventId = (int)AielEventIds.ModuleStart, ...)]
public static partial void LogModuleStart(ILogger logger, string name);
```

**Solution:** Add the optional parameter with the matching default.

```csharp
// ✔ Compliant
public static partial void LogModuleStart(
    ILogger logger, string name,
    AielEventIds eventId = AielEventIds.ModuleStart);
```

**Code fix:** *Add optional AielEventIds eventId parameter* — appends the
parameter with the correct default inferred from the attribute.

---

### AIEL003 – MissingEventIdInMessage

| Property | Value |
|---|---|
| ID | AIEL003 |
| Severity | Warning |
| Category | AielLogging |

**Problem:** The `Message` template in `[LoggerMessage]` does not contain
the `[{EventId}]` structured-logging placeholder.

```csharp
// ❌ Triggers AIEL003
[LoggerMessage(..., Message = "Module started")]
// ❌ Also triggers (missing surrounding brackets)
[LoggerMessage(..., Message = "{EventId} Module started")]
```

**Solution:** Prefix the message with `[{EventId}]`.

```csharp
// ✔ Compliant
[LoggerMessage(..., Message = "[{EventId}] Module started")]
```

**Code fix:** *Prepend [{EventId}] to message template* — inserts
`"[{EventId}] "` at the beginning of the string literal.

---

### AIEL004 – NoDirectILoggerCalls

| Property | Value |
|---|---|
| ID | AIEL004 |
| Severity | Warning |
| Category | AielLogging |

**Problem:** Production code calls `ILogger` extension methods directly
(e.g. `LogInformation`, `LogError`) instead of using a structured
`[LoggerMessage]` helper.

```csharp
// ❌ Triggers AIEL004
_logger.LogInformation("[{EventId}] Starting");
_logger.LogError("Something failed");
```

**Solution:** Create and call a `[LoggerMessage]`-decorated helper.

```csharp
// ✔ Compliant
_logger.LogModuleStart("MyModule");
```

**Code fix (two options):**
- *Replace with [LoggerMessage] helper* — rewires the call to an existing
  helper in the same type when one is found.
- *Add TODO comment* — inserts a `// TODO (AIEL004)` comment when no helper
  can be found automatically.

---

### AIEL005 – EventIdMismatch

| Property | Value |
|---|---|
| ID | AIEL005 |
| Severity | **Error** |
| Category | AielLogging |

**Problem:** The integer `EventId` declared in `[LoggerMessage]` disagrees
with the default value of the `AielEventIds eventId` parameter on the same
method.

```csharp
// ❌ Triggers AIEL005 – attribute says ModuleStart (1001), param says ModuleStop (1002)
[LoggerMessage(EventId = (int)AielEventIds.ModuleStart, ...)]
public static partial void LogModuleStart(
    ILogger logger, AielEventIds eventId = AielEventIds.ModuleStop);
```

**Code fix (two options):**
- *Sync parameter default to match [LoggerMessage] EventId* — corrects the
  parameter default to match the attribute (trust the attribute).
- *Sync [LoggerMessage] EventId to match parameter default* — corrects the
  attribute to match the parameter default (trust the parameter).

---

## Code-Fix Behaviour

All code fixes implement `GetFixAllProvider()` returning
`WellKnownFixAllProviders.BatchFixer`, which means:

- **Fix one** — repairs the single highlighted occurrence.
- **Fix all in document** — repairs every occurrence in the open file.
- **Fix all in project / solution** — repairs every occurrence across the
  entire project or solution in one pass.

Fixes are **format-aware**: they attach `Formatter.Annotation` to inserted
nodes so the IDE's formatter applies your `.editorconfig` indentation and
spacing preferences.

---

## Adding a New Event ID

1. Open `AielEventIds.cs` in the framework assembly.
2. Append a new member to the correct module block with a **unique value**:

   ```csharp
   /// <summary>Description of when this event fires.</summary>
   NewEvent = 1005,
   ```

3. Create a logging helper in the module's log class:

   ```csharp
   [LoggerMessage(
       EventId = (int)AielEventIds.NewEvent,
       Level   = LogLevel.Information,
       Message = "[{EventId}] Something new happened: {Detail}")]
   public static partial void LogNewEvent(
       this ILogger logger,
       string detail,
       AielEventIds eventId = AielEventIds.NewEvent);
   ```

4. The analyzer now knows about the new member and will suggest it in AIEL001
   fixes automatically (no code change required to the analyzer itself).

---

## Extending the Analyzer

### Adding a new rule

1. Add a `DiagnosticDescriptor` to `DiagnosticDescriptors.cs`.
2. Create `MyNewAnalyzer.cs` in `Aiel.Logging.Analyzers/Analyzers/`.
3. Implement the corresponding code fix in `Aiel.Logging.CodeFixes/CodeFixes/`.
4. Add tests in `Aiel.Logging.Analyzers.Tests/Tests/`.

### Shared infrastructure

| Type | Purpose |
|---|---|
| `WellKnownTypes` | Single source for all string constants (type names, method names, placeholders) |
| `AnalyzerHelpers` | Reusable symbol inspection and syntax-finding utilities |
| `AielAnalyzerVerifier<T>` | Test harness with embedded framework stubs |
| `AielCodeFixVerifier<T,F>` | Code-fix test harness with FixAll support |

---

## EditorConfig Snippet

Add to your `.editorconfig` to configure rule severities project-wide:

```ini
[*.cs]
# Aiel Logging Analyzer rules
dotnet_diagnostic.AIEL001.severity = warning   # UseAielEventIds
dotnet_diagnostic.AIEL002.severity = warning   # MissingEventIdParameter
dotnet_diagnostic.AIEL003.severity = warning   # MissingEventIdInMessage
dotnet_diagnostic.AIEL004.severity = warning   # NoDirectILoggerCalls
dotnet_diagnostic.AIEL005.severity = error     # EventIdMismatch (intentionally error)

# Suppress AIEL004 in test projects if you need direct ILogger calls in tests
# dotnet_diagnostic.AIEL004.severity = none
```

---

## Running Locally

```bash
# Clone and build
git clone https://github.com/your-org/aiel-analyzers.git
cd aiel-analyzers
dotnet restore Aiel.Logging.sln
dotnet build  Aiel.Logging.sln

# Run all tests
dotnet test Aiel.Logging.sln --logger "console;verbosity=detailed"

# Run only a specific rule's tests
dotnet test tests/Aiel.Logging.Analyzers.Tests \
  --filter "FullyQualifiedName~AIEL001"

# Pack NuGet packages locally
dotnet pack src/Aiel.Logging.Analyzers --configuration Release --output ./artifacts
dotnet pack src/Aiel.Logging.CodeFixes --configuration Release --output ./artifacts
```

### Seeing diagnostics in Visual Studio / Rider

Open `src/Aiel.Logging.Template/SampleViolations.cs`.  
Each violation is annotated with the rule ID in a comment above it.  
The IDE will underline them and offer code-fix lightbulbs.

---

## Architecture Notes

```
Aiel.Logging.Analyzers          (netstandard2.0)
├── Helpers/
│   ├── WellKnownTypes.cs       – String constants (no magic strings elsewhere)
│   └── AnalyzerHelpers.cs      – Shared symbol/syntax helpers
└── Analyzers/
    ├── DiagnosticDescriptors.cs – All DiagnosticDescriptor definitions
    ├── UseAielEventIdsAnalyzer.cs         (AIEL001)
    ├── MissingEventIdParameterAnalyzer.cs (AIEL002)
    ├── MissingEventIdInMessageAnalyzer.cs (AIEL003)
    ├── NoDirectILoggerCallsAnalyzer.cs    (AIEL004)
    └── EventIdMismatchAnalyzer.cs         (AIEL005)

Aiel.Logging.CodeFixes          (netstandard2.0)
└── CodeFixes/
    ├── UseAielEventIdsCodeFix.cs          (fixes AIEL001)
    ├── MissingEventIdParameterCodeFix.cs  (fixes AIEL002)
    ├── MissingEventIdInMessageCodeFix.cs  (fixes AIEL003)
    ├── NoDirectILoggerCallsCodeFix.cs     (fixes AIEL004)
    └── EventIdMismatchCodeFix.cs          (fixes AIEL005 – two fix options)

Aiel.Logging.Analyzers.Tests    (net8.0)
├── Verifiers/
│   ├── AnalyzerVerifier.cs     – Analyzer test harness + framework stubs
│   └── CodeFixVerifier.cs      – Code-fix test harness (incl. FixAll)
└── Tests/
    ├── UseAielEventIdsAnalyzerTests.cs
    ├── UseAielEventIdsCodeFixTests.cs
    ├── MissingEventIdParameterAnalyzerTests.cs
    ├── MissingEventIdParameterCodeFixTests.cs
    ├── MissingEventIdInMessageAnalyzerTests.cs
    ├── MissingEventIdInMessageCodeFixTests.cs
    ├── NoDirectILoggerCallsAnalyzerTests.cs
    └── EventIdMismatchAnalyzerTests.cs + EventIdMismatchCodeFixTests.cs

Aiel.Logging.Template           (net8.0 – sample only)
├── AielEventIds.cs             – Canonical enum definition
├── SampleCompliant.cs          – Fully compliant usage pattern
└── SampleViolations.cs         – Intentional violations (one per rule)
```

**Design principles:**
- All analyzers target `netstandard2.0` for maximum compatibility.
- No magic strings — every framework type and member name lives in
  `WellKnownTypes`.
- Analyzers are conservative: they skip cases they cannot fully resolve
  rather than emitting false positives.
- Code fixes attach `Formatter.Annotation` so the IDE respects
  your `.editorconfig` without extra effort.
- Tests use the official `Microsoft.CodeAnalysis.Testing` harness — the
  same infrastructure that the Roslyn team uses internally.

