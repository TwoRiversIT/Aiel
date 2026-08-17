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

using Aiel.Authorization.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Authorization.Generators;

public class PermissionDefinitionSourceGeneratorTests
{
    private const String ActionSource = """
        using Aiel.Authorization;

        namespace Sample;

        [AuthorizationDefinition(
            "scheduling.RescheduleAppointment",
            "Location",
            "User",
            "Reschedule Appointment")]
        public class RescheduleAppointment : global::Aiel.Actions.IAction { }
        """;

    [Fact]
    public void EmitsCheckerClass_ForDecoratedAction()
    {
        var result = RunGenerator(ActionSource);

        var checkerTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("PermissionChecker"));
        checkerTree.Should().NotBeNull();
        var text = checkerTree.GetText(TestContext.Current.CancellationToken).ToString();
        text.Should().Contain("RescheduleAppointmentPermissionChecker");
        text.Should().Contain("IActionAuthorizationChecker<");
    }

    [Fact]
    public void EmitsPermissionNameConstant_ForDecoratedAction()
    {
        var result = RunGenerator(ActionSource);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        aggregateTree.Should().NotBeNull();
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        text.Should().Contain("GeneratedAuthorizationNames");
        text.Should().Contain("scheduling.RescheduleAppointment");
    }

    [Fact]
    public void EmitsGetManifestsMethod_ForDecoratedAction()
    {
        var result = RunGenerator(ActionSource);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        aggregateTree.Should().NotBeNull();
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        text.Should().Contain("GeneratedAuthorizationManifests");
        text.Should().Contain("GetManifests()");
    }

    [Fact]
    public void EmitsManifestMetadata_ForActionTypeLifecycleAndPreviousNames()
    {
        const String source = """
            using Aiel.Authorization;

            namespace Sample;

            [AuthorizationDefinition(
                "scheduling.RescheduleAppointment",
                "Location",
                "User",
                "Reschedule Appointment",
                Lifecycle = PermissionLifecycle.Deprecated,
                PreviousNames = new[] { "scheduling.ChangeAppointment" })]
            public class RescheduleAppointment : global::Aiel.Actions.IAction { }
            """;

        var result = RunGenerator(source);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        aggregateTree.Should().NotBeNull();
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        text.Should().Contain("PermissionName = global::Aiel.Authorization.PermissionName.From(\"scheduling.RescheduleAppointment\")");
        text.Should().Contain("ActionType = typeof(global::Sample.RescheduleAppointment)");
        text.Should().Contain("Lifecycle = global::Aiel.Authorization.PermissionLifecycle.Deprecated");
        text.Should().Contain("PreviousNames = new global::Aiel.Authorization.PermissionName[]");
        text.Should().Contain("global::Aiel.Authorization.PermissionName.From(\"scheduling.ChangeAppointment\")");
    }

    [Fact]
    public void StableIdDefaultsToPermissionName_WhenNotExplicitlySet()
    {
        var result = RunGenerator(ActionSource);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        aggregateTree.Should().NotBeNull();
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        // StableId should use the permission name when not explicitly set
        text.Should().Contain("PermissionStableId.From(\"scheduling.RescheduleAppointment\")");
    }

    [Fact]
    public void StableIdUsesExplicitValue_WhenProvided()
    {
        const String source = """
            using Aiel.Actions;
            using Aiel.Authorization;

            namespace Sample;

            [AuthorizationDefinition(
                "scheduling.RescheduleAppointment",
                "Location",
                "User",
                "Reschedule Appointment",
                StableId = "my-explicit-stable-id")]
            public class RescheduleAppointment : IAction { }
            """;

        var result = RunGenerator(source);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        aggregateTree.Should().NotBeNull();
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        text.Should().Contain("PermissionStableId.From(\"my-explicit-stable-id\")");
        text.Should().NotContain("PermissionStableId.From(\"scheduling.RescheduleAppointment\")");
    }

    [Fact]
    public void GeneratedOutput_IsDeterministicAcrossRuns()
    {
        var result1 = RunGenerator(ActionSource);
        var result2 = RunGenerator(ActionSource);

        result2.GeneratedTrees.Should().HaveCount(result1.GeneratedTrees.Length);
        for (var i = 0; i < result1.GeneratedTrees.Length; i++)
        {
            result2.GeneratedTrees[i].GetText(TestContext.Current.CancellationToken).ToString().Should().Be(
                result1.GeneratedTrees[i].GetText(TestContext.Current.CancellationToken).ToString());
        }
    }

    [Fact]
    public void EmitsNoOutput_WhenNoDecoratedActionsExist()
    {
        const String source = """
            namespace Sample;
            public class PlainAction : global::Aiel.Actions.IAction { }
            """;

        var result = RunGenerator(source);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public async Task GeneratedChecker_SatisfiesActionAuthorizationAnalyzer()
    {
        // Run the generator first to produce checker source
        var (_, generatorResult) = RunGeneratorWithUpdatedCompilation(ActionSource);
        generatorResult.GeneratedTrees.Should().NotBeEmpty();

        // Build a new compilation that includes the generated source alongside the original
        var trees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(ActionSource, cancellationToken: TestContext.Current.CancellationToken),
        };
        foreach (var tree in generatorResult.GeneratedTrees)
        {
            trees.Add(tree);
        }

        var references = ReferenceAssemblies;

        var compilationWithGeneratedCode = CSharpCompilation.Create(
            "IntegrationTest",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ActionAuthorizationAnalyzer();
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
        var compilationWithAnalyzers = compilationWithGeneratedCode.WithAnalyzers(analyzers);
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);

        // The generated checker satisfies condition 1 — no AIEL00006 diagnostic
        diagnostics.Where(d => d.Id == "AIEL00006").Should().BeEmpty();
    }

    [Fact]
    public void GeneratedOutput_CompilesAgainstCurrentPermissionContracts()
    {
        var (compilation, _) = RunGeneratorWithUpdatedCompilation(ActionSource);

        var errors = compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        errors.Should().BeEmpty();
    }

    private static GeneratorDriverRunResult RunGenerator(String source)
        => RunGeneratorWithUpdatedCompilation(source).RunResult;

    /// <summary>
    /// The real Aiel assemblies, not hand-written stubs.
    /// </summary>
    /// <remarks>
    /// This test compilation previously used stub copies of <c>Result&lt;T&gt;</c>,
    /// <c>IAuthorizationGrantEvaluator</c>, and <c>AuthorizationGrantDecision</c>. Because the stubs drifted
    /// independently of the real contracts, a breaking change to a published contract could pass this suite
    /// while breaking every consumer. Referencing the real assemblies means contract drift fails here first.
    /// </remarks>
    private static readonly MetadataReference[] ReferenceAssemblies =
        ((String)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToArray();

    private static (CSharpCompilation Compilation, GeneratorDriverRunResult RunResult) RunGeneratorWithUpdatedCompilation(String source)
    {
        var trees = new[]
        {
            CSharpSyntaxTree.ParseText(source),
        };

        var compilation = CSharpCompilation.Create(
            "PermissionGeneratorUnitTests",
            trees,
            ReferenceAssemblies,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new PermissionDefinitionSourceGenerator();
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()]);
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return ((CSharpCompilation)outputCompilation, updatedDriver.GetRunResult());
    }
}
