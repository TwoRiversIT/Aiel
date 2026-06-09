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

using Aiel.Gps.HP;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using HpGFDTA = Aiel.Gps.HP.Sentences.GFDTA;
using HpGfdtaParser = Aiel.Gps.HP.Sentences.GfdtaParser;
using HpGLL = Aiel.Gps.HP.Sentences.GLL;
using HpGllParser = Aiel.Gps.HP.Sentences.GllParser;
using HpNmeaBatchReader = Aiel.Gps.HP.NmeaBatchReader;
using HpNmeaSingleParser = Aiel.Gps.HP.NmeaSingleParser;

namespace Aiel.Gps.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Benchmarks comparing the source-generated built-in dispatch path against the
/// runtime-registered custom parser path in Aiel.Gps.HP.
/// </summary>
/// <remarks>
/// <para>
/// WHY WE BENCHMARK THIS:
/// - Built-in parsers are compiled into a source-generated discriminated union — zero allocation, no dictionary lookup.
/// - Runtime parsers go through <see cref="NmeaParserRegistry"/> — one ConcurrentDictionary lookup and one boxing allocation per message.
/// - These benchmarks quantify exactly how much that difference costs.
/// </para>
/// <para>
/// WHAT TO LOOK FOR IN RESULTS:
/// - "Allocated" column: Built-in should show 0 B for GLL; runtime will show allocation for the boxed struct.
/// - "Mean" column: Runtime overhead comes from the registry lookup and the boxing, not from parsing itself.
/// - Category 3 ("Registry Presence"): Both rows should be nearly identical — confirms the registry is never
///   consulted for sentences already handled by the built-in dispatcher.
/// </para>
/// <para>HOW THE TWO PATHS DIFFER:</para>
/// <code>
/// // Built-in path (source-generated, in NmeaMessage.TryParse):
/// //   switch on identifier bytes → call GllParser.Parse() → return typed struct
///
/// // Runtime path (NmeaParserRegistry):
/// //   ConcurrentDictionary.TryGetValue() → call ICustomNmeaParser.Parse() → box struct as Object
/// </code>
/// </remarks>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class BuiltInVsRuntimeBenchmarks : GpsBenchmarkBase
{
    // ── Single-message test sentences ────────────────────────────────────────────
    // $GPGLL → handled by the source-generated dispatcher (built-in)
    // $RTGLL → unknown to the dispatcher, routed via NmeaParserRegistry (runtime)
    // Both sentences have identical payloads so the parsing work is equal;
    // the only difference is the dispatch mechanism.
    private static readonly Byte[] GllSentenceBytes = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8.ToArray();
    private static readonly Byte[] RtgllSentenceBytes = "$RTGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8.ToArray();
    private static readonly Byte[] GfdtaSentenceBytes = "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8.ToArray();
    private static readonly Byte[] RtdtaSentenceBytes = "$RTDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8.ToArray();

    // ── Pre-built parser and registry instances ──────────────────────────────────
    // Parsers and registries are stateless after construction and safe to reuse across iterations.
    private static readonly HpGllParser BuiltInGllParser = new();
    private static readonly HpGfdtaParser BuiltInGfdtaParser = new();
    private static readonly NmeaParserRegistry RuntimeGllRegistry = CreateRuntimeGllRegistry();
    private static readonly NmeaParserRegistry RuntimeGfdtaRegistry = CreateRuntimeGfdtaRegistry();

    private static NmeaParserRegistry CreateRuntimeGllRegistry()
    {
        var registry = new NmeaParserRegistry();
        registry.Register(new RuntimeGllParser());
        return registry;
    }

    private static NmeaParserRegistry CreateRuntimeGfdtaRegistry()
    {
        var registry = new NmeaParserRegistry();
        registry.Register(new RuntimeGfdtaParser());
        return registry;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Category 1: Single message — no string fields (GLL)
    //
    // GLL contains only numeric types and a Char — no heap allocations expected
    // for the built-in path. The runtime path adds exactly one allocation: boxing
    // the RuntimeGllMessage struct as Object.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Built-in path: source-generated dispatch to <see cref="HpGllParser"/>.
    /// Zero allocations expected.
    /// </summary>
    [BenchmarkCategory("Single: GLL (no strings)"), Benchmark(Baseline = true, Description = "Built-In")]
    public HpGLL BuiltIn_SingleGll()
    {
        return HpNmeaSingleParser.Parse(GllSentenceBytes, BuiltInGllParser);
    }

    /// <summary>
    /// Runtime path: <see cref="NmeaParserRegistry"/> lookup then <see cref="ICustomNmeaParser.Parse"/>.
    /// One allocation expected: boxing the parsed struct.
    /// </summary>
    [BenchmarkCategory("Single: GLL (no strings)"), Benchmark(Description = "Runtime")]
    public Object Runtime_SingleGll()
    {
        // Simulate the dispatch path used by NmeaBatchReader.TryParseCustom():
        //   1. Create Lexer positioned at the sentence start
        //   2. PeekIdentifier() — reads the identifier without advancing
        //   3. ConcurrentDictionary lookup to find the registered parser
        //   4. Parse() — returns a boxed struct (one allocation)
        var lexer = new Lexer(RtgllSentenceBytes);
        var identifier = lexer.PeekIdentifier();
        RuntimeGllRegistry.TryGetParser(identifier, out var parser);
        return parser!.Parse(ref lexer);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Category 2: Single message — with string fields (GFDTA)
    //
    // GFDTA contains two String fields (SerialNumber, Status). Both paths must
    // allocate for those strings. The runtime path adds a second allocation for
    // boxing on top of the unavoidable string allocations.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Built-in path: source-generated dispatch to <see cref="HpGfdtaParser"/>.
    /// Allocates only for String fields (unavoidable).
    /// </summary>
    [BenchmarkCategory("Single: GFDTA (with strings)"), Benchmark(Baseline = true, Description = "Built-In")]
    public HpGFDTA BuiltIn_SingleGfdta()
    {
        return HpNmeaSingleParser.Parse(GfdtaSentenceBytes, BuiltInGfdtaParser);
    }

    /// <summary>
    /// Runtime path: <see cref="NmeaParserRegistry"/> lookup then <see cref="ICustomNmeaParser.Parse"/>.
    /// Allocates for String fields plus one extra allocation for boxing.
    /// </summary>
    [BenchmarkCategory("Single: GFDTA (with strings)"), Benchmark(Description = "Runtime")]
    public Object Runtime_SingleGfdta()
    {
        var lexer = new Lexer(RtdtaSentenceBytes);
        var identifier = lexer.PeekIdentifier();
        RuntimeGfdtaRegistry.TryGetParser(identifier, out var parser);
        return parser!.Parse(ref lexer);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Category 3: Batch — does having a registry present affect built-in throughput?
    //
    // The built-in dispatcher is consulted first for every sentence. Only sentences
    // that are NOT recognised by the built-in dispatcher are forwarded to the registry.
    // These benchmarks confirm that registering custom parsers does not penalise
    // the hot path for built-in message types.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Baseline: 4,000 GLL sentences processed with no registry at all.
    /// </summary>
    [BenchmarkCategory("Batch: Registry Presence (4,000 GLL)"), Benchmark(Baseline = true, Description = "No Registry")]
    public async Task<Int32> Batch_BuiltIn_NoRegistry()
    {
        await using var stream = CreateStream(SyntheticGllData);
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
    /// Comparison: 4,000 GLL sentences processed with a populated registry.
    /// The registry is never consulted because all sentences are recognised by the
    /// built-in dispatcher. Result should be statistically identical to the baseline.
    /// </summary>
    [BenchmarkCategory("Batch: Registry Presence (4,000 GLL)"), Benchmark(Description = "With Registry")]
    public async Task<Int32> Batch_BuiltIn_WithRegistry()
    {
        await using var stream = CreateStream(SyntheticGllData);
        var reader = new HpNmeaBatchReader(stream, registry: RuntimeGllRegistry);

        var count = 0;
        await foreach (var message in reader.ReadAsync())
        {
            count++;
            _ = message.Type;
        }

        return count;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Category 4: Batch — built-in dispatch vs runtime registry dispatch
    //
    // The fairest possible apples-to-apples comparison:
    // - Both datasets contain 4,000 GLL-shaped sentences with identical payloads.
    // - $GPGLL is dispatched by the source-generated switch (zero allocation).
    // - $RTGLL misses the built-in dispatcher and hits the ConcurrentDictionary
    //   (one allocation per message for boxing).
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Baseline: 4,000 $GPGLL sentences via the source-generated built-in dispatcher.
    /// </summary>
    [BenchmarkCategory("Batch: Built-In vs Runtime (4,000 GLL)"), Benchmark(Baseline = true, Description = "Built-In")]
    public async Task<Int64> Batch_BuiltIn_SyntheticGll()
    {
        await using var stream = CreateStream(SyntheticGllData);
        var reader = new HpNmeaBatchReader(stream);

        await foreach (var _ in reader.ReadAsync())
        {
        }

        return reader.Statistics.ParsedMessages;
    }

    /// <summary>
    /// Comparison: 4,000 $RTGLL sentences via the <see cref="NmeaParserRegistry"/>.
    /// Custom messages are drained after <see cref="HpNmeaBatchReader.ReadAsync"/> completes
    /// so that all allocations are visible to the memory diagnoser.
    /// </summary>
    [BenchmarkCategory("Batch: Built-In vs Runtime (4,000 GLL)"), Benchmark(Description = "Runtime")]
    public async Task<Int64> Batch_Runtime_SyntheticRtGll()
    {
        await using var stream = CreateStream(SyntheticRtGllData);
        var reader = new HpNmeaBatchReader(stream, registry: RuntimeGllRegistry);

        // ReadAsync() drives the pipeline and writes custom messages to an unbounded channel.
        await foreach (var _ in reader.ReadAsync())
        {
        }

        // Drain the custom messages channel — the writer was completed by ReadAsync(),
        // so this loop will finish immediately after the last queued item.
        await foreach (var _ in reader.ReadCustomMessagesAsync())
        {
        }

        return reader.Statistics.CustomMessages;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Runtime message types (private to this class)
    //
    // These structs mirror the fields of their built-in counterparts so that the
    // parsing work is equivalent. They exist only for benchmarking purposes and
    // are intentionally NOT decorated with [NmeaMessage] — they are never part
    // of the source-generated discriminated union.
    // ─────────────────────────────────────────────────────────────────────────────

    private struct RuntimeGllMessage
    {
        public Double Latitude;
        public Double Longitude;
        public TimeOnly FixTime;
        public Char DataActive;
        public Int32 Checksum;
    }

    private struct RuntimeGfdtaMessage
    {
        public Double Concentration;
        public Int32 R2;
        public Double Distance;
        public Int32 Light;
        public DateTime DateTime;
        public String SerialNumber;
        public String Status;
        public Int32 Checksum;
    }

    /// <summary>
    /// Runtime parser for the synthetic $RTGLL sentence.
    /// Performs identical work to <see cref="HpGllParser"/> via the runtime registration path.
    /// </summary>
    private sealed class RuntimeGllParser : ICustomNmeaParser
    {
        public ReadOnlySpan<Byte> Identifier => "RTGLL"u8;

        public Object Parse(ref Lexer lexer)
        {
            lexer.ConsumeString(); // Skip the sentence identifier

            return new RuntimeGllMessage
            {
                Latitude = lexer.NextLatitude(),
                Longitude = lexer.NextLongitude(),
                FixTime = lexer.NextTime(),
                DataActive = lexer.NextChar(),
                Checksum = lexer.NextChecksum()
            };
        }
    }

    /// <summary>
    /// Runtime parser for the synthetic $RTDTA sentence.
    /// Performs identical work to <see cref="HpGfdtaParser"/> via the runtime registration path.
    /// </summary>
    private sealed class RuntimeGfdtaParser : ICustomNmeaParser
    {
        public ReadOnlySpan<Byte> Identifier => "RTDTA"u8;

        public Object Parse(ref Lexer lexer)
        {
            lexer.ConsumeString(); // Skip the sentence identifier

            return new RuntimeGfdtaMessage
            {
                Concentration = lexer.NextDouble(),
                R2 = lexer.NextInteger(),
                Distance = lexer.NextDouble(),
                Light = lexer.NextInteger(),
                DateTime = lexer.NextDateTime(),
                SerialNumber = lexer.NextString(),
                Status = lexer.NextString(),
                Checksum = lexer.NextChecksum()
            };
        }
    }
}
