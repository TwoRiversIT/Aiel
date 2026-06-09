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

// -----------------------------------------------------------------------
// NoDirectILoggerCallsCodeFix.cs  (AIEL00011)
//
// When the analyzer detects a direct ILogger.Log* call inside a method
// that is itself decorated with [LoggerMessage], the fix offers to remove
// the offending call (leaving a TODO comment) because a safe automatic
// replacement requires domain knowledge the fix cannot supply.
//
// A second, more aggressive fix is also offered that removes the entire
// statement unconditionally.
//
// Config note: AIEL00011 does not reference the EventIds enum, but the fix
// reads AnalyzerConfiguration from the diagnostic properties for
// consistency with the rest of the fix layer.
// -----------------------------------------------------------------------

using Aiel.Logging.Configuration;
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;

namespace Aiel.Logging.CodeFixes;

/// <summary>
/// Code fix for AIEL00011 — offers two remediation strategies when a direct
/// <c>ILogger</c> extension call is found inside a <c>[LoggerMessage]</c> method:
/// <list type="number">
///   <item>Replace the statement with a <c>// TODO:</c> comment so the developer
///   can wire it to an Aiel-aware helper manually.</item>
///   <item>Delete the statement outright.</item>
/// </list>
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NoDirectILoggerCallsCodeFix))]
[Shared]
public sealed class NoDirectILoggerCallsCodeFix : CodeFixProvider
{
    public override ImmutableArray<String> FixableDiagnosticIds
        => [DiagnosticDescriptors.NoDirectILoggerCalls.Id];

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];

        // AnalyzerConfiguration is resolved for property-stamping consistency;
        // AIEL00011 does not use the EventIds type name in the fix itself.
        var config = AnalyzerConfiguration.ReadFromDiagnostic(diagnostic);

        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // Walk up to the enclosing ExpressionStatement (the invocation call).
        var statement = node.AncestorsAndSelf()
            .OfType<ExpressionStatementSyntax>()
            .FirstOrDefault();

        if (statement is null)
        {
            return;
        }

        // Fix 1 — replace with TODO comment
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with TODO comment (manual migration needed)",
                createChangedDocument: ct =>
                    ReplaceWithTodoCommentAsync(context.Document, statement, ct),
                equivalenceKey: $"{nameof(NoDirectILoggerCallsCodeFix)}_Todo"),
            diagnostic);

        // Fix 2 — remove the statement entirely
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove direct ILogger call",
                createChangedDocument: ct =>
                    RemoveStatementAsync(context.Document, statement, ct),
                equivalenceKey: $"{nameof(NoDirectILoggerCallsCodeFix)}_Remove"),
            diagnostic);
    }

    // ── Fix 1: replace with TODO comment ────────────────────────────────

    private static async Task<Document> ReplaceWithTodoCommentAsync(
        Document document,
        ExpressionStatementSyntax statement,
        CancellationToken cancellationToken)
    {
        var root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        // Build:  // TODO: replace with the appropriate Aiel logging helper (AIEL00011)
        // Emit as an empty statement with a leading comment so formatting is preserved.
        var originalCall = statement.Expression.ToString().Trim();
        var comment = SyntaxFactory.Comment(
            $"// TODO (AIEL00011): replace with Aiel logging helper — was: {TruncateSafe(originalCall, 80)}");

        // An empty statement (just a semicolon) with the comment attached.
        var placeholder = SyntaxFactory
            .EmptyStatement()
            .WithLeadingTrivia(
                statement.GetLeadingTrivia()
                    .Add(comment)
                    .Add(SyntaxFactory.CarriageReturnLineFeed))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(statement, placeholder);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Fix 2: remove statement ──────────────────────────────────────────

    private static async Task<Document> RemoveStatementAsync(
        Document document,
        ExpressionStatementSyntax statement,
        CancellationToken cancellationToken)
    {
        var root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        var newRoot = root.RemoveNode(statement, SyntaxRemoveOptions.KeepNoTrivia)!;
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Utility ──────────────────────────────────────────────────────────

    private static String TruncateSafe(String s, Int32 maxLen)
        => s.Length <= maxLen ? s : String.Concat(s.Substring(0, maxLen - 3), "...");
}
