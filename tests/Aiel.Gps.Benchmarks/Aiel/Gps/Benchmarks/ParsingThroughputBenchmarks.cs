// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Aiel.Gps.Parsing;
using BenchmarkDotNet.Attributes;

namespace Aiel.Gps.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Benchmarks for measuring NmeaReader throughput with different data sizes.
/// </summary>
/// <remarks>
/// <para>
/// WHY WE BENCHMARK THIS:
/// - Parsing throughput is the primary performance characteristic of a GPS library
/// - Real-world GPS devices generate 1-10 messages/second, so we need to stay well ahead
/// - We want to ensure the library can handle burst scenarios and historical data playback
/// </para>
/// <para>
/// WHAT TO LOOK FOR IN RESULTS:
/// - Mean time: Average time to process the dataset
/// - Allocated: Total memory allocated (lower is better, indicates less GC pressure)
/// - Throughput: Can be calculated as (MessageCount / Mean time in seconds)
/// - Scalability: Does time scale linearly with data size?
/// </para>
/// <para>
/// BEST PRACTICES:
/// - [MemoryDiagnoser]: Tracks allocations to identify memory hotspots
/// - Use async/await patterns as they appear in production code
/// - Each benchmark should represent a realistic usage pattern
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ParsingThroughputBenchmarks : GpsBenchmarkBase
{
    /// <summary>
    /// Benchmarks parsing a small dataset (343 messages, ~18KB).
    /// </summary>
    /// <remarks>
    /// This represents a few seconds of GPS data at typical rates.
    /// Good for: Measuring parser overhead and startup costs.
    /// </remarks>
    [Benchmark(Description = "Small Dataset (343 messages)")]
    public async Task<Int32> ParseSmallDataset()
    {
        LexerFactory.Create = (buffer) => new Lexer(buffer);
        var streamReader = CreateStandardStreamReader();
        await using var stream = CreateStream(Track3Data);
        await using var reader = new NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Benchmarks parsing a medium dataset (4,483 messages, ~295KB).
    /// </summary>
    /// <remarks>
    /// This represents several minutes of GPS data with mixed message types.
    /// Good for: Measuring realistic workload performance.
    /// </remarks>
    [Benchmark(Description = "Medium Dataset (4,483 messages)")]
    public async Task<Int32> ParseMediumDataset()
    {
        LexerFactory.Create = (buffer) => new Lexer(buffer);

        var streamReader = CreateFullStreamReader();
        await using var stream = CreateStream(Track2Data);
        await using var reader = new NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Benchmarks parsing a large dataset (13,470 messages, ~920KB).
    /// </summary>
    /// <remarks>
    /// This represents hours of GPS data or historical playback scenarios.
    /// Good for: Stress testing, identifying memory issues, measuring sustained throughput.
    /// </remarks>
    [Benchmark(Description = "Large Dataset (13,470 messages)")]
    public async Task<Int32> ParseLargeDataset()
    {
        LexerFactory.Create = (buffer) => new Lexer(buffer);

        var streamReader = CreateStandardStreamReader();
        await using var stream = CreateStream(Track1Data);
        await using var reader = new NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Benchmarks manual message reading using ReadNextAsync instead of IAsyncEnumerable.
    /// </summary>
    /// <remarks>
    /// WHY BENCHMARK BOTH APIS:
    /// - Some consumers prefer manual control over foreach
    /// - We want to ensure both patterns have similar performance
    /// - Helps identify if IAsyncEnumerable adds overhead
    /// </remarks>
    [Benchmark(Description = "Manual Reading (ReadNextAsync)")]
    public async Task<Int32> ParseWithManualReading()
    {
        LexerFactory.Create = (buffer) => new Lexer(buffer);

        var streamReader = CreateStandardStreamReader();
        await using var stream = CreateStream(Track3Data);
        await using var reader = new NmeaReader(streamReader, stream);

        var count = 0;
        while (await reader.ReadNextAsync(CancellationToken.None) != null)
        {
            count++;
        }

        return count;
    }
}

#pragma warning restore CA1822 // Mark members as static
