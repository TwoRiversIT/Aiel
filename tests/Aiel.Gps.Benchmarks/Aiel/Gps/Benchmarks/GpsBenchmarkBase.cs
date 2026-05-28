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

using Aiel.Resources;

namespace Aiel.Gps.Benchmarks;

/// <summary>
/// Base class for GPS benchmarks providing common test data access.
/// </summary>
/// <remarks>
/// This class loads real-world test data from embedded resources and caches it in memory.
/// Benchmarks should use these cached byte arrays to avoid measuring I/O performance
/// instead of parsing performance.
/// </remarks>
public abstract class GpsBenchmarkBase
{
    /// <summary>
    /// Small dataset (~18KB, 357 lines, 343 parsed messages) containing all standard GPS message types.
    /// Use for: Quick iteration benchmarks, testing parser overhead.
    /// </summary>
    protected static readonly Byte[] Track3Data = LoadTestData("TestData.track3.nmea");

    /// <summary>
    /// Medium dataset (~295KB, 4,491 lines, 4,483 parsed messages) with mixed message types including custom GFDTA.
    /// Use for: Realistic workload benchmarks, testing parser diversity.
    /// </summary>
    protected static readonly Byte[] Track2Data = LoadTestData("TestData.track2.nmea");

    /// <summary>
    /// Large dataset (~920KB, 13,470 messages) with GGA, RMC, and GSA sentences.
    /// Use for: Throughput benchmarks, stress testing, memory allocation analysis.
    /// </summary>
    protected static readonly Byte[] Track1Data = LoadTestData("TestData.track1.nmea");

    /// <summary>
    /// GasFinder custom message dataset (~12KB, 735 lines, 121 parsed messages).
    /// Use for: Custom message type benchmarks, extension point performance.
    /// </summary>
    protected static readonly Byte[] GasFinderData = LoadTestData("TestData.gf.nmea");

    /// <summary>
    /// Synthetic dataset of 4,000 identical GLL sentences processed via the built-in dispatcher.
    /// Use for: Comparing built-in vs runtime registration performance on a level playing field.
    /// </summary>
    /// <remarks>
    /// All sentences use the standard $GPGLL identifier so they are handled by the source-generated
    /// dispatcher — zero allocation, no boxing, no dictionary lookup.
    /// </remarks>
    protected static readonly Byte[] SyntheticGllData = BuildSyntheticDataset("$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8, 4_000);

    /// <summary>
    /// Synthetic dataset of 4,000 identical sentences processed via runtime-registered parser.
    /// Use for: Comparing built-in vs runtime registration performance on a level playing field.
    /// </summary>
    /// <remarks>
    /// All sentences use the proprietary $RTGLL identifier (not known to the built-in dispatcher)
    /// so they are routed through <see cref="HP.NmeaParserRegistry"/> — one allocation per message,
    /// one <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/> lookup per sentence.
    /// </remarks>
    protected static readonly Byte[] SyntheticRtGllData = BuildSyntheticDataset("$RTGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8, 4_000);

    /// <summary>
    /// Pre-configured stream reader with all standard parsers registered.
    /// </summary>
    protected static NmeaStreamReader CreateStandardStreamReader()
    {
        return new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG());
    }

    /// <summary>
    /// Pre-configured stream reader with all parsers including custom GFDTA.
    /// </summary>
    protected static NmeaStreamReader CreateFullStreamReader()
    {
        return new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG(), new GFDTA());
    }

    private static Byte[] LoadTestData(String resourceName)
    {
        using var stream = RH.GetStream<RealWorldDataTests>(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Builds a synthetic dataset by repeating a single NMEA sentence a given number of times.
    /// </summary>
    /// <param name="sentence">The sentence to repeat (including CRLF terminator).</param>
    /// <param name="count">The number of times to repeat the sentence.</param>
    /// <returns>A byte array containing the repeated sentences.</returns>
    private static Byte[] BuildSyntheticDataset(ReadOnlySpan<Byte> sentence, Int32 count)
    {
        var result = new Byte[sentence.Length * count];

        for (var i = 0; i < count; i++)
        {
            sentence.CopyTo(result.AsSpan(i * sentence.Length));
        }

        return result;
    }

    /// <summary>
    /// Creates a new MemoryStream from the provided data.
    /// </summary>
    /// <remarks>
    /// We create new streams for each benchmark iteration because:
    /// 1. Streams maintain position state
    /// 2. Reusing streams would measure seek performance, not parse performance
    /// 3. Fresh streams ensure consistent starting conditions
    /// </remarks>
    protected static MemoryStream CreateStream(Byte[] data) => new(data);
}
