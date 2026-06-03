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
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;

namespace Aiel.Logging.CodeFixes;

/// <summary>
/// Fixes AIEL002 by appending an optional
/// <c>AielEventIds eventId = AielEventIds.MemberName</c> parameter to the
/// method signature.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingEventIdParameterCodeFix))]
[Shared]
public sealed class MissingEventIdParameterCodeFix : CodeFixProvider
{
    private const string Title = "Add optional AielEventIds eventId parameter";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [DiagnosticDescriptors.MissingEventIdParameter.Id];

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

        // Walk up to find the enclosing method declaration.
        var methodDecl = node.AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (methodDecl is null)
        {
            return;
        }

        var memberName = ExtractSuggestedMember(diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: ct =>
                    AddEventIdParameterAsync(context.Document, methodDecl, memberName, ct),
                equivalenceKey: Title),
            diagnostic);
    }

    // ── Transformation ───────────────────────────────────────────────────

    private static async Task<Document> AddEventIdParameterAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        string memberName,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Build:  AielEventIds eventId = AielEventIds.MemberName
        var defaultValue = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(WellKnownTypes.AielEventIdsShort),
                SyntaxFactory.IdentifierName(memberName)));

        var newParam = SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(),
                SyntaxFactory.IdentifierName(WellKnownTypes.AielEventIdsShort),
                SyntaxFactory.Identifier(WellKnownTypes.EventIdParamName),
                defaultValue)
            .WithAdditionalAnnotations(Formatter.Annotation);

        // Append after the last existing parameter.
        var newParams = methodDecl.ParameterList.Parameters.Add(newParam);
        var newParamList = methodDecl.ParameterList.WithParameters(newParams);
        var newMethod = methodDecl.WithParameterList(newParamList);

        var newRoot = root.ReplaceNode(methodDecl, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static string ExtractSuggestedMember(Diagnostic diagnostic)
    {
        // messageFormat: "Method '{0}' … missing an optional 'AielEventIds eventId = AielEventIds.{1}' parameter."
        var msg = diagnostic.GetMessage();
        const string prefix = "AielEventIds eventId = AielEventIds.";
        const string suffix = "' parameter.";

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

        return "SomeMember";
    }
}

