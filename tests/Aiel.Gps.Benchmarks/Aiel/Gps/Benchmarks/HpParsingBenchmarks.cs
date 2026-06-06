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
using System.Diagnostics.CodeAnalysis;
using HpGFDTA = Aiel.Gps.HP.Sentences.GFDTA;
using HpGfdtaParser = Aiel.Gps.HP.Sentences.GfdtaParser;
// Use explicit type names to avoid namespace conflicts
using HpGLL = Aiel.Gps.HP.Sentences.GLL;
using HpGllParser = Aiel.Gps.HP.Sentences.GllParser;
using HpNmeaBatchReader = Aiel.Gps.HP.NmeaBatchReader;
using HpNmeaMessage = Aiel.Gps.HP.NmeaMessage;
using HpNmeaSingleParser = Aiel.Gps.HP.NmeaSingleParser;

namespace Aiel.Gps.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Benchmarks for the high-performance (HP) NMEA parsing implementation.
/// </summary>
/// <remarks>
/// <para>
/// WHY WE BENCHMARK THIS:
/// - The HP library is designed for zero-allocation parsing
/// - We need to verify the design goals are met
/// - Compare against the original implementation
/// </para>
/// <para>
/// WHAT TO LOOK FOR IN RESULTS:
/// - Allocated: Should be 0 for single-message parsing (excluding string fields)
/// - Mean time: Should be competitive with or faster than original
/// - Gen0/Gen1/Gen2: Should be minimal
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class HpParsingBenchmarks : GpsBenchmarkBase
{
    // Pre-encoded test sentences as byte arrays
    private static readonly Byte[] GllSentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8.ToArray();
    private static readonly Byte[] GfdtaSentence = "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8.ToArray();

    /// <summary>
    /// Benchmarks parsing a single GLL sentence using the HP single parser.
    /// </summary>
    /// <remarks>
    /// This should be zero-allocation for the parsing itself.
    /// The GLL struct contains no reference types, so no allocations expected.
    /// </remarks>
    [Benchmark(Description = "HP: Single GLL Parse")]
    public HpGLL ParseSingleGll()
    {
        var parser = new HpGllParser();
        return HpNmeaSingleParser.Parse(GllSentence, parser);
    }

    /// <summary>
    /// Benchmarks parsing a single GFDTA sentence using the HP single parser.
    /// </summary>
    /// <remarks>
    /// GFDTA contains string fields, so there will be allocations for those.
    /// The parsing infrastructure itself should not allocate.
    /// </remarks>
    [Benchmark(Description = "HP: Single GFDTA Parse")]
    public HpGFDTA ParseSingleGfdta()
    {
        var parser = new HpGfdtaParser();
        return HpNmeaSingleParser.Parse(GfdtaSentence, parser);
    }

    /// <summary>
    /// Benchmarks parsing using the discriminated union TryParse.
    /// </summary>
    /// <remarks>
    /// This exercises the source-generated dispatcher that routes to the correct parser.
    /// Should still be zero-allocation for the union itself.
    /// </remarks>
    [Benchmark(Description = "HP: NmeaMessage.TryParse (GLL)")]
    [SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "It's a benchmark! Nobody cares!")]
    public HpNmeaMessage ParseMessageUnionGll()
    {
        HpNmeaMessage.TryParse(GllSentence, out var result);
        return result;
    }

    /// <summary>
    /// Benchmarks batch parsing of the small dataset.
    /// </summary>
    [Benchmark(Description = "HP: Batch Parse Small (343 messages)")]
    public async Task<Int32> BatchParseSmall()
    {
        await using var stream = CreateStream(Track3Data);
        var reader = new HpNmeaBatchReader(stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            // Prevent dead code elimination
            _ = message.Type;
        }

        return count;
    }

    /// <summary>
    /// Benchmarks batch parsing of the medium dataset.
    /// </summary>
    [Benchmark(Description = "HP: Batch Parse Medium (4,483 messages)")]
    public async Task<Int32> BatchParseMedium()
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

    /// <summary>
    /// Benchmarks batch parsing of the large dataset.
    /// </summary>
    [Benchmark(Description = "HP: Batch Parse Large (13,470 messages)")]
    public async Task<Int32> BatchParseLarge()
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
}
