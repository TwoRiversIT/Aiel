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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

namespace Aiel.Logging.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LoggerMessageCodeFixProvider)), Shared]
public class LoggerMessageCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create("CA1848"); // Matches the CA1848 rule

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the invocation expression
        var invocationExpr = root.FindToken(diagnosticSpan.Start)
                                 .Parent.AncestorsAndSelf()
                                 .OfType<InvocationExpressionSyntax>()
                                 .FirstOrDefault();

        if (invocationExpr == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Refactor to LoggerMessage",
                createChangedDocument: c => RefactorToLoggerMessageAsync(context.Document, invocationExpr, c),
                equivalenceKey: "RefactorToLoggerMessage"),
            diagnostic);
    }

    private async Task<Document> RefactorToLoggerMessageAsync(
        Document document,
        InvocationExpressionSyntax invocationExpr,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        // Extract method name and arguments
        if (invocationExpr.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text.StartsWith("Log"))
        {
            var args = invocationExpr.ArgumentList.Arguments;
            if (args.Count > 0 && args[0].Expression is LiteralExpressionSyntax messageLiteral)
            {
                var messageText = messageLiteral.Token.ValueText;

                // Generate a new partial method
                var loggerMethod = SyntaxFactory.ParseMemberDeclaration($@"
[LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = ""{messageText}"")]
public static partial void GiveThisABetterName(this ILogger logger);
");

                // Insert into a static partial class
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                var firstClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (firstClass != null)
                {
                    var newRoot = root.InsertNodesBefore(firstClass, [loggerMethod]);
                    return document.WithSyntaxRoot(newRoot);
                }
            }
        }

        return document;
    }
}
