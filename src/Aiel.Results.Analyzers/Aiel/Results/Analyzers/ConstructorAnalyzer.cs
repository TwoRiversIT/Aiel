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

using Aiel.Internal;
using Aiel.Results.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Results.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConstructorAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.DerivedErrorTypesMustHaveSingleStringConstructor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        // Must be a class
        if (context.Symbol is not INamedTypeSymbol typeSymbol || typeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Must derive from Error
        var baseType = typeSymbol.BaseType;
        if (baseType == null)
        {
            return;
        }

        if (!DerivesFromError(baseType))
        {
            return;
        }

        var overridesGenerateDescription = OverridesGenerateDescription(typeSymbol);
        var hasSingleStringConstructor = false;

        // Get all public instance constructors
        var ctors = typeSymbol.InstanceConstructors
            .Where(c => c.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedAndInternal or Accessibility.ProtectedOrInternal)
            .ToArray();

        foreach (var ctor in ctors)
        {
            var parameters = ctor.Parameters;

            // When GenerateDescription() is overridden, one constructor may have no parameters.
            if (parameters.Length == 0 && overridesGenerateDescription)
            {
                continue;
            }

            // When GenerateDescription() is not overridden, one constructor must accept a single string parameter.
            if (parameters.Length == 1 && !overridesGenerateDescription)
            {
                var param = parameters[0];
                if (param.Type.SpecialType != SpecialType.System_String)
                {
                    Report(context, typeSymbol);
                    return;
                }

                // I can see some crazy developer down the line deciding that the 'description' parameter should
                // contain JSON, Markdown, or a Base64‑encoded goat. We cannot really prevent that, but we can at
                // least ensure that the parameter is named 'description' so that JSON deserializers can map the
                // constructor parameter to the Description property.
                if (!String.Equals(param.Name, "description", StringComparison.Ordinal))
                {
                    Report(context, typeSymbol);
                    return;
                }

                hasSingleStringConstructor = true;
            }
        }

        if (!hasSingleStringConstructor && !overridesGenerateDescription)
        {
            Report(context, typeSymbol);
        }
    }

    private static Boolean OverridesGenerateDescription(INamedTypeSymbol typeSymbol)
        => typeSymbol.GetMembers(GeneratorConsts.GenerateDecriptionMethodName)
            .OfType<IMethodSymbol>()
            .Any(m => m.IsOverride);

    private static Boolean DerivesFromError(INamedTypeSymbol? type)
    {
        while (type != null)
        {
            if (type.Name == "Error" && type.ContainingNamespace.ToDisplayString().EndsWith(GeneratorConsts.Root))
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static void Report(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.DerivedErrorTypesMustHaveSingleStringConstructor,
            typeSymbol.Locations.FirstOrDefault(),
            typeSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }
}
