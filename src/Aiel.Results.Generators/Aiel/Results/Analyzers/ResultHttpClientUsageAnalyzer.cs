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

using Aiel.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Results.Analyzers;

/// <summary>
/// Analyzer that detects generic HttpClient JSON method calls with Result types
/// and suggests using the specialized ResultHttpClientExtensions methods instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultHttpClientUsageAnalyzer : DiagnosticAnalyzer
{
    private static readonly String[] HttpClientJsonMethods =
    [
        "GetFromJsonAsync",
        "PostAsJsonAsync",
        "PutAsJsonAsync",
        "PatchAsJsonAsync",
        "DeleteAsync"
    ];

    private static readonly String[] HttpContentJsonMethods =
    [
        "ReadFromJsonAsync"
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.Prefer_ResultHttpClientExtensions];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(CheckInvocation, SyntaxKind.InvocationExpression);
    }

    private static void CheckInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var methodName = memberAccess.Name.Identifier.Text;

        // Check if this is a generic method call with Result<T> type argument
        if (memberAccess.Name is not GenericNameSyntax genericName ||
            genericName.TypeArgumentList.Arguments.Count == 0)
        {
            return;
        }

        var typeArg = genericName.TypeArgumentList.Arguments[0];

        if (context.SemanticModel.GetSymbolInfo(typeArg).Symbol is not ITypeSymbol typeSymbol)
        {
            return;
        }

        // Check if the type is Result or Result<T>
        var isResultType = IsResultType(typeSymbol);

        if (!isResultType)
        {
            return;
        }

        // Check if it's one of the methods we want to flag
        if (HttpClientJsonMethods.Contains(methodName) || HttpContentJsonMethods.Contains(methodName))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.Prefer_ResultHttpClientExtensions,
                invocation.GetLocation()
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static Boolean IsResultType(ITypeSymbol typeSymbol)
    {
        // Check if it's Result or Result<T>
        var fullName = typeSymbol.ToDisplayString();

        if (fullName == "Aiel.Results.Result")
        {
            return true;
        }

        // Check if it's Result<T>
        if (typeSymbol.Name == "Result" &&
            typeSymbol.ContainingNamespace?.ToDisplayString() == "Aiel.Results")
        {
            return true;
        }

        // Check if it's Result<IReadOnlyCollection<T>> or similar generic
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.Name == "Result" &&
            namedType.ContainingNamespace?.ToDisplayString() == "Aiel.Results")
        {
            return true;
        }

        return false;
    }
}
