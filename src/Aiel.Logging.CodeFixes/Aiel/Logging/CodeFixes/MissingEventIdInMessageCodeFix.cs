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
/// Fixes AIEL003 by prepending <c>[{EventId}] </c> to the message template
/// string literal in a <c>[LoggerMessage]</c> attribute.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingEventIdInMessageCodeFix))]
[Shared]
public sealed class MissingEventIdInMessageCodeFix : CodeFixProvider
{
    private const string Title = "Prepend [{EventId}] to message template";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [DiagnosticDescriptors.MissingEventIdInMessage.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
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
        var span = diagnostic.Location.SourceSpan;
        var node = root.FindNode(span);

        // The reported node is the string literal expression.
        if (node is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: ct =>
                    PrependPlaceholderAsync(context.Document, literal, ct),
                equivalenceKey: Title),
            diagnostic);
    }

    // ── Transformation ───────────────────────────────────────────────────

    private static async Task<Document> PrependPlaceholderAsync(
        Document document,
        LiteralExpressionSyntax literal,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var originalText = literal.Token.ValueText;
        var newText = WellKnownTypes.EventIdPlaceholder + " " + originalText;

        // Reconstruct the string token, preserving the quote style (regular vs verbatim).
        var originalToken = literal.Token;
        SyntaxToken newToken;

        if (originalToken.IsKind(SyntaxKind.StringLiteralToken))
        {
            // Regular string – rebuild with escaped content.
            newToken = SyntaxFactory.Literal(newText)
                .WithTriviaFrom(originalToken);
        }
        else
        {
            // Verbatim or interpolated – safest is a regular string here.
            newToken = SyntaxFactory.Literal(newText)
                .WithTriviaFrom(originalToken);
        }

        var newLiteral = literal.WithToken(newToken);
        var newRoot = root.ReplaceNode(literal, newLiteral);
        return document.WithSyntaxRoot(newRoot);
    }
}

