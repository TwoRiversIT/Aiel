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
using System.Buffers;
using HpGLL = Aiel.Gps.HP.Sentences.GLL;
using HpGllParser = Aiel.Gps.HP.Sentences.GllParser;
using HpNmeaBatchReader = Aiel.Gps.HP.NmeaBatchReader;
using HpNmeaSingleParser = Aiel.Gps.HP.NmeaSingleParser;
// Aliases to disambiguate between original and HP implementations
using OriginalLexer = Aiel.Gps.Parsing.Lexer;

namespace Aiel.Gps.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Side-by-side comparison benchmarks between original Aiel.Gps and HP implementation.
/// </summary>
/// <remarks>
/// <para>
/// WHY WE BENCHMARK THIS:
/// - Direct comparison shows the performance improvement from the HP rewrite
/// - Helps identify if there are any regressions in the new implementation
/// - Validates the zero-allocation design goals
/// </para>
/// <para>
/// WHAT TO LOOK FOR IN RESULTS:
/// - Allocated column: HP should show 0 or near-zero for non-string messages
/// - Mean time: HP should be competitive or faster
/// - The ratio between Original and HP shows the improvement factor
/// </para>
/// <para>
/// HOW TO READ THE RESULTS:
/// - Lower Mean is better (faster)
/// - Lower Allocated is better (less GC pressure)
/// - "0 B" in Allocated means zero heap allocations
/// </para>
/// </remarks>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class HpVsOriginalBenchmarks : GpsBenchmarkBase
{
    // Test data
    private static readonly Byte[] GllSentenceBytes = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8.ToArray();
    private static readonly ReadOnlySequence<Byte> GllSentenceSequence = new(GllSentenceBytes);

    #region Single Message Parsing

    /// <summary>
    /// Original implementation: Parse a single GLL message.
    /// Uses the Lexer directly since NmeaReader requires a stream.
    /// </summary>
    [BenchmarkCategory("Single GLL"), Benchmark(Baseline = true, Description = "Original")]
    public GLL Original_ParseGll()
    {
        var lexer = new OriginalLexer(GllSentenceSequence);
        var parser = new GLL();
        return (GLL)parser.Parse(GllSentenceSequence);
    }

    /// <summary>
    /// HP implementation: Parse a single GLL message.
    /// Uses the NmeaSingleParser static method.
    /// </summary>
    [BenchmarkCategory("Single GLL"), Benchmark(Description = "HP")]
    public HpGLL Hp_ParseGll()
    {
        var parser = new HpGllParser();
        return HpNmeaSingleParser.Parse(GllSentenceBytes, parser);
    }

    #endregion

    #region Stream/Batch Parsing - Small Dataset

    /// <summary>
    /// Original implementation: Parse small dataset via NmeaReader.
    /// </summary>
    [BenchmarkCategory("Small Dataset (343 msgs)"), Benchmark(Baseline = true, Description = "Original")]
    public async Task<Int32> Original_ParseSmall()
    {
        LexerFactory.Create = (buffer) => new OriginalLexer(buffer);
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
    /// HP implementation: Parse small dataset via NmeaBatchReader.
    /// </summary>
    [BenchmarkCategory("Small Dataset (343 msgs)"), Benchmark(Description = "HP")]
    public async Task<Int32> Hp_ParseSmall()
    {
        await using var stream = CreateStream(Track3Data);
        var reader = new HpNmeaBatchReader(stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            _ = message.Type;
        }

        return count;
    }

    #endregion

    #region Stream/Batch Parsing - Medium Dataset

    /// <summary>
    /// Original implementation: Parse medium dataset.
    /// </summary>
    [BenchmarkCategory("Medium Dataset (4,483 msgs)"), Benchmark(Baseline = true, Description = "Original")]
    public async Task<Int32> Original_ParseMedium()
    {
        LexerFactory.Create = (buffer) => new OriginalLexer(buffer);
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
    /// HP implementation: Parse medium dataset.
    /// </summary>
    [BenchmarkCategory("Medium Dataset (4,483 msgs)"), Benchmark(Description = "HP")]
    public async Task<Int32> Hp_ParseMedium()
    {
        await using var stream = CreateStream(Track2Data);
        var reader = new HpNmeaBatchReader(stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            _ = message.Type;
        }

        return count;
    }

    #endregion

    #region Stream/Batch Parsing - Large Dataset

    /// <summary>
    /// Original implementation: Parse large dataset.
    /// </summary>
    [BenchmarkCategory("Large Dataset (13,470 msgs)"), Benchmark(Baseline = true, Description = "Original")]
    public async Task<Int32> Original_ParseLarge()
    {
        LexerFactory.Create = (buffer) => new OriginalLexer(buffer);
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
    /// HP implementation: Parse large dataset.
    /// </summary>
    [BenchmarkCategory("Large Dataset (13,470 msgs)"), Benchmark(Description = "HP")]
    public async Task<Int32> Hp_ParseLarge()
    {
        await using var stream = CreateStream(Track1Data);
        var reader = new HpNmeaBatchReader(stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            _ = message.Type;
        }

        return count;
    }

    #endregion
}
