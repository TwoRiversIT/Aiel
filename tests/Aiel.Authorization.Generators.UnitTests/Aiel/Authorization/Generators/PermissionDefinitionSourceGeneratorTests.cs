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
    /// <summary>
    /// Minimal stubs for the types the generator and checker reference.
    /// All are compiled into the same <see cref="CSharpCompilation"/> so that
    /// <c>ForAttributeWithMetadataName</c> can resolve <c>AuthorizationDefinitionAttribute</c>
    /// and the generated checker source compiles cleanly.
    /// </summary>
    private const String PermissionStub = """
        using System;
        using System.Collections.Generic;
        using System.Threading;
        using System.Threading.Tasks;
        using Aiel.Results;

        namespace Aiel.Actions
        {
            public interface IAction { }
        }

        namespace Aiel.Actions
        {
            public interface IActionExecutionContext<TAction>
                where TAction : global::Aiel.Actions.IAction { }
        }

        namespace Aiel.Results
        {
            public readonly struct Result
            {
                public bool IsSuccess { get; }
                public Error Error { get; }
                public static Result Success() => default;
                public static Result Failure(Error error) => default;
            }

            public readonly struct Result<T>
            {
                public bool IsSuccess { get; }
                public T Value { get; }
                public Error Error { get; }
            }

            public readonly struct Error { }
        }

        namespace Aiel.Authorization
        {
            public interface IAction : global::Aiel.Actions.IAction { }

            public interface IActionAuthorizationChecker<TAction>
                where TAction : global::Aiel.Actions.IAction { }

            public interface IAuthorizationGrantEvaluator
            {
                Task<global::Aiel.Results.Result<AuthorizationGrantDecision?>> EvaluateAsync(
                    PermissionName permissionName,
                    AuthorizationScopeTypeName scopeType,
                    AuthorizationScopeKey scopeKey,
                    AuthorizationSubjectTypeName subjectType,
                    AuthorizationSubjectKey subjectKey,
                    CancellationToken cancellationToken = default);
            }

            public interface IAuthorizationScopeResolver<TAction>
                where TAction : global::Aiel.Actions.IAction
            {
                Task<global::Aiel.Results.Result<AuthorizationScopeResolution>> ResolveAsync(
                    global::Aiel.Actions.IActionExecutionContext<TAction> context,
                    CancellationToken cancellationToken = default);
            }

            public interface IAuthorizationSubjectResolver<TAction>
                where TAction : global::Aiel.Actions.IAction
            {
                AuthorizationSubjectKey ResolveSubjectKey(
                    global::Aiel.Actions.IActionExecutionContext<TAction> context);
            }

            public enum AuthorizationGrantDecision { Granted = 0, Prohibited = 1 }

            public enum PermissionLifecycle { Active = 0, Deprecated = 1 }

            public readonly struct PermissionName
            {
                public static PermissionName From(string value) => default;
            }

            public readonly struct AuthorizationScopeTypeName
            {
                public static AuthorizationScopeTypeName From(string value) => default;
            }

            public readonly struct AuthorizationScopeKey { }

            public readonly struct AuthorizationSubjectTypeName
            {
                public static AuthorizationSubjectTypeName From(string value) => default;
            }

            public readonly struct AuthorizationSubjectKey { }

            public readonly struct PermissionStableId
            {
                public static PermissionStableId From(string value) => default;
            }

            public readonly struct AuthorizationScopeResolution
            {
                public AuthorizationScopeTypeName ScopeType { get; }
                public AuthorizationScopeKey ScopeKey { get; }
            }

            public sealed record AuthorizationDefinitionManifest
            {
                public required PermissionName PermissionName { get; init; }
                public required PermissionStableId StableId { get; init; }
                public required Type ActionType { get; init; }
                public required AuthorizationScopeTypeName ScopeType { get; init; }
                public required AuthorizationSubjectTypeName SubjectType { get; init; }
                public required string DisplayName { get; init; }
                public string Description { get; init; } = string.Empty;
                public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;
                public IReadOnlyList<PermissionName> PreviousNames { get; init; } = Array.Empty<PermissionName>();
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class AuthorizationDefinitionAttribute : Attribute
            {
                public AuthorizationDefinitionAttribute(
                    string permissionName, string scopeType, string subjectType, string displayName)
                {
                    PermissionName = permissionName;
                    ScopeType = scopeType;
                    SubjectType = subjectType;
                    DisplayName = displayName;
                }
                public string PermissionName { get; }
                public string ScopeType { get; }
                public string SubjectType { get; }
                public string DisplayName { get; }
                public string Description { get; init; } = string.Empty;
                public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;
                public string[] PreviousNames { get; init; } = Array.Empty<string>();
                public string? StableId { get; init; }
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class DoesNotRespectAuthorityAttribute : Attribute
            {
                public string Reason { get; init; } = "";
            }

            public static class AuthorizationErrors
            {
                public static global::Aiel.Results.Error PermissionDenied(PermissionName name) => default;
            }
        }
        """;

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
        Assert.NotNull(checkerTree);
        var text = checkerTree.GetText(TestContext.Current.CancellationToken).ToString();
        Assert.Contains("RescheduleAppointmentPermissionChecker", text);
        Assert.Contains("IActionAuthorizationChecker<", text);
    }

    [Fact]
    public void EmitsPermissionNameConstant_ForDecoratedAction()
    {
        var result = RunGenerator(ActionSource);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        Assert.NotNull(aggregateTree);
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        Assert.Contains("GeneratedAuthorizationNames", text);
        Assert.Contains("scheduling.RescheduleAppointment", text);
    }

    [Fact]
    public void EmitsGetManifestsMethod_ForDecoratedAction()
    {
        var result = RunGenerator(ActionSource);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        Assert.NotNull(aggregateTree);
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        Assert.Contains("GeneratedAuthorizationManifests", text);
        Assert.Contains("GetManifests()", text);
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
        Assert.NotNull(aggregateTree);
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        Assert.Contains("PermissionName = global::Aiel.Authorization.PermissionName.From(\"scheduling.RescheduleAppointment\")", text);
        Assert.Contains("ActionType = typeof(global::Sample.RescheduleAppointment)", text);
        Assert.Contains("Lifecycle = global::Aiel.Authorization.PermissionLifecycle.Deprecated", text);
        Assert.Contains("PreviousNames = new global::Aiel.Authorization.PermissionName[]", text);
        Assert.Contains("global::Aiel.Authorization.PermissionName.From(\"scheduling.ChangeAppointment\")", text);
    }

    [Fact]
    public void StableIdDefaultsToPermissionName_WhenNotExplicitlySet()
    {
        var result = RunGenerator(ActionSource);

        var aggregateTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("GeneratedPermissions"));
        Assert.NotNull(aggregateTree);
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        // StableId should use the permission name when not explicitly set
        Assert.Contains("PermissionStableId.From(\"scheduling.RescheduleAppointment\")", text);
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
        Assert.NotNull(aggregateTree);
        var text = aggregateTree.GetText(TestContext.Current.CancellationToken).ToString();
        Assert.Contains("PermissionStableId.From(\"my-explicit-stable-id\")", text);
        Assert.DoesNotContain("PermissionStableId.From(\"scheduling.RescheduleAppointment\")", text);
    }

    [Fact]
    public void GeneratedOutput_IsDeterministicAcrossRuns()
    {
        var result1 = RunGenerator(ActionSource);
        var result2 = RunGenerator(ActionSource);

        Assert.Equal(result1.GeneratedTrees.Length, result2.GeneratedTrees.Length);
        for (var i = 0; i < result1.GeneratedTrees.Length; i++)
        {
            Assert.Equal(
                result1.GeneratedTrees[i].GetText(TestContext.Current.CancellationToken).ToString(),
                result2.GeneratedTrees[i].GetText(TestContext.Current.CancellationToken).ToString());
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

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public async Task GeneratedChecker_SatisfiesActionAuthorizationAnalyzer()
    {
        // Run the generator first to produce checker source
        var (_, generatorResult) = RunGeneratorWithUpdatedCompilation(ActionSource);
        Assert.NotEmpty(generatorResult.GeneratedTrees);

        // Build a new compilation that includes the generated source alongside the original
        var trees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(PermissionStub, cancellationToken: TestContext.Current.CancellationToken),
            CSharpSyntaxTree.ParseText(ActionSource, cancellationToken: TestContext.Current.CancellationToken),
        };
        foreach (var tree in generatorResult.GeneratedTrees)
        {
            trees.Add(tree);
        }

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
        };

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
        Assert.Empty(diagnostics.Where(d => d.Id == "AIEL00006"));
    }

    [Fact]
    public void GeneratedOutput_CompilesAgainstCurrentPermissionContracts()
    {
        var (compilation, _) = RunGeneratorWithUpdatedCompilation(ActionSource);

        var errors = compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.Empty(errors);
    }

    private static GeneratorDriverRunResult RunGenerator(String source)
        => RunGeneratorWithUpdatedCompilation(source).RunResult;

    private static (CSharpCompilation Compilation, GeneratorDriverRunResult RunResult) RunGeneratorWithUpdatedCompilation(String source)
    {
        var trees = new[]
        {
            CSharpSyntaxTree.ParseText(PermissionStub),
            CSharpSyntaxTree.ParseText(source),
        };

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "PermissionGeneratorUnitTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new PermissionDefinitionSourceGenerator();
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()]);
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return ((CSharpCompilation)outputCompilation, updatedDriver.GetRunResult());
    }
}
