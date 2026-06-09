using Aiel.StrongIds.Analyzers;
using Aiel.StrongIds.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.StrongIds;

public sealed class StrongIdShapeAnalyzerTests
{
    [Fact]
    public async Task ValidReadOnlyRecordStruct_ShouldNotReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public readonly partial record struct OrderId;
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidSealedRecord_ShouldNotReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public sealed partial record OrderId;
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(source);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonPartialRecordStruct_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public readonly record struct {|#0:OrderId|};
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00013")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonRecordClass_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public sealed partial class {|#0:OrderId|};
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00013")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PositionalRecordSyntax_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public readonly partial record struct {|#0:OrderId|}(System.Guid Value);
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00014")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeclaredValueMember_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public readonly partial record struct {|#0:OrderId|}
            {
                public System.Guid Value { get; }
            }
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00016")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeclaredInstanceConstructor_ShouldReportDiagnostic()
    {
        const String source = """
            using Aiel.StrongIds;

            [StrongId<System.Guid>]
            public readonly partial record struct {|#0:OrderId|}
            {
                public OrderId(System.Guid value)
                {
                }
            }
            """;

        var test = StrongIdAnalyzerVerifier<StrongIdShapeAnalyzer>.CreateTest(
            source,
            DiagnosticResult.CompilerError("AIEL00017")
                .WithLocation(0));

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
