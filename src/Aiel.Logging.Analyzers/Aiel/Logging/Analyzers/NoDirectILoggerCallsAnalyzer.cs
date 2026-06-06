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

        context.RegisterSyntaxNodeAction(
            AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext ctx)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;

        // We only care about member-access invocations: receiver.MethodName(...)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var methodName = memberAccess.Name.Identifier.ValueText;

        // Quick name check before doing full symbol resolution (performance).
        if (!WellKnownTypes.DirectLoggerMethods.Contains(methodName))
        {
            return;
        }

        // Resolve the invoked method symbol.

        if (ctx.SemanticModel
            .GetSymbolInfo(invocation, ctx.CancellationToken)
            .Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // The first parameter of the extension method or the receiver type
        // must be ILogger / ILogger<T>.
        var receiverType = methodSymbol.IsExtensionMethod
            ? (methodSymbol.ReducedFrom?.Parameters.FirstOrDefault()?.Type
                ?? methodSymbol.Parameters.FirstOrDefault()?.Type)
            : methodSymbol.ContainingType;

        if (receiverType is null)
        {
            return;
        }

        if (!AnalyzerHelpers.IsILogger(receiverType, ctx.Compilation))
        {
            return;
        }

        // Exclude calls that are themselves inside a [LoggerMessage] partial
        // method implementation (the source-generated body).
        if (IsInsideLoggerMessageMethod(invocation, ctx.SemanticModel, ctx.Compilation))
        {
            return;
        }

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.NoDirectILoggerCalls,
            invocation.GetLocation(),
            methodName));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when the invocation node resides inside
    /// a method that is itself decorated with <c>[LoggerMessage]</c>.
    /// (We should not flag source-generator output or hand-written implementations
    /// of the logging helpers themselves.)
    /// </summary>
    private static bool IsInsideLoggerMessageMethod(
        SyntaxNode node,
        SemanticModel model,
        Compilation compilation)
    {
        // Walk up the syntax tree looking for a method declaration.
        var ancestor = node.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (ancestor is null)
        {
            return false;
        }

        var enclosingMethod = model.GetDeclaredSymbol(ancestor);
        if (enclosingMethod is null)
        {
            return false;
        }

        return AnalyzerHelpers.HasLoggerMessageAttribute(enclosingMethod, compilation);
    }
}

