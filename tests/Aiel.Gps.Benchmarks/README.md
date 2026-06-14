# Aiel.Gps Benchmarks

This project contains performance benchmarks for the Aiel.Gps library using [BenchmarkDotNet](https://benchmarkdotnet.org/), the industry-standard .NET benchmarking framework.

## Why We Benchmark

> "Premature optimization is the root of all evil" - Donald Knuth

Benchmarking helps us:

1. **Identify bottlenecks** before they become problems
2. **Validate performance assumptions** with real data
3. **Prevent regressions** by tracking performance over time
4. **Make informed decisions** about implementation tradeoffs
5. **Set performance expectations** for library consumers

## What We Benchmark

### 1. Parsing Throughput (`ParsingThroughputBenchmarks`)

**Purpose**: Measure how fast the library can process GPS data of different sizes.

**Benchmarks**:

- Small Dataset (343 messages, ~18KB)
- Medium Dataset (4,483 messages, ~295KB)
- Large Dataset (13,470 messages, ~920KB)
- Manual Reading vs. IAsyncEnumerable

**What to Look For**:

- **Mean time**: How long it takes on average
- **Allocated memory**: Total bytes allocated (lower = less GC pressure)
- **Throughput**: Messages/second (calculate: MessageCount / MeanSeconds)
- **Scalability**: Does time scale linearly with message count?

**Example Output**:

```
|                       Method |       Mean |    Error |   StdDev | Allocated |
|----------------------------- |-----------:|---------:|---------:|----------:|
| Small Dataset (343 messages) |   1.234 ms | 0.012 ms | 0.011 ms |   45.2 KB |
| Medium Dataset (4,483 msg)   |  15.678 ms | 0.156 ms | 0.146 ms |  567.8 KB |
| Large Dataset (13,470 msg)   |  47.890 ms | 0.478 ms | 0.447 ms | 1789.3 KB |
```

**How to Interpret**:

- If Mean scales linearly with message count Ã¢â€ â€™ Good! O(n) performance
- If Allocated is constant per message Ã¢â€ â€™ Good! No memory leaks
- If small dataset shows high overhead Ã¢â€ â€™ Parser initialization cost

### 2. Parser Configuration (`ParserConfigurationBenchmarks`)

**Purpose**: Determine if registering more parsers affects performance.

**Benchmarks**:
- Single Parser (GGA only) - **Baseline**
- Three Parsers (GGA, RMC, GSA)
- All Standard Parsers (6 types)
- All Parsers Including Custom (7 types)

**What to Look For**:
- **Ratio column**: Compares to baseline (1.00 = same speed, 2.00 = twice as slow)
- **Memory impact**: Does more parsers = more allocations?

**Example Output**:
```
|                              Method |     Mean | Ratio | Allocated |
|------------------------------------ |---------:|------:|----------:|
| Single Parser (GGA only) [Baseline] | 1.234 ms |  1.00 |   45.2 KB |
| Three Parsers (GGA, RMC, GSA)       | 1.256 ms |  1.02 |   45.8 KB |
| All Standard Parsers (6 types)      | 1.289 ms |  1.04 |   46.1 KB |
```

**How to Interpret**:

- Ratio close to 1.00 Ã¢â€ â€™ Parser registration has minimal overhead
- Ratio > 1.20 Ã¢â€ â€™ Consider registering only needed parsers
- Allocated increases Ã¢â€ â€™ Each parser adds memory overhead

### 3. Individual Parser Performance (`IndividualParserBenchmarks`)

**Purpose**: Identify which message types are most expensive to parse.

**Benchmarks**: One for each message type (GGA, RMC, GSA, GSV, GLL, VTG, GFDTA)

**What to Look For**:

- **Relative performance**: Which parsers are slowest?
- **Memory per parse**: Does any parser allocate unexpectedly?
- **Complexity correlation**: Do complex messages take longer?

**Example Output**:

```
|                             Method |      Mean | Allocated |
|----------------------------------- |----------:|----------:|
| Parse GGA (Position Fix) [Baseline]|  1.234 ÃŽÂ¼s |     128 B |
| Parse RMC (Recommended Minimum)    |  1.456 ÃŽÂ¼s |     144 B |
| Parse GSV (Satellites in View)     |  2.789 ÃŽÂ¼s |     256 B |
```

**How to Interpret**:

- GSV slower than GGA Ã¢â€ â€™ Expected (more fields to parse)
- High allocation on simple parser Ã¢â€ â€™ Investigation needed
- Microsecond (ÃŽÂ¼s) times Ã¢â€ â€™ Very fast, good!

## Running Benchmarks

### Quick Start

```powershell
# Run all benchmarks
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks

# Run specific benchmark class
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks --filter *ParsingThroughput*

# Run specific method
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks --filter *ParseLargeDataset*
```

### Important: Always Use Release Configuration

```powershell
# Ã¢Å“â€¦ CORRECT
dotnet run -c Release

# Ã¢ÂÅ’ WRONG - Debug builds are 10-100x slower and not representative
dotnet run -c Debug
```

**Why?**

- Debug builds include extra checks and disable optimizations
- Release builds represent actual production performance
- BenchmarkDotNet will warn if you forget!

### Advanced Options

```powershell
# Export results to different formats
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks --exporters json html csv

# Run quick benchmarks (faster but less accurate)
dotnet run -c Release --project tests\Aiel.Gps.Benchmarks --job short
```

## Understanding the Results

### Key Metrics

| Metric | Description | What You Want |
|--------|-------------|---------------|
| **Mean** | Average execution time | Lower |
| **Error** | Standard error of the mean | Lower (indicates stability) |
| **StdDev** | Standard deviation | Lower (indicates consistency) |
| **Median** | Middle value | Close to Mean |
| **Ratio** | Relative to baseline | Close to 1.00 |
| **Allocated** | Total bytes allocated | Lower (less GC pressure) |
| **Gen0/1/2** | GC collections per 1000 ops | Lower (less GC) |

### Statistical Significance

BenchmarkDotNet runs each benchmark multiple times and uses statistical analysis to ensure results are reliable:

- **Outlier detection**: Identifies and reports anomalous runs
- **Warmup iterations**: Ensures JIT compilation doesn't skew results
- **Multiple runs**: Typically 15-100 iterations for accuracy
- **Statistical tests**: Uses Mann-Whitney U test for comparisons

### Red Flags Ã°Å¸Å¡Â©

Watch out for:

- **High StdDev**: Indicates unstable performance (investigate variability)
- **Increasing Allocated with dataset size**: Possible memory leak
- **Non-linear scaling**: O(nÃ‚Â²) behavior when O(n) expected
- **GC Gen2 collections**: Indicates large object heap pressure

## Best Practices for Benchmarking

### DO Ã¢Å“â€¦

1. **Run on idle system**: Close browsers, IDEs, and other apps
2. **Use Release builds**: Always `-c Release`
3. **Benchmark realistic scenarios**: Use actual data from your use case
4. **Isolate what you're measuring**: Don't include I/O in parsing benchmarks
5. **Compare apples to apples**: Same data, same conditions
6. **Track trends**: Run benchmarks regularly to catch regressions
7. **Use `[GlobalSetup]`**: For expensive initialization
8. **Return results**: Prevents dead code elimination

### DON'T Ã¢ÂÅ’

1. **Run on busy system**: Other processes affect timing
2. **Use Debug builds**: Not representative of production
3. **Benchmark trivial operations**: Overhead dominates results
4. **Change hardware between runs**: Results aren't comparable
5. **Ignore statistical significance**: High StdDev = unreliable
6. **Optimize without measuring**: Benchmarks show what's actually slow
7. **Assume micro-optimizations matter**: Profile first
8. **Forget to commit results**: Track performance over time

## Interpreting GPS-Specific Results

### Real-World Context

GPS devices typically output:

- **1 Hz**: Consumer GPS (1 message/second)
- **5 Hz**: High-end consumer GPS
- **10 Hz**: Professional GPS
- **20+ Hz**: Specialized high-frequency GPS

If your benchmarks show:

- **100+ messages/ms** Ã¢â€ â€™ Excellent! Can handle 100,000+ msg/sec
- **10-100 messages/ms** Ã¢â€ â€™ Good! Can handle 10,000-100,000 msg/sec
- **1-10 messages/ms** Ã¢â€ â€™ Adequate for most use cases
- **< 1 message/ms** Ã¢â€ â€™ May struggle with high-frequency GPS

### Throughput Calculation Example

```
Large Dataset Benchmark:
- Messages: 13,470
- Mean Time: 47.890 ms
- Throughput: 13,470 / 0.04789 = 281,303 messages/second

This is 281,303x faster than a 1Hz GPS device!
Even a 100Hz device is only 100 msg/sec, so we have 2,813x headroom.
```

### Memory Considerations

```
If Allocated = 1,789 KB for 13,470 messages:
- Per message: 1,789 KB / 13,470 = ~136 bytes
- For 1Hz GPS: 136 bytes/sec = negligible
- For 10Hz GPS: 1,360 bytes/sec = still negligible
- GC pressure: Minimal unless processing millions of messages
```

## When to Re-run Benchmarks

Run benchmarks:

1. **Before major refactoring**: Establish baseline
2. **After major refactoring**: Verify no regression
3. **Before release**: Ensure performance targets met
4. **When adding features**: Ensure no performance impact
5. **When investigating performance issues**: Identify bottlenecks
6. **Periodically (monthly/quarterly)**: Track trends

## Customizing Benchmarks

### Adding New Benchmarks

```csharp
[MemoryDiagnoser]  // Track memory allocations
public class MyCustomBenchmarks : GpsBenchmarkBase
{
    [Benchmark]  // Mark method as a benchmark
    public Int32 MyBenchmark()
    {
        // Your code here
        return result;
    }
}
```

### Comparing Implementations

```csharp
[Benchmark(Baseline = true)]  // Set as baseline
public void CurrentImplementation() { /* ... */ }

[Benchmark]  // Compare to baseline
public void NewImplementation() { /* ... */ }
```

### Parameterized Benchmarks

```csharp
[Params(10, 100, 1000)]  // Run with different values
public Int32 MessageCount { get; set; }

[Benchmark]
public void ProcessMessages()
{
    // Use this.MessageCount
}
```

## Further Reading

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/articles/overview.html)
- [BenchmarkDotNet Best Practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
- [.NET Performance Tips](https://docs.microsoft.com/en-us/dotnet/framework/performance/performance-tips)
- [Understanding .NET GC](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)

## Contributing Benchmarks

When adding new benchmarks:

1. Include educational comments explaining WHY and WHAT
2. Use descriptive method names and `[Benchmark(Description = "...")]`
3. Add to this README with interpretation guidance
4. Consider if it should use `[MemoryDiagnoser]`
5. Set a `Baseline` for comparison benchmarks
6. Use realistic data from the integration test project

---

**Remember**: The goal is not just to have fast code, but to **understand** performance characteristics and make **informed decisions** about tradeoffs.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
