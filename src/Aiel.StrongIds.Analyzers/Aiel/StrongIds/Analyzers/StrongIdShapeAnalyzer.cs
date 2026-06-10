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

using Aiel.StrongIds.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.StrongIds.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StrongIdShapeAnalyzer : DiagnosticAnalyzer
{
    private const String StrongIdAttributeName = "StrongIdAttribute";
    private const String StrongIdNamespace = "Aiel.StrongIds";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [
            DiagnosticDescriptors.StrongIdMustBePartialRecordType,
            DiagnosticDescriptors.StrongIdMustNotUsePositionalRecordSyntax,
            DiagnosticDescriptors.StrongIdMustNotDeclareValueMember,
            DiagnosticDescriptors.StrongIdMustNotDeclareInstanceConstructors,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        // Check if it has the StrongId attribute
        if (!HasStrongIdAttribute(typeSymbol))
        {
            return;
        }

        var displayName = typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

        // Validate shape: must be partial record (struct or sealed class)
        if (!IsValidStrongIdShape(typeSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustBePartialRecordType,
                typeSymbol.Locations.FirstOrDefault(),
                displayName));
            return;
        }

        // Validate: must not use positional record syntax
        if (UsesPositionalRecordSyntax(typeSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustNotUsePositionalRecordSyntax,
                typeSymbol.Locations.FirstOrDefault(),
                displayName));
        }

        var userTypeDeclarations = GetNonGeneratedTypeDeclarations(typeSymbol, context.CancellationToken);

        // Validate: must not declare a Value member
        if (DeclaresValueMember(userTypeDeclarations, context.Compilation))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustNotDeclareValueMember,
                typeSymbol.Locations.FirstOrDefault(),
                displayName));
        }

        // Validate: must not declare instance constructors
        if (DeclaresInstanceConstructors(userTypeDeclarations))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustNotDeclareInstanceConstructors,
                typeSymbol.Locations.FirstOrDefault(),
                displayName));
        }
    }

    private static Boolean HasStrongIdAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes()
            .Any(static attr => IsStrongIdAttribute(attr.AttributeClass));
    }

    private static Boolean IsStrongIdAttribute(INamedTypeSymbol? attributeClass)
    {
        if (attributeClass is null)
        {
            return false;
        }

        var originalDefinition = attributeClass.OriginalDefinition;

        return originalDefinition.Name == StrongIdAttributeName
            && originalDefinition.Arity == 1
            && originalDefinition.ContainingNamespace.ToDisplayString() == StrongIdNamespace;
    }

    private static Boolean IsValidStrongIdShape(INamedTypeSymbol symbol)
    {
        // Must not be nested
        if (symbol.ContainingType is not null)
        {
            return false;
        }

        // Must be a record
        if (!symbol.IsRecord)
        {
            return false;
        }

        // Must be partial
        if (!IsPartial(symbol))
        {
            return false;
        }

        // Must be a struct or sealed class
        if (symbol.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        return symbol.TypeKind == TypeKind.Class && symbol.IsSealed;
    }

    private static Boolean IsPartial(INamedTypeSymbol symbol)
    {
        var syntaxNodes = symbol.DeclaringSyntaxReferences
            .Select(static syntaxReference => syntaxReference.GetSyntax())
            .ToList();

        var typeDecls = syntaxNodes.OfType<TypeDeclarationSyntax>().ToList();
        var recordDecls = syntaxNodes.OfType<RecordDeclarationSyntax>().ToList();

        if (typeDecls.Count == 0 && recordDecls.Count == 0)
        {
            return false; // No syntax references found
        }

        return typeDecls.All(d => d.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            && recordDecls.All(d => d.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }

    private static Boolean UsesPositionalRecordSyntax(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(static syntaxReference => syntaxReference.GetSyntax())
            .OfType<RecordDeclarationSyntax>()
            .Any(static declaration => declaration.ParameterList is not null);
    }

    private static ImmutableArray<TypeDeclarationSyntax> GetNonGeneratedTypeDeclarations(INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(static syntaxReference => syntaxReference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Where(declaration => !IsGeneratedSyntaxTree(declaration.SyntaxTree, cancellationToken))
            .ToImmutableArray();
    }

    private static Boolean IsGeneratedSyntaxTree(SyntaxTree tree, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(tree.FilePath))
        {
            return true;
        }

        if (!String.IsNullOrEmpty(tree.FilePath)
            && tree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var root = tree.GetRoot(cancellationToken);
        var leading = root.GetLeadingTrivia().ToFullString();
        return leading.IndexOf("<auto-generated", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static Boolean DeclaresValueMember(ImmutableArray<TypeDeclarationSyntax> typeDeclarations, Compilation compilation)
    {
        foreach (var declaration in typeDeclarations)
        {
            foreach (var member in declaration.Members)
            {
                if (member is FieldDeclarationSyntax field)
                {
                    if (field.Declaration.Variables.Any(static variable => variable.Identifier.ValueText == "Value"))
                    {
                        return true;
                    }

                    continue;
                }

                var declaredSymbol = compilation.GetSemanticModel(member.SyntaxTree).GetDeclaredSymbol(member);
                if (declaredSymbol?.Name == "Value")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static Boolean DeclaresInstanceConstructors(ImmutableArray<TypeDeclarationSyntax> typeDeclarations)
    {
        return typeDeclarations
            .SelectMany(static declaration => declaration.Members)
            .OfType<ConstructorDeclarationSyntax>()
            .Any(static constructor => !constructor.Modifiers.Any(SyntaxKind.StaticKeyword));
    }
}
