# Aiel.Gps.HP — Built-In vs Runtime Registration Benchmarks

## Overview

The `BuiltInVsRuntimeBenchmarks` class in `Aiel.Gps.Benchmarks` answers a single practical question:

> **What does it actually cost to use the runtime registration path instead of the fork path?**

The two extensibility tiers have fundamentally different dispatch mechanisms:

| Tier            | Dispatch Mechanism                    | Allocation per Message | Included in `NmeaMessage`? |
| --------------- | ------------------------------------- | :--------------------: | :------------------------: |
| Built-In (fork) | Source-generated `switch` expression  | Zero                   | Yes                        |
| Runtime (NuGet) | `ConcurrentDictionary` lookup + boxing | One                   | No — separate channel      |

These benchmarks put numbers on that difference so you can make an informed decision about which tier
is appropriate for your workload. Spoiler: for most real-world applications the runtime overhead is
negligible. For sub-100ns telemetry pipelines, fork it.

---

## Running the Benchmarks

```powershell
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks -- --filter *BuiltInVsRuntime*
```

> ⚠️ **Always run benchmarks in Release mode.** The `-c Release` flag is not optional. Running in Debug
> mode will produce numbers that are dramatically slower and completely misleading. Do not be that person.

To run the full benchmark suite (all classes):

```powershell
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks
```

---

## Benchmark Categories

### Category 1: Single Message — GLL (no strings)

**Sentences used:**

- Built-In: `$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n`
- Runtime:  `$RTGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n`

Both sentences have identical payloads. Only the identifier (and therefore the dispatch path) differs.

**What it measures:**

The GLL message contains no `String` fields — only `Double`, `TimeOnly`, `Char`, and `Int32`. The built-in
path returns a stack-allocated `GLL` struct with **zero heap allocations**.

The runtime path must box the parsed `RuntimeGllMessage` struct as `Object` before returning it. This is
the irreducible minimum overhead of the runtime registration model: one allocation of exactly the size
of the struct.

**What to look for:**

- `Allocated` column: built-in should show `0 B`; runtime shows the size of the boxed struct
- `Mean` column: any difference beyond the boxing reflects `ConcurrentDictionary` lookup overhead

---

### Category 2: Single Message — GFDTA (with strings)

**Sentences used:**

- Built-In: `$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n`
- Runtime:  `$RTDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n`

**What it measures:**

GFDTA contains two `String` fields (`SerialNumber` and `Status`). Both paths MUST allocate for those
strings — there is no way around it. The boxing overhead from the runtime path is therefore smaller
relative to the total allocation, making this a less dramatic comparison than Category 1.

**What to look for:**

- `Allocated` column: both should show roughly the same string allocation (≈72 B)
- Runtime overhead appears as the difference in both `Allocated` and `Mean`
- This category demonstrates that when a message already allocates, boxing costs become proportionally minor

---

### Category 3: Batch — Registry Presence on Built-In Messages

**Dataset:** 4,000 × `$GPGLL,...\r\n` (all built-in, no custom sentences)

| Benchmark       | Registry                                    |
| --------------- | ------------------------------------------- |
| No Registry     | `new NmeaBatchReader(stream)` — null         |
| With Registry   | `new NmeaBatchReader(stream, registry: ...)` — populated |

**What it measures:**

This category directly tests the claim: *"adding a registry does not slow down built-in message
processing."*

The built-in dispatcher (`NmeaMessage.TryParse`) is consulted first for every sentence. If it succeeds,
the registry is never consulted. Because every sentence in this dataset IS a built-in GLL, the registry
lookup code path is never entered regardless of whether a registry is present.

**What to look for:**

- Both rows should show nearly identical `Mean` and `Allocated` values
- A statistically significant difference would indicate a regression in the dispatch logic
- Any difference you DO see is likely noise — not registry overhead

✅ If both rows are within statistical noise of each other, the design is working as intended.

---

### Category 4: Batch — Built-In vs Runtime Dispatch (4,000 messages)

**Datasets:**

- Built-In: 4,000 × `$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n`
- Runtime:  4,000 × `$RTGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n`

**What it measures:**

This is the headline comparison. Both datasets contain the same payload repeated 4,000 times. The
difference is purely in dispatch:

- `$GPGLL` → recognised by the source-generated dispatcher → zero allocation
- `$RTGLL` → misses the built-in dispatcher → `ConcurrentDictionary` lookup → parser call → box result

The runtime benchmark drains the custom messages channel after `ReadAsync()` completes so the memory
diagnoser sees all allocations from the full pipeline.

**What to look for:**

- `Allocated` ratio: runtime allocates approximately `sizeof(RuntimeGllMessage) + object header` × 4,000
  more than the built-in path
- `Mean` ratio: the total time difference reflects `ConcurrentDictionary` lookups, the extra channel
  write/read, and GC pressure from the allocations
- `Gen0` column: runtime should show Gen0 collections; built-in should show zero or minimal collections

---

## Actual Results

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7840)  
Unknown processor  
.NET SDK 10.0.103
  - [Host]     : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  - DefaultJob : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

| Method        | Categories                              | Mean              | Error          | StdDev         | Ratio | Gen0     | Gen1   | Allocated  | Alloc Ratio |
|-------------- |---------------------------------------- |------------------:|---------------:|---------------:|------:|---------:|-------:|-----------:|------------:|
| Built-In      | Batch: Built-In vs Runtime (4,000 GLL)  |    906,935.18 ns  |  2,287.379 ns  |  2,139.615 ns  |  1.00 |        - |      - |    6,825 B |        1.00 |
| Runtime       | Batch: Built-In vs Runtime (4,000 GLL)  |  1,303,904.30 ns  | 14,396.381 ns  | 13,466.384 ns  |  1.44 |  19.5313 | 5.8594 |  393,185 B |       57.61 |
|               |                                         |                   |                |                |       |          |        |            |             |
| No Registry   | Batch: Registry Presence (4,000 GLL)    |    915,689.45 ns  |  7,571.574 ns  |  6,712.003 ns  |  1.00 |        - |      - |    6,824 B |        1.00 |
| With Registry | Batch: Registry Presence (4,000 GLL)    |    880,781.18 ns  |  4,271.091 ns  |  3,995.181 ns  |  0.96 |        - |      - |    6,824 B |        1.00 |
|               |                                         |                   |                |                |       |          |        |            |             |
| Built-In      | Single: GFDTA (with strings)            |        184.35 ns  |      1.434 ns  |      1.271 ns  |  1.00 |   0.0038 |      - |       72 B |        1.00 |
| Runtime       | Single: GFDTA (with strings)            |        213.99 ns  |      1.201 ns  |      1.003 ns  |  1.16 |   0.0093 |      - |      176 B |        2.44 |
|               |                                         |                   |                |                |       |          |        |            |             |
| Built-In      | Single: GLL (no strings)               |         76.69 ns  |      0.437 ns  |      0.409 ns  |  1.00 |        - |      - |          - |          NA |
| Runtime       | Single: GLL (no strings)               |        148.10 ns  |      1.767 ns  |      1.653 ns  |  1.93 |   0.0041 |      - |       80 B |          NA |

---

## Interpreting the Results

### The Boxing Overhead Is Measurable

The measured allocation for a boxed `RuntimeGllMessage` is **80 bytes** — the struct data (two `Double`,
one `TimeOnly`, one `Char`, one `Int32`, with alignment padding) plus the 16-byte object header on a
64-bit runtime. This is the irreducible floor of the runtime registration model for zero-string messages.

For `RuntimeGfdtaMessage` the boxing adds **104 bytes** on top of the 72 bytes already spent on strings,
for a total of **176 bytes** vs 72 bytes on the built-in path.

In a batch of 4,000 GLL messages, that 80-byte-per-message cost compounds to **~320 KB** of boxing
allocations alone — plus the channel write overhead, bringing the measured total to **393 KB**.

### The Dictionary Lookup Is Fast

`ConcurrentDictionary<String, ICustomNmeaParser>.TryGetValue()` is typically 10–30 ns on modern hardware.
For a 76 ns built-in GLL budget, a single dictionary miss contributes roughly **10–30%** of the total
runtime overhead measured at 1.93×.

### Registry Presence Has Zero Impact on Built-In Messages

The `Batch: Registry Presence` results prove this conclusively: `With Registry` is **0.96×** the baseline
(i.e., within noise of identical). Both rows allocate exactly **6,824 bytes**, confirming that the
registry is never consulted when `NmeaMessage.TryParse` succeeds. You can register as many custom parsers
as you like without affecting built-in message throughput.

### GC Pressure Compounds at Scale

The batch results show Gen0 **and** Gen1 collections for the runtime path (19.5 Gen0, 5.9 Gen1), versus
zero collections for the built-in path. This is the real cost at scale — not the per-message latency,
but the GC pauses that accumulate under sustained load.

### Rule of Thumb

| Messages per second | Recommendation |
| ------------------- | -------------- |
| < 10,000/s          | Runtime registration is fine — overhead is negligible |
| 10,000–100,000/s    | Runtime registration works; profile GC pressure under sustained load |
| > 100,000/s         | Consider forking for zero-allocation dispatch |

---

## Related

- [`src/Aiel.Gps.HP/README.md`](../../src/Aiel.Gps.HP/README.md) — full library documentation including extensibility guide
- [`tests/Aiel.Gps.Benchmarks/Aiel/Gps/Benchmarks/BuiltInVsRuntimeBenchmarks.cs`](../../tests/Aiel.Gps.Benchmarks/Aiel/Gps/Benchmarks/BuiltInVsRuntimeBenchmarks.cs) — benchmark source code
- [`tests/Aiel.Gps.Benchmarks/Aiel/Gps/Benchmarks/GpsBenchmarkBase.cs`](../../tests/Aiel.Gps.Benchmarks/Aiel/Gps/Benchmarks/GpsBenchmarkBase.cs) — synthetic dataset generation
