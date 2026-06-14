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

using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers.CodeFixes;

public sealed class NoDirectILoggerCallsCodeFixTests
{
    // Fix index 0 = "Replace with TODO comment"
    // Fix index 1 = "Remove direct ILogger call"

    [Fact]
    public async Task DirectLogCall_ReplacedWithTodoComment()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    logger.LogInformation("Service started");
                }
            }
            """;

        var fixedSource = await ApplyCodeFixAsync(source, codeFixIndex: 0, TestContext.Current.CancellationToken);

        fixedSource.Should().Contain("TODO (AIEL00011)");
        fixedSource.Should().Contain("logger.LogInformation(\"Service started\")");
    }

    private static async Task<String> ApplyCodeFixAsync(String source, Int32 codeFixIndex, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Preview))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Object).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Task).Assembly.Location));

        solution = solution
            .AddDocument(DocumentId.CreateNewId(projectId), "Source.cs", source)
            .AddDocument(DocumentId.CreateNewId(projectId), "AielEvent.cs", TestCode.AielEventIdsSource)
            .AddDocument(DocumentId.CreateNewId(projectId), "LoggerMessageAttribute.cs", TestCode.LoggerMessageAttrSource)
            .AddDocument(DocumentId.CreateNewId(projectId), "ILogger.cs", TestCode.ILoggerSource);

        var project = solution.GetProject(projectId)!;
        var document = project.Documents.First(d => d.Name == "Source.cs");
        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(compilation);

        var analyzer = new NoDirectILoggerCallsAnalyzer();
        var diagnostics = await compilation
            .WithAnalyzers([analyzer], options: null)
            .GetAnalyzerDiagnosticsAsync(cancellationToken)
            .ConfigureAwait(false);

        var diagnostic = diagnostics.First(d => d.Id == DiagnosticDescriptors.NoDirectILoggerCalls.Id);

        var codeFixProvider = new NoDirectILoggerCallsCodeFix();
        var codeActions = new List<CodeAction>();

        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => codeActions.Add(action),
            cancellationToken);

        await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        var action = codeActions[codeFixIndex];
        var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
        var changedSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var fixedDocument = changedSolution.GetDocument(document.Id);
        ArgumentNullException.ThrowIfNull(fixedDocument);

        var text = await fixedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
        return text.ToString();
    }

    [Fact]
    public async Task DirectLogCall_RemovedEntirely()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogInformation("Service started")|};
                }
            }
            """;

        // Fix 1: remove the statement completely.
        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielCodeFixVerifier<NoDirectILoggerCallsAnalyzer, NoDirectILoggerCallsCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, codeFixIndex: 1, expected: expected);
    }
}
