# Aiel Logging Analyzers

[![CI](https://github.com/your-org/aiel-analyzers/actions/workflows/ci.yml/badge.svg)](https://github.com/your-org/aiel-analyzers/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Aiel.Logging.Analyzers.svg)](https://www.nuget.org/packages/Aiel.Logging.Analyzers)

Roslyn source analyzers and code fixes that enforce the **Aiel framework structured-logging contract** at compile time — zero runtime overhead, IDE-native feedback, and one-click fixes.

---

## Rules at a glance

| Rule | Severity | Title |
|---|---|---|
| AIEL001 | ⚠ Warning | Use `AielEventIds` enum — no raw integer literals in `[LoggerMessage]` |
| AIEL002 | ⚠ Warning | Every `[LoggerMessage]` helper must expose `AielEventIds eventId = …` |
| AIEL003 | ⚠ Warning | Message template must contain `[{EventId}]` placeholder |
| AIEL004 | ⚠ Warning | Do not call `ILogger` methods directly — use `[LoggerMessage]` helpers |
| AIEL005 | 🔴 Error  | `EventId` in attribute and `eventId` parameter default must agree |

Every rule ships with a **code fix** (including FixAll support).

---

## Quick start

```xml
<!-- In your .csproj -->
<ItemGroup>
  <PackageReference Include="Aiel.Logging.Analyzers" Version="1.0.0" />
  <PackageReference Include="Aiel.Logging.CodeFixes"  Version="1.0.0" />
</ItemGroup>
```

The packages are compile-time only (`PrivateAssets="all"`) and never become transitive dependencies.

---

## The compliant logging pattern

```csharp
// 1. Define your helpers ──────────────────────────────────────────────
internal static partial class MyModuleLog
{
    [LoggerMessage(
        EventId = (int)AielEventIds.ModuleStart,   // ← AIEL001: must use enum cast
        Level   = LogLevel.Information,
        Message = "[{EventId}] Module started: {Name}")]  // ← AIEL003: must have [{EventId}]
    public static partial void LogModuleStart(
        this ILogger logger,
        string name,
        AielEventIds eventId = AielEventIds.ModuleStart);  // ← AIEL002 + AIEL005: must match
}

// 2. Call helpers — never ILogger directly ────────────────────────────
public class MyModule
{
    private readonly ILogger<MyModule> _logger;

    public void Start(string name)
    {
        _logger.LogModuleStart(name);  // ← AIEL004: correct — uses helper
        // _logger.LogInformation(...)  // ← AIEL004: would fire here
    }
}
```

## Running locally

```bash
# Build everything
dotnet build Aiel.Logging.sln

# Run all tests
dotnet test Aiel.Logging.sln --logger "console;verbosity=normal"

# Run tests for a specific rule (e.g. AIEL003)
dotnet test tests/Aiel.Logging.Analyzers.Tests \
  --filter "FullyQualifiedName~AIEL003"

# Pack NuGet packages
dotnet pack src/Aiel.Logging.Analyzers --configuration Release --output ./artifacts
dotnet pack src/Aiel.Logging.CodeFixes --configuration Release --output ./artifacts
```

Open `src/Aiel.Logging.Template/SampleViolations.cs` in Visual Studio or
Rider to see all five rules light up with underlines and lightbulb fixes.

---

## Configuring severity via `.editorconfig`

```ini
[*.cs]
dotnet_diagnostic.AIEL001.severity = warning
dotnet_diagnostic.AIEL002.severity = warning
dotnet_diagnostic.AIEL003.severity = warning
dotnet_diagnostic.AIEL004.severity = warning
dotnet_diagnostic.AIEL005.severity = error
```

Suppress a rule per-file with the standard `#pragma warning disable AIEL00x` or
per-project via `<NoWarn>AIEL004</NoWarn>`.

---

## Adding a new event ID

1. Add a member to `AielEventIds.cs` with a unique value in the correct module range.
2. Create a `[LoggerMessage]`-decorated helper.
3. The analyzer immediately understands the new member — no changes to the
   analyzer code are needed.

For full extension guidance see [docs/LoggingAnalyzer.md](docs/LoggingAnalyzer.md).

---

## Contributing

Pull requests welcome. Please:
- Add tests for any new rule or fix.
- Run `dotnet format` before committing.
- Ensure CI passes (build → test → format check).

## License

MIT © Aiel Framework Contributors


