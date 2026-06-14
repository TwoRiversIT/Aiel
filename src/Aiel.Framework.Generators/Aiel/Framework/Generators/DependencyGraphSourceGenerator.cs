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

using Aiel.Framework.Generators.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace Aiel.Framework.Generators;

/// <summary>
/// Source generator that discovers Aiel dependencies rooted in the current application
/// and emits a generated dependency graph. This provides a strongly-typed, declarative
/// view of all dependencies and their dependencies without requiring runtime reflection.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DependencyGraphSourceGenerator : IIncrementalGenerator
{
    private const String AddApplicationMethod = "AddApplicationAsync";
    private const String ApplicationType = "AielApplicationConfigurator";
    private const String DependenciesProperty = "Dependencies";
    private const String DependencyDescriptor = "DependencyDescriptor";
    private const String DependencyManager = "DependencyManager";
    private const String DependsOn = "DependsOn";
    private const String DependsOnAttribute = DependsOn + "Attribute";
    private const String GeneratedClassName = "AielDependencyGraph";
    private const String GeneratedNamespace = "Microsoft.Extensions.DependencyInjection";
    private const String HostApplicationBuilder = "HostApplicationBuilder";
    private const String RegisterDependenciesMethod = "RegisterDependenciesAsync";
    private const String RootNamespace = "Aiel.Framework";
    private const String WebApplicationBuilder = "WebApplicationBuilder";
    private const String WebAssemblyBuilder = "WebAssemblyHostBuilder";

    private const String FqDependencyDescriptor = "global::" + RootNamespace + "." + DependencyDescriptor;
    //private const String FqDependsOn = "global::" + RootNamespace + "." + DependsOnAttribute;
    private const String FqIDependencyInitializer = "global::" + RootNamespace + ".IInitializer";

    private const String NsHostApplicationBuilder = "Microsoft.Extensions.Hosting." + HostApplicationBuilder;
    private const String FqHostApplicationBuilder = "global::" + NsHostApplicationBuilder;

    private const String FqApplication = "global::" + RootNamespace + "." + ApplicationType;

    private const String NsWebApplicationBuilder = "Microsoft.AspNetCore.Builder." + WebApplicationBuilder;
    private const String FqWebApplicationBuilder = "global::" + NsWebApplicationBuilder;

    private const String NsWebAssemblyBuilder = "Microsoft.AspNetCore.Components.WebAssembly.Hosting." + WebAssemblyBuilder;
    private const String FqWebAssemblyBuilder = "global::" + NsWebAssemblyBuilder;

    private const String NsIHostBuilder = "Microsoft.Extensions.Hosting.IHostBuilder";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Discover root AielApplicationConfigurator types in the current application project.
        // The generator then walks their [DependsOn] graph across referenced dependencies
        // to build a complete, compile-time view of the assembly dependency graph.
        var roots = context.SyntaxProvider
            .CreateSyntaxProvider(static (node, _) => IsCandidate(node), Transform)
            .Where(static symbol => symbol is not null)
            .Select(static (symbol, _) => symbol!);

        var collected = roots.Collect();

        // Detect the project type by inspecting the compilation for available builder types.
        var projectType = context.CompilationProvider
            .Select(static (compilation, _) => DetectProjectType(compilation));

        var combined = collected.Combine(projectType);

        context.RegisterSourceOutput(combined, static (spc, pair) => Emit(spc, pair.Left, pair.Right));
    }

    private static Boolean IsCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    private static INamedTypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken = default)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (symbol.IsAbstract)
        {
            return null;
        }

        if (!symbol.IsSealed)
        {
            return null;
        }

        if (InheritsFromApplication(symbol))
        {
            // Treat every concrete AielApplicationConfigurator defined in the current application project
            // as a root. The dependency closure is computed in Emit using [DependsOn] attributes.
            return symbol;
        }

        return null;
    }

    private static Boolean InheritsFromApplication(INamedTypeSymbol symbol)
    {
        for (var current = symbol.BaseType; current is not null; current = current.BaseType)
        {
            var fqSymbol = current.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (String.Equals(fqSymbol, FqApplication, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static Boolean ImplementsIDependencyInitializer(INamedTypeSymbol symbol)
    {
        return symbol.AllInterfaces.Any(i =>
            String.Equals(
                i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                FqIDependencyInitializer,
                StringComparison.Ordinal));
    }

    private static ProjectType DetectProjectType(Compilation compilation)
    {
        var projectTypeFromUsage = DetectProjectTypeFromBuilderUsage(compilation);
        if (projectTypeFromUsage == ProjectType.Ambiguous)
        {
            return ProjectType.Ambiguous;
        }

        if (projectTypeFromUsage != ProjectType.Unknown)
        {
            return projectTypeFromUsage;
        }

        var wapBuilder = compilation.GetTypeByMetadataName(NsWebApplicationBuilder);
        var hostBuilder = compilation.GetTypeByMetadataName(NsHostApplicationBuilder);

        if (wapBuilder is not null && hostBuilder is not null)
        {
            return ProjectType.Ambiguous;
        }

        // IMPORTANT! The order of these checks matters. We want to generate the correct
        // extension method for the actual project type.

        // Check for Blazor WebAssembly first. This builder is specific to
        // WebAssembly applications.
        var wasmBuilder = compilation.GetTypeByMetadataName(NsWebAssemblyBuilder);
        if (wasmBuilder is not null)
        {
            return ProjectType.WebAssembly;
        }

        // Check for ASP.NET Core web applications next. Web projects may also
        // have HostApplicationBuilder available, so this must come before the
        // generic host check.
        if (wapBuilder is not null)
        {
            return ProjectType.WebApplication;
        }

        // Check for generic host (Web or Worker) by looking for HostApplicationBuilder
        if (hostBuilder is not null)
        {
            return ProjectType.HostApplication;
        }

        return ProjectType.Unknown;
    }

    private static ProjectType DetectProjectTypeFromBuilderUsage(Compilation compilation)
    {
        var detectedProjectTypes = new HashSet<ProjectType>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var projectType = GetProjectTypeFromInvocation(semanticModel, invocation);
                if (projectType != ProjectType.Unknown)
                {
                    detectedProjectTypes.Add(projectType);
                }
            }
        }

        if (detectedProjectTypes.Count == 1)
        {
            return detectedProjectTypes.First();
        }

        if (detectedProjectTypes.Count > 1)
        {
            return ProjectType.Ambiguous;
        }

        return ProjectType.Unknown;
    }

    private static ProjectType GetProjectTypeFromInvocation(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
    {
        var builderProjectType = GetProjectTypeFromBuilderType(semanticModel.GetTypeInfo(invocation).Type);
        if (builderProjectType != ProjectType.Unknown)
        {
            return builderProjectType;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            return GetProjectTypeFromBuilderType(methodSymbol.ReturnType);
        }

        if (symbolInfo.CandidateSymbols.IsEmpty)
        {
            return ProjectType.Unknown;
        }

        var candidateProjectTypes = new HashSet<ProjectType>();
        foreach (var candidateSymbol in symbolInfo.CandidateSymbols)
        {
            if (candidateSymbol is not IMethodSymbol candidateMethodSymbol)
            {
                continue;
            }

            var candidateProjectType = GetProjectTypeFromBuilderType(candidateMethodSymbol.ReturnType);
            if (candidateProjectType != ProjectType.Unknown)
            {
                candidateProjectTypes.Add(candidateProjectType);
            }
        }

        if (candidateProjectTypes.Count == 1)
        {
            return candidateProjectTypes.First();
        }

        if (candidateProjectTypes.Count > 1)
        {
            return ProjectType.Ambiguous;
        }

        return ProjectType.Unknown;
    }

    private static ProjectType GetProjectTypeFromBuilderType(ITypeSymbol? typeSymbol)
    {
        var fullyQualifiedType = typeSymbol?.ToDisplayString();

        if (String.Equals(fullyQualifiedType, NsWebAssemblyBuilder, StringComparison.Ordinal))
        {
            return ProjectType.WebAssembly;
        }

        if (String.Equals(fullyQualifiedType, NsWebApplicationBuilder, StringComparison.Ordinal))
        {
            return ProjectType.WebApplication;
        }

        if (String.Equals(fullyQualifiedType, NsHostApplicationBuilder, StringComparison.Ordinal))
        {
            return ProjectType.HostApplication;
        }

        if (String.Equals(fullyQualifiedType, NsIHostBuilder, StringComparison.Ordinal))
        {
            return ProjectType.HostApplication;
        }

        return ProjectType.Unknown;
    }

    private static void Emit(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> rootDependencies, ProjectType projectType)
    {

        if (rootDependencies.IsDefaultOrEmpty)
        {
            return;
        }

        if (projectType == ProjectType.Ambiguous)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.AmbiguousProjectType, Location.None));
            return;
        }

        // Build the transitive closure of dependencies reachable from the roots via [DependsOn].
        var comparer = SymbolEqualityComparer.Default;
        var graph = new Dictionary<INamedTypeSymbol, List<DependencyReference>>(comparer);
        var unresolvedDependencies = new Dictionary<String, DependencyReference>(StringComparer.Ordinal);
        var queue = new Queue<INamedTypeSymbol>();
        var seenRoots = new HashSet<INamedTypeSymbol>(comparer);

        foreach (var root in rootDependencies)
        {
            if (root is null)
            {
                continue;
            }

            if (seenRoots.Add(root))
            {
                queue.Enqueue(root);
            }
        }

        while (queue.Count > 0)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var current = queue.Dequeue();
            if (graph.ContainsKey(current))
            {
                continue;
            }

            var childDependencies = new List<DependencyReference>();
            graph[current] = childDependencies;

            foreach (var attributeData in current.GetAttributes())
            {
                if (!IsDependsOnAttribute(attributeData.AttributeClass))
                {
                    continue;
                }

                foreach (var dependencyReference in GetDependsOnDependencies(attributeData, current))
                {
                    childDependencies.Add(dependencyReference);

                    if (dependencyReference.Symbol is INamedTypeSymbol dependencySymbol)
                    {
                        if (!graph.ContainsKey(dependencySymbol))
                        {
                            queue.Enqueue(dependencySymbol);
                        }

                        continue;
                    }

                    unresolvedDependencies[dependencyReference.DisplayName] = dependencyReference;
                }
            }
        }

        var builder = new StringBuilder();

        builder.AppendLine(Header(GeneratedClassName));
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Threading;");
        builder.AppendLine("using System.Threading.Tasks;");
        builder.AppendLine();

        // Emit the generated dependency graph and extension methods into the
        // {GeneratedNamespace} namespace so they light up
        // alongside the existing host builder extensions.
        builder.AppendLine($"namespace {GeneratedNamespace};");
        builder.AppendLine();
        builder.AppendLine($"internal static class {GeneratedClassName}");
        builder.AppendLine("{");
        builder.AppendLine($"\tpublic static IReadOnlyCollection<{FqDependencyDescriptor}> {DependenciesProperty} {{ get; }} = new {FqDependencyDescriptor}[]");
        builder.AppendLine("\t{");

        var count = 0;

        var dependencies = graph.Keys.ToArray();
        for (var i = 0; i < dependencies.Length; i++)
        {
            var dependencySymbol = dependencies[i];
            var dependencyName = dependencySymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            var safeDependencyName = dependencyName.Replace("\"", "\\\"");
            var dependencyTypeName = dependencySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var dependencyReferences = graph[dependencySymbol];

            builder.Append($"\t\tnew {FqDependencyDescriptor}(\"{safeDependencyName}\", typeof({dependencyTypeName})");
            builder.Append(", ");

            if (dependencyReferences.Count == 0)
            {
                builder.Append("Array.Empty<Type>()");
            }
            else
            {
                builder.AppendLine("new Type[] { ");
                for (var j = 0; j < dependencyReferences.Count; j++)
                {
                    count++;

                    if (j > 0)
                    {
                        builder.AppendLine(", ");
                    }

                    builder.Append($"\t\t\t\ttypeof({dependencyReferences[j].TypeExpression})");
                }

                builder.AppendLine();
                builder.Append("\t\t\t}");
            }

            // Configurators: AielDependencyConfigurator implements IDependencyConfigurator, so include the type itself.
            builder.Append($", new Type[] {{ typeof({dependencyTypeName}) }}");

            // Initializers: include the type only when it also implements IInitializer.
            if (ImplementsIDependencyInitializer(dependencySymbol))
            {
                builder.Append($", new Type[] {{ typeof({dependencyTypeName}) }}");
            }
            else
            {
                builder.Append(", Array.Empty<Type>()");
            }

            builder.Append(")");
            if (i < dependencies.Length - 1)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        var unresolvedDependencyValues = unresolvedDependencies.Values.ToArray();
        for (var i = 0; i < unresolvedDependencyValues.Length; i++)
        {
            var unresolvedDependency = unresolvedDependencyValues[i];
            builder.AppendLine(",");
            builder.Append($"\t\tnew {FqDependencyDescriptor}(\"{unresolvedDependency.DisplayName}\", typeof({unresolvedDependency.TypeExpression}), Array.Empty<Type>(), new Type[] {{ typeof({unresolvedDependency.TypeExpression}) }}, Array.Empty<Type>())");
            builder.AppendLine();
        }

        builder.AppendLine("\t};");
        builder.AppendLine();

        // Emit the appropriate extension method based on project type
        EmitAddApplicationAsyncExtensionMethod(builder, projectType, dependencies[0], count);

        builder.AppendLine("}");

        var code = builder.ToString();
        if (code.IndexOf("\r\n") < 0)
        {
            code = code.Replace("\n", "\r\n");
        }

        context.AddSource($"{GeneratedClassName}.g.cs", code);
    }

    private static void EmitAddApplicationAsyncExtensionMethod(StringBuilder builder, ProjectType projectType, INamedTypeSymbol rootNamedTypeSymbol, Int32 count)
    {
        if (projectType != ProjectType.Unknown)
        {
            builder.AppendLine($"\t// Project Type: {projectType}");
            builder.AppendLine("\t/// <summary>");
            if (count == 0)
            {
                builder.AppendLine($"\t/// WARNING: No dependencies discovered. {rootNamedTypeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}");
                builder.AppendLine($"\t/// does not have any [{DependsOn}(typeof(<dependency>))] attributes.");
            }
            else
            {
                builder.AppendLine("\t/// Registers application dependencies and configures them asynchronously in topological order (dependencies before dependents).");
            }

            builder.AppendLine("\t/// </summary>");
        }

        var builderType = projectType switch
        {
            ProjectType.HostApplication => FqHostApplicationBuilder,
            ProjectType.WebApplication => FqWebApplicationBuilder,
            ProjectType.WebAssembly => FqWebAssemblyBuilder,
            _ => null
        };

        if (builderType is null)
        {
            // Don't emit an extension method if we can't determine the project type
            builder.AppendLine("\t// Project Type: Unknown");
            builder.AppendLine("\t// No extension method generated: unable to determine project type.");
            builder.AppendLine($"\t// Use the `{GeneratedClassName}.{DependenciesProperty}` property directly with `{DependencyManager}`, or fallback to the `{RegisterDependenciesMethod}<TDependency>()` method.");
        }
        else
        {
            builder.AppendLine($"\tpublic static async Task<{builderType}> {AddApplicationMethod}(");
            builder.AppendLine($"\t\tthis {builderType} builder,");
            builder.AppendLine("\t\tCancellationToken cancellationToken = default)");
            builder.AppendLine("\t{");
            builder.AppendLine($"\t\tawait builder.{RegisterDependenciesMethod}({DependenciesProperty}, cancellationToken);");
            builder.AppendLine("\t\treturn builder;");
            builder.AppendLine("\t}");
        }
    }

    private static Boolean IsDependsOnAttribute(INamedTypeSymbol? attributeClass)
    {
        if (attributeClass is null)
        {
            return false;
        }

        return String.Equals(attributeClass.Name, DependsOnAttribute, StringComparison.Ordinal)
            && String.Equals(attributeClass.ContainingNamespace?.ToDisplayString(), RootNamespace, StringComparison.Ordinal);
    }

    private static IEnumerable<DependencyReference> GetDependsOnDependencies(AttributeData attributeData, INamedTypeSymbol current)
    {
        var foundDependency = false;

        if (attributeData.ConstructorArguments.Length > 0)
        {
            foreach (var argument in attributeData.ConstructorArguments)
            {
                foreach (var dependencySymbol in ExtractDependencySymbols(argument))
                {
                    foundDependency = true;
                    yield return CreateResolvedReference(dependencySymbol);
                }
            }
        }

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            if (!String.Equals(namedArgument.Key, "Type", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var dependencySymbol in ExtractDependencySymbols(namedArgument.Value))
            {
                foundDependency = true;
                yield return CreateResolvedReference(dependencySymbol);
            }
        }

        if (foundDependency)
        {
            yield break;
        }

        if (attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            yield break;
        }

        if (attributeSyntax.ArgumentList is null)
        {
            yield break;
        }

        foreach (var attributeArgument in attributeSyntax.ArgumentList.Arguments)
        {
            if (attributeArgument.Expression is not TypeOfExpressionSyntax typeOfExpression)
            {
                continue;
            }

            var dependencyTypeName = typeOfExpression.Type.ToString();
            if (dependencyTypeName.StartsWith("global::", StringComparison.Ordinal))
            {
                dependencyTypeName = dependencyTypeName.Substring("global::".Length);
            }

            INamedTypeSymbol? dependencySymbol = null;
            if (dependencyTypeName.IndexOf('.') >= 0)
            {
                dependencySymbol = current.ContainingAssembly.GetTypeByMetadataName(dependencyTypeName);
            }
            else
            {
                foreach (var namespacedType in current.ContainingNamespace.GetTypeMembers(dependencyTypeName))
                {
                    dependencySymbol = namespacedType;
                    break;
                }

                dependencySymbol ??= FindTypeBySimpleName(current.ContainingAssembly.GlobalNamespace, dependencyTypeName);

                if (dependencySymbol is null && attributeData.AttributeClass is not null)
                {
                    dependencySymbol = FindTypeBySimpleName(attributeData.AttributeClass.ContainingAssembly.GlobalNamespace, dependencyTypeName);
                }

                if (dependencySymbol is null)
                {
                    foreach (var module in current.ContainingAssembly.Modules)
                    {
                        foreach (var referencedAssembly in module.ReferencedAssemblySymbols)
                        {
                            dependencySymbol = FindTypeBySimpleName(referencedAssembly.GlobalNamespace, dependencyTypeName);
                            if (dependencySymbol is not null)
                            {
                                break;
                            }
                        }

                        if (dependencySymbol is not null)
                        {
                            break;
                        }
                    }
                }
            }

            if (dependencySymbol is not null)
            {
                yield return CreateResolvedReference(dependencySymbol);
                continue;
            }

            var unresolvedDependencyName = dependencyTypeName.IndexOf('.') >= 0
                ? dependencyTypeName
                : $"{RootNamespace}.{dependencyTypeName}";
            yield return CreateUnresolvedReference(unresolvedDependencyName);
        }
    }

    private static IEnumerable<INamedTypeSymbol> ExtractDependencySymbols(TypedConstant argument)
    {
        if (argument.Kind == TypedConstantKind.Array)
        {
            foreach (var value in argument.Values)
            {
                foreach (var nestedDependencySymbol in ExtractDependencySymbols(value))
                {
                    yield return nestedDependencySymbol;
                }
            }

            yield break;
        }

        if (argument.Value is INamedTypeSymbol dependencySymbol)
        {
            yield return dependencySymbol;
            yield break;
        }

        if (argument.Value is ITypeSymbol dependencyTypeSymbol
            && dependencyTypeSymbol is INamedTypeSymbol namedDependencyTypeSymbol)
        {
            yield return namedDependencyTypeSymbol;
        }
    }

    private static INamedTypeSymbol? FindTypeBySimpleName(INamespaceSymbol namespaceSymbol, String simpleTypeName)
    {
        foreach (var typeMember in namespaceSymbol.GetTypeMembers())
        {
            if (String.Equals(typeMember.Name, simpleTypeName, StringComparison.Ordinal))
            {
                return typeMember;
            }
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            var dependencySymbol = FindTypeBySimpleName(nestedNamespace, simpleTypeName);
            if (dependencySymbol is not null)
            {
                return dependencySymbol;
            }
        }

        return null;
    }

    private static DependencyReference CreateResolvedReference(INamedTypeSymbol symbol)
    {
        return new DependencyReference(
            symbol,
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    private static DependencyReference CreateUnresolvedReference(String unresolvedDependencyName)
    {
        return new DependencyReference(
            null,
            unresolvedDependencyName,
            $"global::{unresolvedDependencyName}");
    }

    private static String Header(String passName)
    {
        return $"""
            // <auto-generated>
            //   This file was brought to you by {ThisAssembly.AssemblyName}
            //   Generator: {nameof(DependencyGraphSourceGenerator)}
            //   Pass: {passName}
            //
            //   DO NOT EDIT THIS FILE BY HAND OR THE WORLD MAY END!
            //   (Seriously. The generator will overwrite your changes anyway.)
            //
            // </auto-generated>

            """;
    }

    private sealed class DependencyReference(INamedTypeSymbol? symbol, String displayName, String typeExpression)
    {
        public INamedTypeSymbol? Symbol { get; } = symbol;

        public String DisplayName { get; } = displayName;

        public String TypeExpression { get; } = typeExpression;
    }

    private enum ProjectType
    {
        Unknown,
        Ambiguous,
        HostApplication,
        WebApplication,
        WebAssembly
    }
}
