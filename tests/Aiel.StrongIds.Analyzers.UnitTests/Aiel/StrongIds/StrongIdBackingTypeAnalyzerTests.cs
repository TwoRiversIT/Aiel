using Aiel.StrongIds.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.StrongIds;

public sealed class StrongIdBackingTypeAnalyzerTests
{
    [Fact]
    public async Task GuidBackingType_ShouldNotReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public readonly partial record struct OrderId;
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Int32BackingType_ShouldNotReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<int>]
            public readonly partial record struct CounterId;
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Int64BackingType_ShouldNotReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<long>]
            public readonly partial record struct NumericId;
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task StringBackingType_ShouldNotReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<string>]
            public readonly partial record struct CorrelationId;
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DecimalBackingType_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<decimal>]
            public readonly partial record struct {|#0:PriceId|};
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00018")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DoubleBackingType_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<double>]
            public readonly partial record struct {|#0:PercentageId|};
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00018")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CustomTypeBackingType_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            public class CustomType { }

            [StrongId<CustomType>]
            public readonly partial record struct {|#0:CustomId|};
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdBackingTypeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00018")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
