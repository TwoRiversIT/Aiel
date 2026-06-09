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

using BenchmarkDotNet.Attributes;

namespace Aiel.Gps.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Benchmarks comparing different parser configurations.
/// </summary>
/// <remarks>
/// <para>
/// WHY WE BENCHMARK THIS:
/// - Shows the cost of registering additional parsers
/// - Helps users optimize for their specific message types
/// - Identifies if parser lookup is a bottleneck
/// </para>
/// <para>
/// WHAT TO LOOK FOR:
/// - Does registering more parsers slow down processing?
/// - Is the overhead per-parser or per-message?
/// - Should users register only the parsers they need?
/// </para>
/// <para>
/// INTERPRETATION:
/// If "SingleParser" is significantly faster than "AllParsers" on the same data,
/// it means parser lookup has measurable overhead. Users should only register
/// the message types they actually need.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ParserConfigurationBenchmarks : GpsBenchmarkBase
{
    /// <summary>
    /// Benchmarks parsing with only one parser registered (GGA).
    /// </summary>
    /// <remarks>
    /// This is the minimal configuration - only parsing one message type.
    /// Represents: A consumer that only cares about position fixes.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "Single Parser (GGA only)")]
    public async Task<Int32> SingleParser()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

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
    /// Benchmarks parsing with three common parsers registered.
    /// </summary>
    /// <remarks>
    /// Represents: A typical GPS consumer wanting position, velocity, and fix quality.
    /// </remarks>
    [Benchmark(Description = "Three Parsers (GGA, RMC, GSA)")]
    public async Task<Int32> ThreeParsers()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA());

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
    /// Benchmarks parsing with all standard parsers registered.
    /// </summary>
    /// <remarks>
    /// Represents: A comprehensive GPS consumer wanting all available data.
    /// </remarks>
    [Benchmark(Description = "All Standard Parsers (6 types)")]
    public async Task<Int32> AllStandardParsers()
    {
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
    /// Benchmarks parsing with all parsers including custom message types.
    /// </summary>
    /// <remarks>
    /// Represents: Maximum parser configuration with custom extensions.
    /// </remarks>
    [Benchmark(Description = "All Parsers Including Custom (7 types)")]
    public async Task<Int32> AllParsersIncludingCustom()
    {
        var streamReader = CreateFullStreamReader();

        await using var stream = CreateStream(Track3Data);
        await using var reader = new NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }
}

#pragma warning restore CA1822 // Mark members as static
