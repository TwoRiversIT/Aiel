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
// UseAielEventIdsCodeFix.cs  –  Fix for AIEL00008
//
// Replaces a raw integer EventId literal with (int)<ConfiguredType>.Member.
// The configured type name is read from Diagnostic.Properties so the fix
// always generates syntax that matches whatever enum the project uses.
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

namespace Aiel.Logging.Analyzers.CodeFixes;

/// <summary>
/// Fixes AIEL00008 by replacing a raw integer literal with
/// <c>(int)&lt;ConfiguredEventIdsType&gt;.MemberName</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseAielEventIdsCodeFix))]
[Shared]
public sealed class UseAielEventIdsCodeFix : CodeFixProvider
{
    private const String Title = "Replace with (int)<EventIds> member";

    /// <inheritdoc />
    public override ImmutableArray<String> FixableDiagnosticIds
        => [DiagnosticDescriptors.UseAielEventIds.Id];

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
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        if (node is not ExpressionSyntax expr)
        {
            return;
        }

        // Read the EventIds type name that was resolved at analysis time.
        // Falls back to the Aiel default when the property is absent.
        var config = AnalyzerConfiguration.ReadFromDiagnostic(diagnostic)
            ?? EventIdsTypeConfig.FromFullTypeName(AnalyzerConfiguration.DefaultFullTypeName);

        var memberName = ExtractSuggestedMember(diagnostic.GetMessage(), config.ShortName)
            ?? AnalyzerConfiguration.DefaultMemberName;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Replace with (int){config.ShortName}.{memberName}",
                createChangedDocument: ct =>
                    ReplaceWithEnumCastAsync(context.Document, expr, config.ShortName, memberName, ct),
                equivalenceKey: Title),
            diagnostic);
    }

    // ── Transformation ───────────────────────────────────────────────────

    private static async Task<Document> ReplaceWithEnumCastAsync(
        Document document,
        ExpressionSyntax expr,
        String enumShortName,
        String memberName,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Build:  (int)EnumType.MemberName
        var castExpr = SyntaxFactory.CastExpression(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(enumShortName),
                SyntaxFactory.IdentifierName(memberName)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        return document.WithSyntaxRoot(root.ReplaceNode(expr, castExpr));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static String? ExtractSuggestedMember(String msg, String shortName)
    {
        // messageFormat: "Use '(int)EnumType.{member}' instead."
        var prefix = $"(int){shortName}.";
        const String suffix = "' instead.";

        var start = msg.IndexOf(prefix, StringComparison.Ordinal);
        if (start >= 0)
        {
            start += prefix.Length;
            var end = msg.IndexOf(suffix, start, StringComparison.Ordinal);
            if (end > start)
            {
                return msg.Substring(start, end - start);
            }
        }

        return null;
    }
}
