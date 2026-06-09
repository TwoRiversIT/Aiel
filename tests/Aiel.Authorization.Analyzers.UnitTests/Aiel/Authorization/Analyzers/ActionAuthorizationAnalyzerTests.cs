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

using Aiel.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Authorization.Analyzers;

public class ActionAuthorizationAnalyzerTests
{
    /// <summary>
    /// Minimal in-memory definitions of <c>IAction</c>, <c>IActionAuthorizationChecker&lt;TAction&gt;</c>,
    /// and <c>DoesNotRespectAuthorityAttribute</c>. These stubs are compiled into the same
    /// <see cref="CSharpCompilation"/> as the test source so <c>GetTypeByMetadataName</c> can resolve them
    /// and <c>compilation.Assembly.GlobalNamespace</c> walking can discover test action classes.
    ///
    /// NOTE: <c>Reason</c> has no <c>required</c> modifier here so tests can exercise the empty/whitespace
    /// path without a compile-time error at the call site.
    /// </summary>
    private const String PermissionStub = """
        namespace Aiel.Actions
        {
            public interface IAction { }
        }

        namespace Aiel.Authorization
        {
            public interface IActionAuthorizationChecker<TAction>
                where TAction : global::Aiel.Actions.IAction { }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class DoesNotRespectAuthorityAttribute : System.Attribute
            {
                // No 'required' modifier: allows test source to omit Reason, exercising the null path.
                public string Reason { get; init; } = "";
            }
        }
        """;

    [Fact]
    public async Task ReportsDiagnostic_WhenActionHasNoCheckerAndNoMarker()
    {
        const String source = """
            namespace Sample;
            public class MyAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticRuleIDs.AIEL00006_ActionHasNoAuthorizationStoryId, diagnostic.Id);
        Assert.Contains("MyAction", diagnostic.GetMessage());
    }

    [Fact]
    public async Task DoesNotReport_WhenActionHasConcreteChecker()
    {
        const String source = """
            namespace Sample;
            public class MyAction : Aiel.Actions.IAction { }
            public class MyActionChecker : Aiel.Authorization.IActionAuthorizationChecker<MyAction> { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenActionIsMarkedWithValidDoesNotRespectAuthority()
    {
        const String source = """
            using Aiel.Authorization;
            namespace Sample;
            [DoesNotRespectAuthority(Reason = "Available to unauthenticated callers")]
            public class MyAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenNoConcreteActionExists()
    {
        const String source = """
            namespace Sample;
            public class NotAnAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsDiagnostic_WhenDoesNotRespectAuthorityHasEmptyReason()
    {
        const String source = """
            using Aiel.Authorization;
            namespace Sample;
            [DoesNotRespectAuthority(Reason = "")]
            public class MyAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticRuleIDs.AIEL00007_DoesNotRespectAuthorityReasonIsEmptyId, diagnostic.Id);
        Assert.Contains("MyAction", diagnostic.GetMessage());
    }

    [Fact]
    public async Task ReportsDiagnostic_WhenDoesNotRespectAuthorityHasWhitespaceReason()
    {
        const String source = """
            using Aiel.Authorization;
            namespace Sample;
            [DoesNotRespectAuthority(Reason = "   ")]
            public class MyAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticRuleIDs.AIEL00007_DoesNotRespectAuthorityReasonIsEmptyId, diagnostic.Id);
    }

    [Fact]
    public async Task DoesNotReport_WhenActionIsAbstract()
    {
        const String source = """
            namespace Sample;
            public abstract class AbstractAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenIActionIsNotReferenced()
    {
        // No stub means GetTypeByMetadataName("Aiel.Actions.IAction") returns null → early exit.
        const String source = """
            namespace Sample;
            public class SomeClass { }
            """;

        var diagnostics = await AnalyzeAsync(source, includeStub: false);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsDiagnostic_OnEachAction_WhenMultipleActionsHaveNoAuthorizationStory()
    {
        const String source = """
            namespace Sample;
            public class ActionOne : Aiel.Actions.IAction { }
            public class ActionTwo : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, d => Assert.Equal(DiagnosticRuleIDs.AIEL00006_ActionHasNoAuthorizationStoryId, d.Id));
    }

    [Fact]
    public async Task DoesNotReport_AIEL00006_WhenMarkerIsPresentEvenWithEmptyReason()
    {
        // When [DoesNotRespectAuthority] is present with an empty Reason, only AIEL00007 fires,
        // never AIEL00006. The marker's presence is the authorization story; the Reason quality is separate.
        const String source = """
            using Aiel.Authorization;
            namespace Sample;
            [DoesNotRespectAuthority(Reason = "")]
            public class MyAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticRuleIDs.AIEL00006_ActionHasNoAuthorizationStoryId);
    }

    [Fact]
    public async Task DoesNotReport_WhenTypeIsInterface()
    {
        const String source = """
            namespace Sample;
            public interface ICustomAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenReasonIsNonEmpty()
    {
        const String source = """
            using Aiel.Authorization;
            namespace Sample;
            [DoesNotRespectAuthority(Reason = "Reason supplied.")]
            public class MyAction : Aiel.Actions.IAction { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenCheckerIsInDifferentNamespace()
    {
        // Checker recognition must be compilation-wide, not namespace-restricted.
        const String source = """
            namespace Foo { public class SampleAction : Aiel.Actions.IAction { } }
            namespace Bar { public class SampleActionChecker : Aiel.Authorization.IActionAuthorizationChecker<Foo.SampleAction> { } }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsAIEL00006_WhenCheckerExistsForDifferentAction()
    {
        // Checker for A must not suppress the diagnostic for B.
        const String source = """
            namespace Sample;
            public class SampleActionA : Aiel.Actions.IAction { }
            public class SampleActionB : Aiel.Actions.IAction { }
            public class SampleActionAChecker : Aiel.Authorization.IActionAuthorizationChecker<SampleActionA> { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticRuleIDs.AIEL00006_ActionHasNoAuthorizationStoryId, diagnostic.Id);
        Assert.Contains("SampleActionB", diagnostic.GetMessage());
    }

    [Fact]
    public async Task DoesNotReport_WhenCheckerIsAbstractButConcreteSubclassExists()
    {
        // A concrete subclass of an abstract checker satisfies the requirement because
        // AllInterfaces on the concrete subclass includes the checker interface.
        const String source = """
            namespace Sample;
            public class SampleAction : Aiel.Actions.IAction { }
            public abstract class AbstractChecker : Aiel.Authorization.IActionAuthorizationChecker<SampleAction> { }
            public sealed class ConcreteChecker : AbstractChecker { }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(String source, Boolean includeStub = true)
    {
        List<SyntaxTree> trees = includeStub
            ? [CSharpSyntaxTree.ParseText(PermissionStub), CSharpSyntaxTree.ParseText(source)]
            : [CSharpSyntaxTree.ParseText(source)];

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "ActionAuthorizationAnalyzerUnitTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ActionAuthorizationAnalyzer();
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
