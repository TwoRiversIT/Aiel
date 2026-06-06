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

using Aiel.Logging.Helpers;
using Aiel.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    private const string TitleReplace = "Replace with [LoggerMessage] helper";
    private const string TitleComment = "Add TODO comment (manual refactor required)";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [DiagnosticDescriptors.NoDirectILoggerCalls.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken)
                                     .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        var node = root.FindNode(span);

        var invocation = node as InvocationExpressionSyntax
                         ?? node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation is null)
        {
            return;
        }

        // ── Try to find a matching [LoggerMessage] helper ────────────────
        var model = await document.GetSemanticModelAsync(context.CancellationToken)
                                    .ConfigureAwait(false);
        if (model is null)
        {
            return;
        }

        var helperMethod = FindMatchingHelper(invocation, model, context.CancellationToken);

        if (helperMethod is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleReplace,
                    createChangedDocument: ct =>
                        ReplaceWithHelperAsync(document, invocation, helperMethod, ct),
                    equivalenceKey: TitleReplace),
                diagnostic);
        }

        // Always offer the TODO-comment fix as a fallback.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: TitleComment,
                createChangedDocument: ct =>
                    AddTodoCommentAsync(document, invocation, ct),
                equivalenceKey: TitleComment),
            diagnostic);
    }

    // ── Transformation: replace with helper call ─────────────────────────

    private static async Task<Document> ReplaceWithHelperAsync(
        Document document,
        InvocationExpressionSyntax original,
        IMethodSymbol helper,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Determine the receiver expression (the ILogger variable).
        ExpressionSyntax? receiver = null;
        if (original.Expression is MemberAccessExpressionSyntax ma)
        {
            receiver = ma.Expression;
        }

        // Build helper call:  receiver.HelperName(args...)
        // We forward all original arguments after the logger (extension method style).
        var originalArgs = original.ArgumentList.Arguments;

        // Extension methods: arg[0] is the logger itself – skip if the helper
        // already takes the logger as its first explicit parameter.
        ExpressionSyntax helperExpr = receiver is not null
            ? SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                receiver,
                SyntaxFactory.IdentifierName(helper.Name))
            : SyntaxFactory.IdentifierName(helper.Name);

        // Pass through original arguments (excluding the logger if extension method).
        var argList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(originalArgs));

        var newCall = SyntaxFactory.InvocationExpression(helperExpr, argList)
            .WithTriviaFrom(original);

        var newRoot = root.ReplaceNode(original, newCall);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Transformation: TODO comment ─────────────────────────────────────

    private static async Task<Document> AddTodoCommentAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Get the statement containing the invocation.
        var stmt = invocation.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
        if (stmt is null)
        {
            return document;
        }

        var todoTrivia = SyntaxFactory.Comment(
            "// TODO (AIEL004): Replace this direct ILogger call with a [LoggerMessage] helper.\r\n");
        var leading = stmt.GetLeadingTrivia().Insert(0, todoTrivia);
        var newStmt = stmt.WithLeadingTrivia(leading);
        var newRoot = root.ReplaceNode(stmt, newStmt);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Helper detection ─────────────────────────────────────────────────

    /// <summary>
    /// Searches the compilation for a <c>[LoggerMessage]</c>-decorated method
    /// whose name plausibly corresponds to the direct ILogger method being called.
    /// Matching is intentionally loose (any [LoggerMessage] helper in scope).
    /// </summary>
    private static IMethodSymbol? FindMatchingHelper(
        InvocationExpressionSyntax invocation,
        SemanticModel model,
        CancellationToken ct)
    {
        // Determine the containing type so we can search its members.
        var containingType = invocation.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();

        if (containingType is null)
        {
            return null;
        }

        if (model.GetDeclaredSymbol(containingType, ct) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var compilation = model.Compilation;

        // Look for any [LoggerMessage] static partial method in the same type.
        return typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => AnalyzerHelpers.HasLoggerMessageAttribute(m, compilation));
    }
}

