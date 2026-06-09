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
// EventIdMismatchCodeFix.cs  –  Fix for AIEL00012
//
// Offers two code actions when [LoggerMessage] EventId and the eventId
// parameter default disagree:
//   1. Trust the attribute  → update the parameter default
//   2. Trust the parameter  → update the attribute EventId expression
//
// The configured EventIds type name is read from Diagnostic.Properties
// so the fix generates syntax for whatever enum the project uses.
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
/// Fixes AIEL00012 by synchronising the <c>eventId</c> parameter default with
/// the <c>[LoggerMessage]</c> attribute's <c>EventId</c> value, or vice-versa.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EventIdMismatchCodeFix))]
[Shared]
public sealed class EventIdMismatchCodeFix : CodeFixProvider
{
    private const String TitleSyncParam = "Sync parameter default to match [LoggerMessage] EventId";
    private const String TitleSyncAttr = "Sync [LoggerMessage] EventId to match parameter default";

    /// <inheritdoc />
    public override ImmutableArray<String> FixableDiagnosticIds
        => [DiagnosticDescriptors.EventIdMismatch.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var model = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (model is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var methodDecl = node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDecl is null)
        {
            return;
        }

        var config = AnalyzerConfiguration.ReadFromDiagnostic(diagnostic)
                     ?? EventIdsTypeConfig.FromFullTypeName(AnalyzerConfiguration.DefaultFullTypeName);

        var (attrMember, paramMember) = ExtractMembers(diagnostic.GetMessage(), config.ShortName);
        if (attrMember is null || paramMember is null)
        {
            return;
        }

        // Fix 1: sync attribute → parameter (trust parameter)
        context.RegisterCodeFix(
            CodeAction.Create(
                title: TitleSyncAttr,
                createChangedDocument: ct =>
                    SyncAttributeToParameterAsync(document, methodDecl, model,
                        config, paramMember, ct),
                equivalenceKey: TitleSyncAttr),
            diagnostic);

        // Fix 2: sync parameter → attribute (trust attribute)
        context.RegisterCodeFix(
            CodeAction.Create(
                title: TitleSyncParam,
                createChangedDocument: ct =>
                    SyncParameterToAttributeAsync(document, methodDecl, config.ShortName, attrMember, ct),
                equivalenceKey: TitleSyncParam),
            diagnostic);
    }

    // ── Fix 1: update parameter default ──────────────────────────────────

    private static async Task<Document> SyncParameterToAttributeAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        String enumShortName,
        String targetMember,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var param = methodDecl.ParameterList.Parameters
            .FirstOrDefault(p =>
                p.Identifier.ValueText.Equals(
                    WellKnownTypes.EventIdParamName, StringComparison.OrdinalIgnoreCase)
                && p.Default is not null);
        if (param is null)
        {
            return document;
        }

        var newDefault = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(enumShortName),
                SyntaxFactory.IdentifierName(targetMember)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        return document.WithSyntaxRoot(
            root.ReplaceNode(param, param.WithDefault(newDefault)));
    }

    // ── Fix 2: update attribute EventId expression ───────────────────────

    private static async Task<Document> SyncAttributeToParameterAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        SemanticModel model,
        EventIdsTypeConfig config,
        String targetMember,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var loggerMsgAttr = AnalyzerHelpers.FindAttributeSyntax(
            methodDecl.AttributeLists, model, WellKnownTypes.LoggerMessageAttr);
        if (loggerMsgAttr?.ArgumentList is null)
        {
            return document;
        }

        var eventIdArg = loggerMsgAttr.ArgumentList.Arguments
            .FirstOrDefault(a =>
                a.NameEquals?.Name.Identifier.ValueText == WellKnownTypes.EventIdArgName);
        if (eventIdArg is null)
        {
            return document;
        }

        // Resolve the integer value for targetMember from the compiled type.
        var eventIdsType = config.GetTypeSymbol(model.Compilation);
        if (eventIdsType is null)
        {
            return document;
        }

        var memberField = eventIdsType.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.Name.Equals(targetMember, StringComparison.Ordinal));
        if (memberField?.HasConstantValue != true)
        {
            return document;
        }

        // Build:  (int)EnumType.targetMember
        var newExpr = SyntaxFactory.CastExpression(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(config.ShortName),
                SyntaxFactory.IdentifierName(targetMember)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        return document.WithSyntaxRoot(
            root.ReplaceNode(eventIdArg, eventIdArg.WithExpression(newExpr)));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static (String? attrMember, String? paramMember) ExtractMembers(
        String msg, String shortName)
    {
        // messageFormat: "The [LoggerMessage] EventId ({ShortName}.AttrMember) does not
        //                 match the default value of parameter 'eventId' ({ShortName}.ParamMember)."
        var prefix = $"{shortName}.";

        var idx1 = msg.IndexOf(prefix, StringComparison.Ordinal);
        if (idx1 < 0)
        {
            return (null, null);
        }

        var start1 = idx1 + prefix.Length;
        var end1 = msg.IndexOfAny([')', ' ', ','], start1);
        if (end1 < 0)
        {
            end1 = msg.Length;
        }

        var attrMember = msg.Substring(start1, end1 - start1);

        var idx2 = msg.IndexOf(prefix, end1, StringComparison.Ordinal);
        if (idx2 < 0)
        {
            return (attrMember, null);
        }

        var start2 = idx2 + prefix.Length;
        var end2 = msg.IndexOfAny([')', ' ', ','], start2);
        if (end2 < 0)
        {
            end2 = msg.Length;
        }

        var paramMember = msg.Substring(start2, end2 - start2);

        return (attrMember, paramMember);
    }
}

