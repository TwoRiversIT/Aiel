# Aiel.Gps.HP Implementation Plan

This document tracks the progress of the high-performance NMEA parsing library rewrite.

## Goals

- **Primary:** Minimize allocations for high-throughput NMEA parsing
- **Secondary:** Maintain extensibility for custom NMEA message types
- **Tertiary:** Provide both zero-allocation single-parse and batch processing APIs

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Aiel.Gps.HP                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                     │
│  │  Lexer          │    │  INmeaParser<T> │                     │
│  │  (ref struct)   │    │  (interface)    │                     │
│  └────────┬────────┘    └────────┬────────┘                     │
│           │                      │                              │
│           ▼                      ▼                              │
│  ┌─────────────────────────────────────────┐                    │
│  │  NmeaSingleParser.Parse<T>()            │  ◄── Zero-alloc    │
│  │  (static, generic, compile-time known)  │      path          │
│  └─────────────────────────────────────────┘                    │
│                                                                 │
│  ┌─────────────────────────────────────────┐                    │
│  │  NmeaMessage (discriminated union)      │  ◄── Generated     │
│  │  - GLL, GGA, RMC, GSA, GSV, VTG, GFDTA  │      by source     │
│  │  - Match(), TryGetGll(), IsGll, etc.    │      generator     │
│  └────────────────────┬────────────────────┘                    │
│                       │                                         │
│                       ▼                                         │
│  ┌─────────────────────────────────────────┐                    │
│  │  NmeaBatchReader                        │  ◄── Batch/stream  │
│  │  - IAsyncEnumerable<NmeaMessage>        │      processing    │
│  │  - Pipeline-based I/O                   │                    │
│  │  - Error queue                          │                    │
│  └─────────────────────────────────────────┘                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Allocation Strategy

| Message Type | Registration | Per Sentence Parsed |
|--------------|--------------|---------------------|
| **Built-in** (GLL, GGA, RMC, etc.) | 0 allocations | 0 allocations (discriminated union) |
| **Custom** (user-defined at runtime) | 1 allocation (delegate) | 1 allocation (boxing) |

---

## Phase 1: Fix Core Infrastructure

**Goal:** Get the project compiling with a minimal, working foundation.

### Tasks

- [x] Delete broken `NmeaReader.cs` from HP project
- [x] Delete broken `NmeaParserEntry.cs` (will redesign in Phase 2)
- [x] Delete broken `NmeaParserRegistry.cs` (will redesign in Phase 2)
- [x] Delete broken `NmeaStreamReader.cs` (will redesign in Phase 3)
- [x] Delete broken `NmeaPipelineReader.cs` (will redesign in Phase 3)
- [x] Delete broken `NmeaParserBase.cs` (unused)
- [x] Implement `Lexer.PeekIdentifier()`
- [x] Fix `Lexer` closure bugs (captured `_currentByte` by value in predicates)
- [x] Fix `Lexer.ByteAt()` bounds checking
- [x] Fix `Lexer.ConsumeString()` to update `_currentByte`
- [x] Fix `Lexer.NextString()` to use `Encoding.UTF8.GetString()`
- [x] Clean up `INmeaParser` interface (remove dead code)
- [x] Clean up `GllParser` and `GfdtaParser` (remove `ParseUntyped`)
- [x] Verify `NmeaSingleParser` works with existing parsers
- [x] Ensure project builds successfully
- [x] Create test project `Aiel.Gps.HP.UnitTests`
- [x] Create unit tests for Lexer (19 tests)
- [x] Create unit tests for NmeaSingleParser
- [x] All tests passing (516 total)

### Known Issues for Later

- Lexer uses `Peek()` internally which is marked obsolete - refactor needed
- Consider removing closure-based `Advance(Func<>)` pattern entirely (performance)

### Status: ✅ Complete

---

## Phase 2: Discriminated Union (Source Generator)

**Goal:** Create a source generator that produces a discriminated union for known message types.

### Tasks

- [x] Create `Aiel.Gps.HP.Generators` project
- [x] Create `[NmeaMessage]` attribute for message structs
- [x] Create `[NmeaParser]` attribute for parser structs
- [x] Generate `NmeaMessageType` enum
- [x] Generate `NmeaMessage` struct (non-overlapping due to reference types in messages)
- [x] Generate `Match()` method for exhaustive pattern matching (both `Func<>` and `Action<>` variants)
- [x] Generate `TryGetXxx()` methods for each message type
- [x] Generate `IsXxx` properties for type checking
- [x] Generate `FromXxx()` factory methods
- [x] Generate static `TryParse(ReadOnlySpan<Byte>)` dispatcher
- [x] Generate `ToString()` override
- [x] Create unit tests for generated code (11 tests)
- [x] All tests passing (516 total)

### Design Notes

The original plan was to use `[StructLayout(LayoutKind.Explicit)]` with overlapping fields for true
zero-allocation. However, the CLR does not allow reference types (strings) to overlap with value types.
Since `GFDTA` contains `String` fields, we had to use non-overlapping storage.

This means the `NmeaMessage` struct is larger (contains space for all message types), but:
- Still a value type (no heap allocation for the union itself)
- Strings within messages still allocate (unavoidable for string data)
- Pattern matching and type checking are still efficient

### Status: ✅ Complete

---

## Phase 3: Batch Reader

**Goal:** Create a high-performance batch/stream reader for processing large NMEA data streams.

### Tasks

- [x] Create `NmeaBatchReader` class
- [x] Implement pipeline-based I/O with `System.IO.Pipelines`
- [x] Return `IAsyncEnumerable<NmeaMessage>`
- [x] Implement error channel for unparseable sentences (`ReadErrorsAsync()`)
- [x] Add statistics (`BatchReaderStatistics` class)
- [x] Create `BatchReaderStatistics` class with thread-safe counters
- [x] Support cancellation tokens
- [x] Handle partial sentences gracefully
- [x] Handle garbage data before sentences
- [x] Create unit tests (10 tests)
- [x] All tests passing (516 total)

### Features Implemented

| Feature | Description |
|---------|-------------|
| `ReadAsync()` | Returns `IAsyncEnumerable<NmeaMessage>` for streaming messages |
| `ReadErrorsAsync()` | Returns `IAsyncEnumerable<ParseError>` for failed parses |
| `Statistics` | Thread-safe counters for BytesRead, TotalSentences, ParsedMessages, Errors |
| Pipeline I/O | Uses `System.IO.Pipelines` for efficient buffered reading |
| Cancellation | Full support for `CancellationToken` |

### Status: ✅ Complete

---

## Phase 4: Optional Runtime Extensibility

**Goal:** Allow users to register custom parsers at runtime (accepting 1 allocation per message).

### Tasks

- [x] Design `ICustomNmeaParser` interface for runtime parsers
- [x] Implement `NmeaParserRegistry` for custom parser registration
- [x] Integrate with `NmeaBatchReader` (registry parameter)
- [x] Add `ReadCustomMessagesAsync()` for custom messages
- [x] Update `BatchReaderStatistics` with `CustomMessages` counter
- [x] Create unit tests (10 tests)
- [x] All tests passing

### Files Created

| File | Purpose |
|------|---------|
| `ICustomNmeaParser.cs` | Interface for runtime-registered parsers |
| `NmeaParserRegistry.cs` | Thread-safe registry for custom parsers |
| `NmeaParserRegistryTests.cs` | Unit tests for registry and integration |

### Usage Example

```csharp
// Create a custom parser
public sealed class MyCustomParser : ICustomNmeaParser
{
    public ReadOnlySpan<Byte> Identifier => "MYCST"u8;

    public Object Parse(ref Lexer lexer)
    {
        lexer.ConsumeString(); // Skip identifier
        return new MyCustomMessage
        {
            Field1 = lexer.NextInteger(),
            Field2 = lexer.NextString()
        };
    }
}

// Register and use
var registry = new NmeaParserRegistry();
registry.Register(new MyCustomParser());

var reader = new NmeaBatchReader(stream, registry: registry);

// Read custom messages concurrently with built-in messages
var customTask = Task.Run(async () =>
{
    await foreach (var custom in reader.ReadCustomMessagesAsync())
    {
        var msg = (MyCustomMessage)custom;
        // Handle custom message
    }
});

await foreach (var message in reader.ReadAsync())
{
    // Handle built-in messages
}

await customTask;
```

### Design Notes

- Custom parsers return boxed `Object` (1 allocation per message)
- Built-in messages (via source generator) remain zero-allocation
- Registry is thread-safe using `ConcurrentDictionary`
- Custom messages flow through a separate `Channel<Object>`

### Status: ✅ Complete

---

## Phase 5: Performance Comparison with Previous Version

**Goal:** Benchmark the new HP implementation against the original `Aiel.Gps` library to quantify improvements.

### Tasks

- [x] Add reference to `Aiel.Gps.HP` in benchmark project
- [x] Create `HpParsingBenchmarks.cs` with HP-specific benchmarks
- [x] Create `HpVsOriginalBenchmarks.cs` for side-by-side comparison
- [x] Add v2 (DKW.NMEA) to benchmark project for full historical comparison
- [x] Create `V2VsV3VsHpBenchmarks.cs` for three-way comparison
- [x] Run benchmarks and document results
- [x] Analyze for regressions or unexpected behavior

### Benchmark Categories

| Benchmark Class | Purpose |
|-----------------|---------|
| `HpParsingBenchmarks` | HP-only benchmarks (single parse, batch parse) |
| `HpVsOriginalBenchmarks` | v3 vs v4 comparison |
| `V2VsV3VsHpBenchmarks` | Full three-way comparison (v2, v3, v4) |

### Benchmark Results

#### Single Message Parsing (GLL)

| Version | Mean | Allocated | Ratio |
|---------|------|-----------|-------|
| v2 (DKW.NMEA) | 663 ns | 528 B | 1.00 (baseline) |
| v3 (Aiel) | 648 ns | 216 B | 0.98x |
| **v4 (HP)** | **82 ns** | **0 B** | **0.12x (8x faster)** |

#### Stream Parsing - Large Dataset (13,470 messages)

| Version | Mean | Allocated | Ratio |
|---------|------|-----------|-------|
| v2 (DKW.NMEA) | 12.5 ms | 16.3 MB | 1.00 (baseline) |
| v3 (Aiel) | 19.1 ms | 7.6 MB | 1.53x slower |
| **v4 (HP)** | **2.5 ms** | **3.2 MB** | **0.20x (5x faster)** |

#### Key Findings

- **Single message: 0 bytes allocated** - True zero-allocation achieved
- **Stream parsing: 5-6x faster** than original v2
- **Memory reduced by 80%** (16.3 MB → 3.2 MB for large dataset)
- **Peak throughput: 5.4 million messages/second**

### How to Run

```powershell
cd tests\Aiel.Gps.Benchmarks
dotnet run -c Release --framework net10.0 --os win --arch x64 -- --filter "*V2VsV3VsHpBenchmarks*"
```

### Status: ✅ Complete

--- 

## Files Reference

### To Keep (Phase 1)

| File | Purpose |
|------|---------|
| `Lexer.cs` | Zero-allocation tokenizer (ref struct) |
| `INmeaParser.cs` | Parser interface |
| `NmeaSingleParser.cs` | Static single-sentence parser |
| `ParseError.cs` | Error representation |
| `ReadOnlySequenceExtensions.cs` | Buffer utilities |
| `Sentences/GLL.cs` | GLL message and parser |
| `Sentences/GFDTA.cs` | GFDTA message and parser |

### To Delete (Phase 1)

| File | Reason |
|------|--------|
| `NmeaReader.cs` | References non-existent types, wrong design |
| `NmeaParserEntry.cs` | Cannot work with ref struct Lexer |
| `NmeaParserRegistry.cs` | Delegate approach incompatible with ref struct |
| `NmeaStreamReader.cs` | Depends on broken registry |
| `NmeaPipelineReader.cs` | Depends on broken registry |
| `NmeaParserBase.cs` | Unused abstract base |

---

## Change Log

| Date | Phase | Change |
|------|-------|--------|
| 2025-01-XX | 0 | Created implementation plan |
| 2025-01-XX | 1 | Deleted broken files (NmeaReader, NmeaParserEntry, NmeaParserRegistry, NmeaStreamReader, NmeaPipelineReader, NmeaParserBase) |
| 2025-01-XX | 1 | Implemented `Lexer.PeekIdentifier()` |
| 2025-01-XX | 1 | Fixed Lexer closure bugs - replaced `Advance(Func<>)` with direct while loops |
| 2025-01-XX | 1 | Fixed `Lexer.ByteAt()` bounds checking for negative indices |
| 2025-01-XX | 1 | Fixed `Lexer.ConsumeString()` to update `_currentByte` after advancing |
| 2025-01-XX | 1 | Fixed `Lexer.NextString()` to use `Encoding.UTF8.GetString()` |
| 2025-01-XX | 1 | Created test project and 19 unit tests |
| 2025-01-XX | 1 | Phase 1 complete - all 516 tests passing |
| 2025-01-XX | 2 | Created `Aiel.Gps.HP.Generators` source generator project |
| 2025-01-XX | 2 | Created `[NmeaMessage]` and `[NmeaParser]` attributes |
| 2025-01-XX | 2 | Added attributes to GLL and GFDTA message/parser structs |
| 2025-01-XX | 2 | Implemented `NmeaMessageUnionGenerator` source generator |
| 2025-01-XX | 2 | Generator produces: enum, struct with IsXxx/TryGetXxx/FromXxx/Match/TryParse/ToString |
| 2025-01-XX | 2 | Changed from overlapping to non-overlapping layout (CLR limitation with reference types) |
| 2025-01-XX | 2 | Created 11 unit tests for discriminated union |
| 2025-01-XX | 2 | Phase 2 complete - all 516 tests passing |
| 2025-01-XX | 3 | Created `NmeaBatchReader` with pipeline-based I/O |
| 2025-01-XX | 3 | Created `BatchReaderStatistics` for tracking read metrics |
| 2025-01-XX | 3 | Added `ReadAsync()` returning `IAsyncEnumerable<NmeaMessage>` |
| 2025-01-XX | 3 | Added `ReadErrorsAsync()` returning `IAsyncEnumerable<ParseError>` |
| 2025-01-XX | 3 | Extended `ParseError` class with simpler constructor |
| 2025-01-XX | 3 | Created 10 unit tests for batch reader |
| 2025-01-XX | 3 | Phase 3 complete - all 516 tests passing |
| 2025-01-XX | 4 | Created `ICustomNmeaParser` interface for runtime extensibility |
| 2025-01-XX | 4 | Created `NmeaParserRegistry` with thread-safe registration |
| 2025-01-XX | 4 | Updated `NmeaBatchReader` to accept optional registry |
| 2025-01-XX | 4 | Added `ReadCustomMessagesAsync()` for custom messages |
| 2025-01-XX | 4 | Added `CustomMessages` counter to `BatchReaderStatistics` |
| 2025-01-XX | 4 | Created 10 unit tests for registry and integration |
| 2025-01-XX | 4 | Phase 4 complete - all tests passing |
| 2025-01-XX | 5 | Added HP project reference to benchmark project |
| 2025-01-XX | 5 | Created `HpParsingBenchmarks.cs` for HP-only benchmarks |
| 2025-01-XX | 5 | Created `HpVsOriginalBenchmarks.cs` for side-by-side comparison |
| 2025-01-XX | 5 | Added v2 (DKW.NMEA) project reference for full historical comparison |
| 2025-01-XX | 5 | Created `V2VsV3VsHpBenchmarks.cs` for three-way comparison |
| 2025-01-XX | 5 | Ran benchmarks: v4 is 8x faster single-parse, 5x faster stream |
| 2025-01-XX | 5 | Achieved 0 B allocation for single message parsing |
| 2025-01-XX | 5 | Peak throughput: 5.4 million messages/second |
| 2025-01-XX | 5 | Updated blog post with validated benchmark results |
| 2025-01-XX | 5 | Phase 5 complete - all phases done! 🎉 |

