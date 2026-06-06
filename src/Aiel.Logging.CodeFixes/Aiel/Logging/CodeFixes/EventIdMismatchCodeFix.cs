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
/// Fixes AIEL00012 by synchronising the <c>eventId</c> parameter default with
/// the <c>[LoggerMessage]</c> attribute's <c>EventId</c> value, or vice-versa.
/// vice-versa).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EventIdMismatchCodeFix))]
[Shared]
public sealed class EventIdMismatchCodeFix : CodeFixProvider
{
    private const string TitleSyncParam = "Sync parameter default to match [LoggerMessage] EventId";
    private const string TitleSyncAttr = "Sync [LoggerMessage] EventId to match parameter default";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [DiagnosticDescriptors.EventIdMismatch.Id];

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

        var model = await document.GetSemanticModelAsync(context.CancellationToken)
                                  .ConfigureAwait(false);
        if (model is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        var node = root.FindNode(span);

        // Walk up to find the method declaration.
        var methodDecl = node.AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();
        if (methodDecl is null)
        {
            return;
        }

        // Locate attribute and parameter nodes for the two fix directions.
        if (model.GetDeclaredSymbol(methodDecl, context.CancellationToken) is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var aielType = AnalyzerHelpers.GetAielEventIdsType(model.Compilation);
        if (aielType is null)
        {
            return;
        }

        var attrData = AnalyzerHelpers.GetLoggerMessageAttribute(methodSymbol, model.Compilation);
        if (attrData is null)
        {
            return;
        }

        // Get both member names from the diagnostic message.
        var (attrMember, paramMember) = ExtractMembers(diagnostic);
        if (attrMember is null || paramMember is null)
        {
            return;
        }

        // ── Fix 1: update parameter default to match attribute ──────────
        context.RegisterCodeFix(
            CodeAction.Create(
                title: TitleSyncParam,
                createChangedDocument: ct =>
                    SyncParameterToAttributeAsync(document, methodDecl, attrMember, ct),
                equivalenceKey: TitleSyncParam),
            diagnostic);

        // ── Fix 2: update attribute EventId to match parameter ──────────
        context.RegisterCodeFix(
            CodeAction.Create(
                title: TitleSyncAttr,
                createChangedDocument: ct =>
                    SyncAttributeToParameterAsync(document, methodDecl, paramMember, aielType, ct),
                equivalenceKey: TitleSyncAttr),
            diagnostic);
    }

    // ── Transformation: sync parameter default → attribute value ─────────

    private static async Task<Document> SyncParameterToAttributeAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        string targetMember,   // from attribute – the correct member
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Find the optional AielEventIds parameter.
        var param = methodDecl.ParameterList.Parameters
            .FirstOrDefault(p =>
                p.Identifier.ValueText.Equals(
                    WellKnownTypes.EventIdParamName, StringComparison.OrdinalIgnoreCase)
                && p.Default is not null);

        if (param is null)
        {
            return document;
        }

        // Build new default: = AielEventIds.targetMember
        var newDefault = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(WellKnownTypes.AielEventIdsShort),
                SyntaxFactory.IdentifierName(targetMember)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newParam = param.WithDefault(newDefault);
        var newRoot = root.ReplaceNode(param, newParam);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Transformation: sync attribute EventId → parameter default ───────

    private static async Task<Document> SyncAttributeToParameterAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        string targetMember,   // from parameter – the correct member
        INamedTypeSymbol aielType,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var model = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (model is null)
        {
            return document;
        }

        // Find the [LoggerMessage] attribute on the method.
        var loggerMsgAttr = AnalyzerHelpers.FindAttributeSyntax(
            methodDecl.AttributeLists, model, WellKnownTypes.LoggerMessageAttr);
        if (loggerMsgAttr?.ArgumentList is null)
        {
            return document;
        }

        // Find the EventId named argument.
        var eventIdArg = loggerMsgAttr.ArgumentList.Arguments
            .FirstOrDefault(a =>
                a.NameEquals?.Name.Identifier.ValueText == WellKnownTypes.EventIdArgName);
        if (eventIdArg is null)
        {
            return document;
        }

        // Get integer value of targetMember from the enum.
        var memberField = aielType.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f =>
                f.Name.Equals(targetMember, StringComparison.Ordinal));

        if (memberField?.HasConstantValue != true)
        {
            return document;
        }

        // Build: (int)AielEventIds.targetMember
        var newExpr = SyntaxFactory.CastExpression(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(WellKnownTypes.AielEventIdsShort),
                SyntaxFactory.IdentifierName(targetMember)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newArg = eventIdArg.WithExpression(newExpr);
        var newRoot = root.ReplaceNode(eventIdArg, newArg);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Parses the diagnostic message to extract attribute and parameter member names.
    /// messageFormat: "The [LoggerMessage] EventId ({0}) does not match the default value of parameter 'eventId' ({1}). They must agree."
    /// where {0} = "AielEventIds.AttrMember" and {1} = "AielEventIds.ParamMember".
    /// </summary>
    private static (string? attrMember, string? paramMember) ExtractMembers(Diagnostic diagnostic)
    {
        var msg = diagnostic.GetMessage();

        // Extract first AielEventIds.X occurrence
        const string prefix = "AielEventIds.";
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

        // Extract second occurrence
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

