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
using System.Collections.Immutable;

namespace Aiel.Permissions.Analyzers;

/// <summary>
/// Ensures every concrete <c>IAction</c> implementation in the current assembly either has a
/// concrete <c>IActionPermissionChecker&lt;TAction&gt;</c> visible in the same compilation, or is
/// annotated with <c>[DoesNotRespectAuthority(Reason = "...")]</c>.
/// </summary>
/// <remarks>
/// <para>This analyzer is fail-closed: the absence of an authorization story is a compile-time error
/// (TRAF01001). An empty or whitespace <c>Reason</c> on the marker attribute is also an error (TRAF01002).</para>
/// <para>
/// Task 9 will add a third passing condition — a generated permission definition — without changing
/// the diagnostic IDs or existing passing behavior.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ActionAuthorizationAnalyzer : DiagnosticAnalyzer
{
    private const String IActionMetadataName = "Aiel.Actions.IAction";
    private const String IActionPermissionCheckerMetadataName = "Aiel.Permissions.IActionPermissionChecker`1";
    private const String DoesNotRespectAuthorityMetadataName = "Aiel.Permissions.DoesNotRespectAuthorityAttribute";
    private const String ReasonPropertyName = "Reason";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [
            PermissionDiagnosticDescriptors.ActionHasNoAuthorizationStory,
            PermissionDiagnosticDescriptors.DoesNotRespectAuthorityReasonIsEmpty,
        ];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var actionInterface = context.Compilation.GetTypeByMetadataName(IActionMetadataName);
        if (actionInterface is null)
        {
            // Compilation does not reference Aiel action contracts; nothing to check.
            return;
        }

        var checkerInterface = context.Compilation.GetTypeByMetadataName(IActionPermissionCheckerMetadataName);
        var markerAttribute = context.Compilation.GetTypeByMetadataName(DoesNotRespectAuthorityMetadataName);

        // Collect concrete checker coverage from the current assembly only.
        // Checkers are expected to live alongside or after the actions they guard.
        var coveredActions = CollectCoveredActions(context.Compilation.Assembly.GlobalNamespace, checkerInterface);

        // Only walk types defined in the current assembly — diagnostics can only be placed on source files.
        var actionTypes = CollectConcreteActionTypes(context.Compilation.Assembly.GlobalNamespace, actionInterface);

        foreach (var actionType in actionTypes)
        {
            AnalyzeActionType(context, actionType, coveredActions, markerAttribute);
        }
    }

    private static void AnalyzeActionType(
        CompilationAnalysisContext context,
        INamedTypeSymbol actionType,
        IReadOnlyCollection<INamedTypeSymbol> coveredActions,
        INamedTypeSymbol? markerAttribute)
    {
        var marker = FindMarkerAttribute(actionType, markerAttribute);

        if (marker is not null)
        {
            // Attribute is present — validate Reason.
            var reason = GetReasonValue(marker);
            if (String.IsNullOrWhiteSpace(reason))
            {
                var location = GetAttributeLocation(marker, context.CancellationToken)
                    ?? actionType.Locations.FirstOrDefault()
                    ?? Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                    PermissionDiagnosticDescriptors.DoesNotRespectAuthorityReasonIsEmpty,
                    location,
                    actionType.Name));
            }

            // Do NOT also report TRAF01001 when the marker is present, regardless of Reason validity.
            return;
        }

        // No marker — check for a concrete checker covering this action.
        if (coveredActions.Any(covered => SymbolEqualityComparer.Default.Equals(covered, actionType)))
        {
            return;
        }

        var typeLocation = actionType.Locations.FirstOrDefault() ?? Location.None;
        context.ReportDiagnostic(Diagnostic.Create(
            PermissionDiagnosticDescriptors.ActionHasNoAuthorizationStory,
            typeLocation,
            actionType.Name));
    }

    private static AttributeData? FindMarkerAttribute(INamedTypeSymbol actionType, INamedTypeSymbol? markerAttribute)
    {
        if (markerAttribute is null)
        {
            return null;
        }

        foreach (var attr in actionType.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, markerAttribute))
            {
                return attr;
            }
        }

        return null;
    }

    private static String? GetReasonValue(AttributeData attributeData)
    {
        foreach (var named in attributeData.NamedArguments)
        {
            if (named.Key == ReasonPropertyName)
            {
                return named.Value.Value as String;
            }
        }

        return null;
    }

    private static Location? GetAttributeLocation(AttributeData attributeData, CancellationToken cancellationToken)
    {
        var syntaxRef = attributeData.ApplicationSyntaxReference;
        if (syntaxRef is null)
        {
            return null;
        }

        var node = syntaxRef.GetSyntax(cancellationToken);
        var loc = node.GetLocation();
        return loc.IsInSource ? loc : null;
    }

    private static List<INamedTypeSymbol> CollectCoveredActions(
        INamespaceSymbol rootNamespace,
        INamedTypeSymbol? checkerInterface)
    {
        var covered = new List<INamedTypeSymbol>();
        if (checkerInterface is null)
        {
            return covered;
        }

        CollectCoveredActionsInNamespace(rootNamespace, checkerInterface, covered);
        return covered;
    }

    private static void CollectCoveredActionsInNamespace(
        INamespaceSymbol ns,
        INamedTypeSymbol checkerInterface,
        List<INamedTypeSymbol> covered)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                CollectCoveredActionsInNamespace(childNs, checkerInterface, covered);
            }
            else if (member is INamedTypeSymbol typeSymbol)
            {
                CollectCoveredActionsInType(typeSymbol, checkerInterface, covered);
            }
        }
    }

    private static void CollectCoveredActionsInType(
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol checkerInterface,
        List<INamedTypeSymbol> covered)
    {
        // Only concrete (non-abstract) classes can be instantiated as checkers.
        if (typeSymbol.TypeKind == TypeKind.Class && !typeSymbol.IsAbstract)
        {
            foreach (var iface in typeSymbol.AllInterfaces)
            {
                if (iface.IsGenericType
                    && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, checkerInterface)
                    && iface.TypeArguments.Length == 1
                    && iface.TypeArguments[0] is INamedTypeSymbol actionArg)
                {
                    covered.Add(actionArg);
                }
            }
        }

        foreach (var nested in typeSymbol.GetTypeMembers())
        {
            CollectCoveredActionsInType(nested, checkerInterface, covered);
        }
    }

    private static List<INamedTypeSymbol> CollectConcreteActionTypes(
        INamespaceSymbol rootNamespace,
        INamedTypeSymbol actionInterface)
    {
        var results = new List<INamedTypeSymbol>();
        CollectConcreteActionTypesInNamespace(rootNamespace, actionInterface, results);
        return results;
    }

    private static void CollectConcreteActionTypesInNamespace(
        INamespaceSymbol ns,
        INamedTypeSymbol actionInterface,
        List<INamedTypeSymbol> results)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                CollectConcreteActionTypesInNamespace(childNs, actionInterface, results);
            }
            else if (member is INamedTypeSymbol typeSymbol && IsConcreteActionType(typeSymbol, actionInterface))
            {
                results.Add(typeSymbol);
            }
        }
    }

    private static Boolean IsConcreteActionType(INamedTypeSymbol typeSymbol, INamedTypeSymbol actionInterface)
    {
        if (typeSymbol.TypeKind != TypeKind.Class)
        {
            return false;
        }

        if (typeSymbol.IsAbstract)
        {
            return false;
        }

        return typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, actionInterface));
    }
}
