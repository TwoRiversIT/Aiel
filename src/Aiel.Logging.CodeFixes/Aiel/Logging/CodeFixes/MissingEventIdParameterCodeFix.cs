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
// MissingEventIdParameterCodeFix.cs  –  Fix for AIEL00009
//
// Appends an optional  "<ConfiguredType> eventId = <ConfiguredType>.X"
// parameter to a [LoggerMessage]-decorated method that is missing it.
// The configured type name is read from Diagnostic.Properties.
// -----------------------------------------------------------------------

using Aiel.Internal;
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
/// Fixes AIEL00009 by appending an optional
/// <c>&lt;EventIdsType&gt; eventId = &lt;EventIdsType&gt;.MemberName</c>
/// parameter to the method signature.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingEventIdParameterCodeFix))]
[Shared]
public sealed class MissingEventIdParameterCodeFix : CodeFixProvider
{
    private const String Title = "Add optional EventIds eventId parameter";

    /// <inheritdoc />
    public override ImmutableArray<String> FixableDiagnosticIds
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
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var methodDecl = node.AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();
        if (methodDecl is null)
        {
            return;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        var config = AnalyzerConfiguration.ReadFromDiagnostic(diagnostic)
                     ?? EventIdsTypeConfig.FromFullTypeName(
                            AnalyzerConfiguration.DefaultFullTypeName);

        var memberName = TryResolveSuggestedMember(methodDecl, semanticModel, config)
            ?? ExtractSuggestedMember(diagnostic.GetMessage(), config.ShortName)
            ?? AnalyzerConfiguration.DefaultMemberName;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Add optional {config.ShortName} eventId = {config.ShortName}.{memberName}",
                createChangedDocument: ct =>
                    AddEventIdParameterAsync(context.Document, methodDecl,
                        config.ShortName, memberName, ct),
                equivalenceKey: Title),
            diagnostic);
    }

    // ── Transformation ───────────────────────────────────────────────────

    private static async Task<Document> AddEventIdParameterAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        String enumShortName,
        String memberName,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Build:  EnumType eventId = EnumType.MemberName
        var defaultValue = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(enumShortName),
                SyntaxFactory.IdentifierName(memberName)));

        var newParam = SyntaxFactory.Parameter(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(),
                SyntaxFactory.IdentifierName(enumShortName),
                SyntaxFactory.Identifier(WellKnownTypes.EventIdParamName),
                defaultValue)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newParams = methodDecl.ParameterList.Parameters.Add(newParam);
        var newParamList = methodDecl.ParameterList.WithParameters(newParams);
        var newMethod = methodDecl.WithParameterList(newParamList);

        return document.WithSyntaxRoot(root.ReplaceNode(methodDecl, newMethod));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static String? TryResolveSuggestedMember(
        MethodDeclarationSyntax methodDecl,
        SemanticModel semanticModel,
        EventIdsTypeConfig config)
    {
        if (semanticModel.GetDeclaredSymbol(methodDecl) is not IMethodSymbol method)
        {
            return null;
        }

        var attrData = AnalyzerHelpers.GetLoggerMessageAttribute(method, semanticModel.Compilation);
        if (attrData is null)
        {
            return null;
        }

        var arg = AnalyzerHelpers.GetNamedArgument(attrData, WellKnownTypes.EventIdArgName);
        if (arg?.Value is not Int32 intVal)
        {
            return null;
        }

        var eventIdsType = config.GetTypeSymbol(semanticModel.Compilation);
        if (eventIdsType is null)
        {
            return null;
        }

        return AnalyzerHelpers.TryResolveMemberName(intVal, eventIdsType);
    }

    private static String ExtractSuggestedMember(String msg, String shortName)
    {
        // messageFormat: "…missing an optional '<ShortName> eventId = <ShortName>.{member}' parameter."
        var prefix = $"{shortName} eventId = {shortName}.";
        const String suffix = "' parameter.";

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

        return AnalyzerConfiguration.DefaultMemberName;
    }
}
