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
// UseAielEventIdsAnalyzer.cs  –  AIEL00008
//
// Reports when the EventId argument of a [LoggerMessage] attribute is a
// raw integer literal instead of (int)<ConfiguredEventIds>.Replace_With_A_Valid_Member.
//
// The EventIds enum type is configurable (see AnalyzerConfiguration).
// Default: Aiel.Logging.AielEvent
// -----------------------------------------------------------------------

using Aiel.Internal;
using Aiel.Logging.Configuration;
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers;

/// <summary>
/// AIEL00008 – Detects raw integer literals used as the <c>EventId</c> argument
/// in <c>[LoggerMessage]</c> attributes and suggests the configured EventIds
/// enum cast instead.
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Resolve the configured EventIds type once per compilation so all
        // inner analysis callbacks share the same snapshot.
        context.RegisterCompilationStartAction(compilationCtx =>
        {
            var config = AnalyzerConfiguration.Resolve(compilationCtx.Options);

            compilationCtx.RegisterSyntaxNodeAction(
                ctx => AnalyzeAttribute(ctx, config),
                SyntaxKind.Attribute);
        });
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, EventIdsTypeConfig config)
    {
        var optionsProvider = context.Options.AnalyzerConfigOptionsProvider;

        // Prefer per-tree options if you need file-scoped values:
        var tree = context.Node.SyntaxTree;
        var treeOptions = optionsProvider.GetOptions(tree);
        if (treeOptions.TryGetValue($"build_property.{AnalyzerConfiguration.EditorConfigKey}", out var configuredValueFromTree))
        {
            // use configuredValueFromTree
            config = EventIdsTypeConfig.FromFullTypeName(configuredValueFromTree);
        }

        // Fallback to global options
        if (!optionsProvider.GlobalOptions.TryGetValue($"build_property.{AnalyzerConfiguration.MsBuildPropertyKey}", out var configuredValue))
        {
            configuredValue = "Aiel.AielEvent"; // your default
        }

        // Resolve the type by metadata name
        var compilation = context.SemanticModel.Compilation;
        var enumType = compilation.GetTypeByMetadataName(configuredValue);
        if (enumType is null)
        {
            // handle missing type (report diagnostic or fallback)
        }

        // continue analysis using enumType

        var attrNode = (AttributeSyntax)context.Node;

        // 1. Must be [LoggerMessage].
        var attrSymbol = context.SemanticModel
            .GetSymbolInfo(attrNode, context.CancellationToken)
            .Symbol?.ContainingType;

        if (attrSymbol?.ToDisplayString() != WellKnownTypes.LoggerMessageAttr)
        {
            return;
        }

        // 2. Find the EventId argument.
        var eventIdArg = FindEventIdArgument(attrNode);
        if (eventIdArg is null)
        {
            return;
        }

        // 3. Accept: (int)<ConfiguredType>.Member — reject everything else.
        if (AnalyzerHelpers.IsConfiguredEventIdsCast(
                eventIdArg.Expression, context.SemanticModel, config))
        {
            return;
        }

        // 4. Resolve the integer value and suggest the matching member name.
        var constantOpt = context.SemanticModel
            .GetConstantValue(eventIdArg.Expression, context.CancellationToken);

        var rawValue = constantOpt.HasValue ? Convert.ToString(constantOpt.Value) : "?";
        var suggestion = TrySuggestMember(constantOpt, context.Compilation, config);

        var props = AnalyzerConfiguration.BuildDiagnosticProperties(config);

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.UseAielEventIds,
            eventIdArg.Expression.GetLocation(),
            properties: props,
            rawValue,
            suggestion ?? AnalyzerConfiguration.DefaultMemberName));
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
            if (arg.NameEquals?.Name.Identifier.ValueText == WellKnownTypes.EventIdArgName)
            {
                return arg;
            }
        }

        // Positional: first argument when it looks like a numeric literal.
        if (attr.ArgumentList.Arguments.Count > 0)
        {
            var first = attr.ArgumentList.Arguments[0];
            if (first.NameEquals is null
                && first.Expression is LiteralExpressionSyntax lit
                && lit.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return first;
            }
        }

        return null;
    }

    private static String? TrySuggestMember(
        Optional<Object?> constantOpt,
        Compilation compilation,
        EventIdsTypeConfig config)
    {
        if (!constantOpt.HasValue || constantOpt.Value is null)
        {
            return null;
        }

        var enumType = config.GetTypeSymbol(compilation);
        if (enumType is null)
        {
            return null;
        }

        return AnalyzerHelpers.TryResolveMemberName(
            Convert.ToInt32(constantOpt.Value), enumType);
    }
}
