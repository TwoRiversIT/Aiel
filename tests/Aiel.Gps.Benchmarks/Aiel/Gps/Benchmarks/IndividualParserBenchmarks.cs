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
using System.Text;

namespace Aiel.Gps.Benchmarks;

/// <summary>
/// Benchmarks for individual message type parsers.
/// </summary>
/// <remarks>
/// <para>
/// WHY WE BENCHMARK INDIVIDUAL PARSERS:
/// - Identifies which message types are expensive to parse
/// - Helps optimize hot paths (e.g., if GGA is 10x slower than RMC, focus there)
/// - Validates that parser complexity matches message complexity
/// </para>
/// <para>
/// WHAT TO LOOK FOR:
/// - Which parsers allocate the most memory?
/// - Which parsers are slowest?
/// - Is the performance difference justified by message complexity?
/// </para>
/// <para>
/// EXPECTED RESULTS:
/// - Simple messages (RMC) should be faster than complex ones (GSV with satellite data)
/// - Memory allocations should be minimal (ideally zero per parse)
/// - Parsers should scale linearly with message complexity
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class IndividualParserBenchmarks
{
    private ReadOnlySequence<Byte> _ggaSentence;
    private ReadOnlySequence<Byte> _rmcSentence;
    private ReadOnlySequence<Byte> _gsaSentence;
    private ReadOnlySequence<Byte> _gsvSentence;
    private ReadOnlySequence<Byte> _gllSentence;
    private ReadOnlySequence<Byte> _vtgSentence;
    private ReadOnlySequence<Byte> _gfdtaSentence;

    private GGA _ggaParser = default!;
    private RMC _rmcParser = default!;
    private GSA _gsaParser = default!;
    private GSV _gsvParser = default!;
    private GLL _gllParser = default!;
    private VTG _vtgParser = default!;
    private GFDTA _gfdtaParser = default!;

    /// <summary>
    /// Setup method called once before benchmarks run.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WHY WE USE GLOBALSETUP:
    /// - Expensive setup operations should not be measured as part of the benchmark
    /// - We want to measure parsing time, not sentence preparation time
    /// - Parsers are typically instantiated once and reused
    /// </para>
    /// <para>
    /// We use realistic NMEA sentences from actual GPS devices to ensure
    /// benchmarks reflect real-world performance.
    /// </para>
    /// </remarks>
    [GlobalSetup]
    public void Setup()
    {
        _ggaParser = new GGA();
        _rmcParser = new RMC();
        _gsaParser = new GSA();
        _gsvParser = new GSV();
        _gllParser = new GLL();
        _vtgParser = new VTG();
        _gfdtaParser = new GFDTA();

        _ggaSentence = CreateSequence("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62");
        _rmcSentence = CreateSequence("$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62");
        _gsaSentence = CreateSequence("$GPGSA,A,3,01,02,03,04,05,06,07,08,09,10,11,12,1.0,0.5,0.9*30");
        _gsvSentence = CreateSequence("$GPGSV,3,1,12,01,45,123,45,02,30,234,42,03,15,345,38,04,60,056,50*7B");
        _gllSentence = CreateSequence("$GPGLL,5057.1975,N,11134.8332,W,232608.000,A,D*7C");
        _vtgSentence = CreateSequence("$GPVTG,140.0,T,130.0,M,5.5,N,10.2,K,D*2A");
        _gfdtaSentence = CreateSequence("$GFDTA,     7.2,98,3.0,16380,2019/01/22 16:04:58, CH4AB-1047,1*40");
    }

    [Benchmark(Baseline = true, Description = "Parse GGA (Position Fix)")]
    public NmeaMessage ParseGGA() => _ggaParser.Parse(_ggaSentence);

    [Benchmark(Description = "Parse RMC (Recommended Minimum)")]
    public NmeaMessage ParseRMC() => _rmcParser.Parse(_rmcSentence);

    [Benchmark(Description = "Parse GSA (DOP and Active Satellites)")]
    public NmeaMessage ParseGSA() => _gsaParser.Parse(_gsaSentence);

    [Benchmark(Description = "Parse GSV (Satellites in View)")]
    public NmeaMessage ParseGSV() => _gsvParser.Parse(_gsvSentence);

    [Benchmark(Description = "Parse GLL (Geographic Position)")]
    public NmeaMessage ParseGLL() => _gllParser.Parse(_gllSentence);

    [Benchmark(Description = "Parse VTG (Track and Speed)")]
    public NmeaMessage ParseVTG() => _vtgParser.Parse(_vtgSentence);

    [Benchmark(Description = "Parse GFDTA (GasFinder Custom)")]
    public NmeaMessage ParseGFDTA() => _gfdtaParser.Parse(_gfdtaSentence);

    private static ReadOnlySequence<Byte> CreateSequence(String sentence)
    {
        var bytes = Encoding.UTF8.GetBytes(sentence);
        return new ReadOnlySequence<Byte>(bytes);
    }
}
