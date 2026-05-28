# Quick Start: Running Your First Benchmark

This guide will walk you through running your first benchmark and understanding the results.

## Step 1: Run a Quick Benchmark

```powershell
# Navigate to the benchmark project directory
cd tests\Aiel.Gps.Benchmarks

# Run a quick "dry" benchmark (fast but less accurate)
dotnet run -c Release --framework net10.0 -- --filter *IndividualParser* --job dry
```

**What's happening:**
- `-c Release`: Builds with optimizations (REQUIRED!)
- `--framework net10.0`: Specifies which .NET version to use
- `--filter *IndividualParser*`: Runs only the IndividualParserBenchmarks class
- `--job dry`: Quick test run (1 iteration, useful for smoke testing)

## Step 2: Understanding the Output

You'll see output like this:

```
| Method                                  | Mean     | Ratio | Allocated |
|---------------------------------------- |---------:|------:|----------:|
| 'Parse GGA (Position Fix)'              | 1.960 ms |  1.00 |         - |
| 'Parse RMC (Recommended Minimum)'       | 2.056 ms |  1.05 |         - |
| 'Parse GFDTA (GasFinder Custom)'        | 8.410 ms |  4.29 |         - |
```

**Reading the Results:**
- **Method**: The benchmark that was run
- **Mean**: Average time per operation
  - `1.960 ms` = 1.96 milliseconds = 0.00196 seconds
  - This means it can parse ~510 GGA messages per second
- **Ratio**: Relative to baseline
  - `1.05` means RMC is 5% slower than GGA
  - `4.29` means GFDTA is 4.29x slower than GGA
- **Allocated**: Memory allocated per operation
  - `-` means BenchmarkDotNet needs more iterations to measure accurately

## Step 3: Run a Full Benchmark (Accurate Results)

```powershell
# Run WITHOUT --job dry for statistically significant results
dotnet run -c Release --framework net10.0 -- --filter *IndividualParser*
```

This will:
- Run each benchmark multiple times (typically 15-100 iterations)
- Calculate statistical measures (mean, median, standard deviation)
- Take several minutes to complete
- Provide highly accurate and reliable results

Expected output:

```
| Method                                  | Mean       | Error     | StdDev    | Allocated |
|---------------------------------------- |-----------:|----------:|----------:|----------:|
| 'Parse GGA (Position Fix)'              |   524.3 ns |   5.12 ns |   4.79 ns |     128 B |
| 'Parse RMC (Recommended Minimum)'       |   678.9 ns |   8.45 ns |   7.90 ns |     144 B |
```

**New Metrics:**
- **Error**: Half of 99.9% confidence interval (smaller = more reliable)
- **StdDev**: Standard deviation (smaller = more consistent)
- **ns**: Nanoseconds (1 ns = 0.000001 ms)
- **B**: Bytes allocated

## Step 4: Run All Benchmarks

```powershell
# Run everything (takes 10-20 minutes)
dotnet run -c Release --framework net10.0
```

## Step 5: Export Results

```powershell
# Export to multiple formats
dotnet run -c Release --framework net10.0 -- --exporters json html csv
```

Results will be saved to:
- `BenchmarkDotNet.Artifacts/results/` directory
- Open the `.html` file in your browser for a nice visual report

## Common Use Cases

### Compare Two Implementations

```csharp
[Benchmark(Baseline = true)]
public void OldImplementation() { /* ... */ }

[Benchmark]
public void NewImplementation() { /* ... */ }
```

Run and compare the Ratio column to see if NewImplementation is faster.

### Test Different Data Sizes

```powershell
# Run throughput benchmarks with real-world data
dotnet run -c Release --framework net10.0 -- --filter *Throughput*
```

### Quick Iteration During Development

```powershell
# Fast feedback loop
dotnet run -c Release --framework net10.0 -- --filter *YourBenchmark* --job dry
```

## Troubleshooting

### Error: "Build failed" or running slowly
**Solution**: Make sure you're using `-c Release`. Debug builds are not representative.

### Error: "Your project targets multiple frameworks"
**Solution**: Add `--framework net10.0` to specify which .NET version.

### Warning: "The minimum observed iteration time is very small"
**Solution**: This is normal with `--job dry`. For accurate results, remove `--job dry`.

### Results vary widely between runs
**Solution**: 
- Close other applications (browser, IDE, etc.)
- Run on an idle system
- Let the full benchmark run (don't use `--job dry`)
- Check if antivirus is scanning files

## Next Steps

1. Read the full [README.md](README.md) for detailed explanations
2. Run benchmarks before making performance changes
3. Run benchmarks after making changes to verify improvement
4. Compare results to see if your optimization worked
5. Check both Mean (speed) and Allocated (memory) columns

## Example Workflow: Optimizing a Parser

```powershell
# 1. Establish baseline
dotnet run -c Release --framework net10.0 -- --filter *ParseGGA* > before.txt

# 2. Make your code changes

# 3. Run benchmarks again
dotnet run -c Release --framework net10.0 -- --filter *ParseGGA* > after.txt

# 4. Compare results
# Look for:
# - Lower Mean time (faster)
# - Lower Allocated (less memory)
# - Lower Error/StdDev (more consistent)
```

---

**Remember**: Always benchmark in Release mode on an idle system for accurate results!
