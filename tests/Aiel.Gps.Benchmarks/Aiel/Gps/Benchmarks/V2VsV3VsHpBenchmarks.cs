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

using V3GGA = Aiel.Gps.GGA;
using V3GLL = Aiel.Gps.GLL;
using V3GSA = Aiel.Gps.GSA;
using V3GSV = Aiel.Gps.GSV;
using V3Lexer = Aiel.Gps.Parsing.Lexer;
using V3LexerFactory = Aiel.Gps.LexerFactory;
using V3NmeaReader = Aiel.Gps.NmeaReader;
using V3RMC = Aiel.Gps.RMC;
// v3: Aiel.Gps implementation (2025 refresh of v2)
using V3StreamReader = Aiel.Gps.NmeaStreamReader;
using V3VTG = Aiel.Gps.VTG;
// v4: Aiel.Gps.HP (High-Performance, zero-allocation)
using V4BatchReader = Aiel.Gps.HP.NmeaBatchReader;
using V4GLL = Aiel.Gps.HP.Sentences.GLL;
using V4GllParser = Aiel.Gps.HP.Sentences.GllParser;
using V4SingleParser = Aiel.Gps.HP.NmeaSingleParser;

namespace Aiel.Gps.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Head-to-head comparison of all three parser versions.
/// </summary>
/// <remarks>
/// <para>
/// VERSION HISTORY:
/// - v2 (DKW.NMEA): Original 2018-2019 implementation using pipelines and ReadOnlySequence
/// - v3 (Aiel.Gps): 2025 refresh with bug fixes and .NET 10 update
/// - v4 (Aiel.Gps.HP): High-performance rewrite with zero allocations
/// </para>
/// <para>
/// WHAT TO LOOK FOR:
/// - Allocated: v4 should show 0 B for single-message parsing
/// - Mean: v4 should be faster due to less GC pressure
/// - The journey from v2 → v3 → v4 shows the evolution
/// </para>
/// </remarks>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class V2VsV3VsHpBenchmarks : GpsBenchmarkBase
{
    // Test data - same sentence for all versions
    private static readonly Byte[] GllSentenceBytes = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8.ToArray();
    private static readonly ReadOnlySequence<Byte> GllSentenceSequence = new(GllSentenceBytes);

    #region Single Message Parsing

    /// <summary>
    /// v3: Aiel.Gps single message parsing
    /// </summary>
    [BenchmarkCategory("Single GLL"), Benchmark(Description = "v3 (Aiel)")]
    public V3GLL V3_ParseGll()
    {
        var parser = new V3GLL();
        return (V3GLL)parser.Parse(GllSentenceSequence);
    }

    /// <summary>
    /// v4: Aiel.Gps.HP single message parsing (zero-alloc)
    /// </summary>
    [BenchmarkCategory("Single GLL"), Benchmark(Description = "v4 (HP)")]
    public V4GLL V4_ParseGll()
    {
        var parser = new V4GllParser();
        return V4SingleParser.Parse(GllSentenceBytes, parser);
    }

    #endregion

    #region Stream Parsing - Small Dataset (343 messages)

    /// <summary>
    /// v3: Aiel.Gps stream parsing
    /// </summary>
    [BenchmarkCategory("Small (343 msgs)"), Benchmark(Description = "v3 (Aiel)")]
    public async Task<Int32> V3_ParseSmall()
    {
        V3LexerFactory.Create = (buffer) => new V3Lexer(buffer);
        var streamReader = new V3StreamReader()
            .Register(new V3GGA(), new V3RMC(), new V3GSA(), new V3GSV(), new V3GLL(), new V3VTG());

        await using var stream = CreateStream(Track3Data);
        await using var reader = new V3NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// v4: Aiel.Gps.HP batch parsing (zero-alloc union)
    /// </summary>
    [BenchmarkCategory("Small (343 msgs)"), Benchmark(Description = "v4 (HP)")]
    public async Task<Int32> V4_ParseSmall()
    {
        await using var stream = CreateStream(Track3Data);
        var reader = new V4BatchReader(stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            _ = message.Type; // Prevent dead code elimination
        }

        return count;
    }

    #endregion

    #region Stream Parsing - Medium Dataset (4,483 messages)

    /// <summary>
    /// v3: Aiel.Gps stream parsing
    /// </summary>
    [BenchmarkCategory("Medium (4,483 msgs)"), Benchmark(Description = "v3 (Aiel)")]
    public async Task<Int32> V3_ParseMedium()
    {
        V3LexerFactory.Create = (buffer) => new V3Lexer(buffer);
        var streamReader = new V3StreamReader()
            .Register(new V3GGA(), new V3RMC(), new V3GSA(), new V3GSV(), new V3GLL(), new V3VTG());

        await using var stream = CreateStream(Track2Data);
        await using var reader = new V3NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// v4: Aiel.Gps.HP batch parsing (zero-alloc union)
    /// </summary>
    [BenchmarkCategory("Medium (4,483 msgs)"), Benchmark(Description = "v4 (HP)")]
    public async Task<Int32> V4_ParseMedium()
    {
        await using var stream = CreateStream(Track2Data);
        var reader = new V4BatchReader(stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            _ = message.Type;
        }

        return count;
    }

    #endregion

    #region Stream Parsing - Large Dataset (13,470 messages)

    /// <summary>
    /// v3: Aiel.Gps stream parsing
    /// </summary>
    [BenchmarkCategory("Large (13,470 msgs)"), Benchmark(Description = "v3 (Aiel)")]
    public async Task<Int32> V3_ParseLarge()
    {
        V3LexerFactory.Create = (buffer) => new V3Lexer(buffer);
        var streamReader = new V3StreamReader()
            .Register(new V3GGA(), new V3RMC(), new V3GSA(), new V3GSV(), new V3GLL(), new V3VTG());

        await using var stream = CreateStream(Track1Data);
        await using var reader = new V3NmeaReader(streamReader, stream);

        var count = 0;
        await foreach (var message in reader.ReadAsync(CancellationToken.None))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// v4: Aiel.Gps.HP batch parsing (zero-alloc union)
    /// </summary>
    [BenchmarkCategory("Large (13,470 msgs)"), Benchmark(Description = "v4 (HP)")]
    public async Task<Int32> V4_ParseLarge()
    {
        await using var stream = CreateStream(Track1Data);
        var reader = new V4BatchReader(stream);

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
