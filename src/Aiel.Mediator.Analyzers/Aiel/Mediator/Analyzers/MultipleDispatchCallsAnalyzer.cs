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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Aiel.Roslyn;
using System.Collections.Immutable;

namespace Aiel.Mediator.Analyzers;

/// <summary>
/// Warns when a single method body calls ISender or IPublisher dispatch methods more than once.
/// Each call creates its own DI scope; multiple calls usually indicate that related operations
/// should be composed into a single command/query or coordinated via a pipeline behavior.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MultipleDispatchCallsAnalyzer : DiagnosticAnalyzer
{
    // Matched by name so the analyzer does not need a compile-time reference to Aiel.Mediator.
    private const String ISenderTypeName = "ISender";
    private const String IPublisherTypeName = "IPublisher";

    private static readonly ImmutableHashSet<String> DispatchMethodNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "ExecuteAsync",
        "QueryAsync",
        "PublishAsync");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.MultipleDispatchCallsInMethod];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // OperationBlockStart lets us accumulate all invocations in a method body before
        // deciding whether to report — no partial hits, no per-invocation false positives.
        context.RegisterOperationBlockStartAction(OnOperationBlockStart);
    }

    private static void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
    {
        if (context.OwningSymbol is not IMethodSymbol method)
        {
            return;
        }

        // One list per method body. Roslyn fires RegisterOperationAction callbacks for a
        // single block sequentially on one thread, so no concurrent mutation occurs here.
        var callLocations = new List<Location>();

        context.RegisterOperationAction(
            opCtx => CollectDispatchCall(opCtx, callLocations),
            OperationKind.Invocation);

        context.RegisterOperationBlockEndAction(
            endCtx => ReportIfMultipleCalls(endCtx, method, callLocations));
    }

    private static void CollectDispatchCall(OperationAnalysisContext context, List<Location> locations)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        if (!DispatchMethodNames.Contains(invocation.TargetMethod.Name))
        {
            return;
        }

        // Resolve the static receiver type. For instance calls invocation.Instance is non-null;
        // for extension methods the first argument carries the receiver — but our dispatch
        // methods are not extension methods, so Instance always applies.
        var receiverType = invocation.Instance?.Type
                        ?? invocation.TargetMethod.ContainingType;

        if (receiverType is null || !IsDispatchInterface(receiverType))
        {
            return;
        }

        locations.Add(invocation.Syntax.GetLocation());
    }

    private static Boolean IsDispatchInterface(ITypeSymbol type)
    {
        // Accept the interface itself or any concrete type that implements it.
        if (IsTargetInterface(type))
        {
            return true;
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (IsTargetInterface(iface))
            {
                return true;
            }
        }

        return false;
    }

    private static Boolean IsTargetInterface(ITypeSymbol type)
        => type.TypeKind == TypeKind.Interface
        && (type.Name == ISenderTypeName || type.Name == IPublisherTypeName);

    private static void ReportIfMultipleCalls(
        OperationBlockAnalysisContext context,
        IMethodSymbol method,
        List<Location> callLocations)
    {
        if (callLocations.Count <= 1)
        {
            return;
        }

        // Primary location is the method declaration; additional locations highlight each call site.
        var primaryLocation = method.Locations.IsEmpty ? callLocations[0] : method.Locations[0];
        var additionalLocations = callLocations.ToImmutableArray();

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MultipleDispatchCallsInMethod,
            primaryLocation,
            additionalLocations,
            method.Name,
            callLocations.Count));
    }
}
