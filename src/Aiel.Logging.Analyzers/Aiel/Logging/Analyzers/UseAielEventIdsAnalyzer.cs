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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers;

/// <summary>
/// AIEL001 – Detects raw integer literals used as the <c>EventId</c> argument
/// in <c>[LoggerMessage]</c> attributes and suggests using the
/// <c>AielEventIds</c> enum instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAielEventIdsAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.UseAielEventIds];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        // Safe default: do not run on generated code, enable concurrent execution.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // We analyse attribute syntax on method declarations.
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext ctx)
    {
        var attrNode = (AttributeSyntax)ctx.Node;

        // 1. Resolve the attribute symbol – must be [LoggerMessage].
        var attrSymbol = ctx.SemanticModel
            .GetSymbolInfo(attrNode, ctx.CancellationToken)
            .Symbol?.ContainingType;

        if (attrSymbol?.ToDisplayString() != WellKnownTypes.LoggerMessageAttr)
        {
            return;
        }

        // 2. Find the EventId argument (named argument wins; positional at index 0).
        var eventIdArg = FindEventIdArgument(attrNode);
        if (eventIdArg is null)
        {
            return;
        }

        // 3. The expression must NOT be a cast of an AielEventIds member.
        //    Accept:  (int)AielEventIds.Foo   →  CastExpression whose operand is MemberAccess on AielEventIds
        //    Reject:  anything else (literal, other cast, constant, etc.)
        if (IsAielEventIdsCast(eventIdArg.Expression, ctx.SemanticModel))
        {
            return;
        }

        // 4. Try to determine the integer value so we can suggest a member name.
        var constantOpt = ctx.SemanticModel
            .GetConstantValue(eventIdArg.Expression, ctx.CancellationToken);

        var rawValue = constantOpt.HasValue ? Convert.ToString(constantOpt.Value) : "?";
        var suggestion = TrySuggestMember(constantOpt, ctx.Compilation);

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.UseAielEventIds,
            eventIdArg.Expression.GetLocation(),
            rawValue,
            suggestion ?? "SomeMember"));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static AttributeArgumentSyntax? FindEventIdArgument(AttributeSyntax attr)
    {
        if (attr.ArgumentList is null)
        {
            return null;
        }

        foreach (var arg in attr.ArgumentList.Arguments)
        {
            // Named argument: EventId = ...
            if (arg.NameEquals?.Name.Identifier.ValueText == WellKnownTypes.EventIdArgName)
            {
                return arg;
            }
        }

        // Positional: first argument for the common (eventId, level, message) overload
        if (attr.ArgumentList.Arguments.Count > 0)
        {
            var first = attr.ArgumentList.Arguments[0];
            // Only treat it as the event-id position when it looks like a number
            if (first.NameEquals is null
                && first.Expression is LiteralExpressionSyntax lit
                && lit.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return first;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="expr"/> is
    /// <c>(int)AielEventIds.SomeMember</c>.
    /// </summary>
    private static bool IsAielEventIdsCast(ExpressionSyntax expr, SemanticModel model)
    {
        if (expr is not CastExpressionSyntax cast)
        {
            return false;
        }

        // The cast target type must be int / Int32
        var castType = model.GetTypeInfo(cast.Type).Type;
        if (castType?.SpecialType != SpecialType.System_Int32)
        {
            return false;
        }

        // The operand must be a member access on AielEventIds
        if (cast.Expression is not MemberAccessExpressionSyntax ma)
        {
            return false;
        }

        var memberType = model.GetTypeInfo(ma.Expression).Type;
        return memberType?.ToDisplayString() == WellKnownTypes.AielEventIds;
    }

    private static string? TrySuggestMember(Optional<object?> constantOpt, Compilation compilation)
    {
        if (!constantOpt.HasValue || constantOpt.Value is null)
        {
            return null;
        }

        var enumType = AnalyzerHelpers.GetAielEventIdsType(compilation);
        if (enumType is null)
        {
            return null;
        }

        return AnalyzerHelpers.TryResolveEventIdMemberName(
            Convert.ToInt32(constantOpt.Value), enumType);
    }
}

