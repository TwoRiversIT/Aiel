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
// NoDirectILoggerCallsAnalyzer.cs  –  AIEL00011
//
// Reports direct calls to ILogger extension methods (LogInformation, etc.)
// that occur outside a [LoggerMessage]-decorated method.
//
// Does not depend on the configured EventIds type — AIEL00011 fires whenever
// ILogger is called directly, regardless of which enum is in use.
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
using System.Linq;

namespace Aiel.Logging.Analyzers;

/// <summary>
/// AIEL00011 – Detects direct calls to <c>ILogger</c> extension methods
/// and requires callers to use <c>[LoggerMessage]</c>-decorated helpers instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoDirectILoggerCallsAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.NoDirectILoggerCalls];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationCtx =>
        {
            var config = AnalyzerConfiguration.Resolve(compilationCtx.Options);

            compilationCtx.RegisterSyntaxNodeAction(
                ctx => AnalyzeInvocation(ctx, config),
                SyntaxKind.InvocationExpression);
        });
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext ctx, EventIdsTypeConfig config)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var methodName = memberAccess.Name.Identifier.ValueText;
        if (!WellKnownTypes.DirectLoggerMethods.Contains(methodName))
        {
            return;
        }

        if (ctx.SemanticModel
            .GetSymbolInfo(invocation, ctx.CancellationToken)
            .Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var receiverType = methodSymbol.MethodKind == MethodKind.ReducedExtension || methodSymbol.ReducedFrom is not null
            ? (methodSymbol.ReducedFrom?.Parameters.FirstOrDefault()?.Type
                ?? methodSymbol.Parameters.FirstOrDefault()?.Type)
            : methodSymbol.IsExtensionMethod
                ? methodSymbol.Parameters.FirstOrDefault()?.Type
                : methodSymbol.ContainingType;

        if (receiverType is null)
        {
            return;
        }

        if (!AnalyzerHelpers.IsILogger(receiverType, ctx.Compilation))
        {
            return;
        }

        // Don't flag calls inside [LoggerMessage] implementations themselves.
        if (IsInsideLoggerMessageMethod(invocation, ctx.SemanticModel, ctx.Compilation))
        {
            return;
        }

        var props = AnalyzerConfiguration.BuildDiagnosticProperties(config);

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.NoDirectILoggerCalls,
            invocation.GetLocation(),
            properties: props,
            methodName));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static Boolean IsInsideLoggerMessageMethod(
        SyntaxNode node, SemanticModel model, Compilation compilation)
    {
        var ancestor = node.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (ancestor is null)
        {
            return false;
        }

        var enclosing = model.GetDeclaredSymbol(ancestor);
        return enclosing is not null
            && AnalyzerHelpers.HasLoggerMessageAttribute(enclosing, compilation);
    }
}
