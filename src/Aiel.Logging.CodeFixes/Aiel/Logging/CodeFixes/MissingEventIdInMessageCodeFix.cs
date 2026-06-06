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
// MissingEventIdInMessageCodeFix.cs  (AIEL00010)
//
// Inserts "[{EventId}]" at the start of the LoggerMessage.Message string
// when the placeholder is absent.
//
// Config note: AIEL00010 deals only with message template text — it does not
// reference the EventIds enum — but the fix still reads config from the
// diagnostic properties for consistency (future-proofing if the message
// format ever becomes configurable).
// -----------------------------------------------------------------------

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
/// Code fix for AIEL00010 — inserts <c>[{EventId}] </c> at the beginning of the
/// <see cref="Microsoft.Extensions.Logging.LoggerMessageAttribute"/> <c>Message</c>
/// argument when the <c>[{EventId}]</c> placeholder is missing.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingEventIdInMessageCodeFix))]
[Shared]
public sealed class MissingEventIdInMessageCodeFix : CodeFixProvider
{
    private const String EventIdPlaceholder = "[{EventId}]";

    public override ImmutableArray<String> FixableDiagnosticIds
        => [DiagnosticDescriptors.MissingEventIdInMessage.Id];

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

        // The diagnostic span points at the Message literal argument.
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // Walk up to find the AttributeArgumentSyntax whose expression is
        // either a string literal or an interpolated string.
        var argSyntax = node.AncestorsAndSelf()
            .OfType<AttributeArgumentSyntax>()
            .FirstOrDefault();

        if (argSyntax is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Prepend \"{EventIdPlaceholder}\" to message",
                createChangedDocument: ct =>
                    PrependPlaceholderAsync(context.Document, argSyntax, ct),
                equivalenceKey: nameof(MissingEventIdInMessageCodeFix)),
            diagnostic);
    }

    // ── Implementation ───────────────────────────────────────────────────

    private static async Task<Document> PrependPlaceholderAsync(
        Document document,
        AttributeArgumentSyntax argSyntax,
        CancellationToken cancellationToken)
    {
        var root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        ExpressionSyntax? newExpression = argSyntax.Expression switch
        {
            // Simple string literal: "My message" → "[{EventId}] My message"
            LiteralExpressionSyntax lit
                when lit.IsKind(SyntaxKind.StringLiteralExpression) =>
                    BuildPrependedStringLiteral(lit),

            // Verbatim string literal: @"My message" → "[{EventId}] My message" (regular)
            LiteralExpressionSyntax lit
                when lit.IsKind(SyntaxKind.StringLiteralExpression) &&
                     lit.Token.IsVerbatimStringLiteral() =>
                    BuildPrependedStringLiteral(lit),

            // Raw string literal or interpolated string — convert to regular literal
            _ => BuildFallbackLiteral(argSyntax.Expression)
        };

        if (newExpression is null)
        {
            return document;
        }

        var newArg = argSyntax.WithExpression(newExpression.WithTriviaFrom(argSyntax.Expression));
        var newRoot = root.ReplaceNode(argSyntax, newArg);
        return document.WithSyntaxRoot(newRoot);
    }

    /// <summary>
    /// Prepends <c>[{EventId}] </c> to the text of an existing string literal.
    /// Preserves the verbatim / non-verbatim style of the original.
    /// </summary>
    private static LiteralExpressionSyntax BuildPrependedStringLiteral(
        LiteralExpressionSyntax original)
    {
        var originalText = original.Token.ValueText; // already unescaped
        var newText = $"{EventIdPlaceholder} {originalText}";

        // Always emit as a regular (non-verbatim) string because "[{EventId}]"
        // contains no characters that need verbatim treatment.
        var newToken = SyntaxFactory.Literal(newText);
        return SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            newToken);
    }

    /// <summary>
    /// Fallback for interpolated / raw strings — emits a plain string literal
    /// containing only the placeholder (the developer can merge the rest manually).
    /// </summary>
    private static LiteralExpressionSyntax BuildFallbackLiteral(ExpressionSyntax _)
    {
        var token = SyntaxFactory.Literal($"{EventIdPlaceholder} <original message>");
        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, token);
    }
}
