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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Aiel.Permissions.Generators;

[Generator]
public sealed class PermissionDefinitionSourceGenerator : IIncrementalGenerator
{
    private const String DefinesPermissionAttributeMetadataName = "Aiel.Permissions.DefinesPermissionAttribute";

    private static readonly SymbolDisplayFormat GlobalFqnFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            DefinesPermissionAttributeMetadataName,
            static (node, _) => node is TypeDeclarationSyntax,
            static (attributeContext, _) => Transform(attributeContext));

        // Emit one checker file per action
        context.RegisterSourceOutput(candidates, static (ctx, model) => EmitChecker(ctx, model));

        // Emit one aggregate file for all actions
        context.RegisterSourceOutput(
            candidates.Collect(),
            static (ctx, models) => EmitAggregates(ctx, models));
    }

    private static PermissionModel Transform(GeneratorAttributeSyntaxContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var attribute = context.Attributes[0];

        var permissionName = (String)attribute.ConstructorArguments[0].Value!;
        var scopeType = (String)attribute.ConstructorArguments[1].Value!;
        var subjectType = (String)attribute.ConstructorArguments[2].Value!;
        var displayName = (String)attribute.ConstructorArguments[3].Value!;

        var description = String.Empty;
        var lifecycleValue = 0; // PermissionLifecycle.Active
        var previousNames = ImmutableArray<String>.Empty;
        String? stableId = null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Description":
                    description = (String?)namedArg.Value.Value ?? String.Empty;
                    break;
                case "Lifecycle":
                    lifecycleValue = (Int32)namedArg.Value.Value!;
                    break;
                case "PreviousNames":
                    previousNames = namedArg.Value.Values
                        .Select(v => (String)v.Value!)
                        .ToImmutableArray();
                    break;
                case "StableId":
                    stableId = (String?)namedArg.Value.Value;
                    break;
            }
        }

        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        return new PermissionModel(
            actionFqn: typeSymbol.ToDisplayString(GlobalFqnFormat),
            actionName: typeSymbol.Name,
            ns: ns,
            hintName: typeSymbol.ToDisplayString(),
            permissionName: permissionName,
            scopeType: scopeType,
            subjectType: subjectType,
            displayName: displayName,
            description: description,
            lifecycleValue: lifecycleValue,
            previousNames: previousNames,
            stableId: stableId ?? permissionName);
    }

    private static void EmitChecker(SourceProductionContext context, PermissionModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (model.Namespace is not null)
        {
            sb.AppendLine($"namespace {model.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"internal sealed class {model.ActionName}PermissionChecker(");
        sb.AppendLine($"    global::Aiel.Permissions.IPermissionGrantEvaluator evaluator,");
        sb.AppendLine($"    global::Aiel.Permissions.IPermissionScopeResolver<{model.ActionFqn}> scopeResolver,");
        sb.AppendLine($"    global::Aiel.Permissions.IPermissionSubjectResolver<{model.ActionFqn}> subjectResolver)");
        sb.AppendLine($"    : global::Aiel.Permissions.IActionPermissionChecker<{model.ActionFqn}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public async global::System.Threading.Tasks.Task<global::Aiel.Results.Result> CheckPermissionAsync(");
        sb.AppendLine($"        global::Aiel.Execution.IActionExecutionContext<{model.ActionFqn}> context,");
        sb.AppendLine($"        global::System.Threading.CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        var scopeResult = await scopeResolver.ResolveAsync(context, cancellationToken).ConfigureAwait(false);");
        sb.AppendLine("        if (!scopeResult.IsSuccess) { return global::Aiel.Results.Result.Failure(scopeResult.Error); }");
        sb.AppendLine();
        sb.AppendLine("        var subjectKey = subjectResolver.ResolveSubjectKey(context);");
        sb.AppendLine($"        var permissionName = global::Aiel.Permissions.PermissionName.From(\"{model.PermissionName}\");");
        sb.AppendLine($"        var subjectTypeName = global::Aiel.Permissions.PermissionSubjectTypeName.From(\"{model.SubjectType}\");");
        sb.AppendLine("        var decisionResult = await evaluator.EvaluateAsync(");
        sb.AppendLine("            permissionName,");
        sb.AppendLine("            scopeResult.Value.ScopeType,");
        sb.AppendLine("            scopeResult.Value.ScopeKey,");
        sb.AppendLine("            subjectTypeName,");
        sb.AppendLine("            subjectKey,");
        sb.AppendLine("            cancellationToken).ConfigureAwait(false);");
        sb.AppendLine();
        sb.AppendLine("        if (!decisionResult.IsSuccess) { return global::Aiel.Results.Result.Failure(decisionResult.Error); }");
        sb.AppendLine();
        sb.AppendLine("        return decisionResult.Value == global::Aiel.Permissions.PermissionGrantDecision.Granted");
        sb.AppendLine("            ? global::Aiel.Results.Result.Success()");
        sb.AppendLine("            : global::Aiel.Results.Result.Failure(");
        sb.AppendLine("                global::Aiel.Permissions.PermissionErrors.PermissionDenied(permissionName));");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        var hintName = $"{model.HintName}PermissionChecker.g.cs";
        context.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void EmitAggregates(SourceProductionContext context, ImmutableArray<PermissionModel> models)
    {
        if (models.IsDefaultOrEmpty)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("/// <summary>Generated permission name constants. Declared <c>partial</c> to allow hand-authored additions.</summary>");
        sb.AppendLine("public static partial class GeneratedPermissionNames");
        sb.AppendLine("{");
        foreach (var model in models)
        {
            sb.AppendLine($"    public const string {model.ActionName} = \"{model.PermissionName}\";");
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("/// <summary>Provides the generated <see cref=\"global::Aiel.Permissions.PermissionDefinitionManifest\"/> entries for registration at startup.</summary>");
        sb.AppendLine("public static partial class GeneratedPermissionManifests");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Returns all permission definition manifests produced by the source generator.</summary>");
        sb.AppendLine("    public static global::System.Collections.Generic.IEnumerable<global::Aiel.Permissions.PermissionDefinitionManifest> GetManifests()");
        sb.AppendLine("    {");
        foreach (var model in models)
        {
            sb.AppendLine($"        yield return new global::Aiel.Permissions.PermissionDefinitionManifest");
            sb.AppendLine("        {");
            sb.AppendLine($"            PermissionName = global::Aiel.Permissions.PermissionName.From(\"{model.PermissionName}\"),");
            sb.AppendLine($"            StableId = global::Aiel.Permissions.PermissionStableId.From(\"{model.StableId}\"),");
            sb.AppendLine($"            ActionType = typeof({model.ActionFqn}),");
            sb.AppendLine($"            ScopeType = global::Aiel.Permissions.PermissionScopeTypeName.From(\"{model.ScopeType}\"),");
            sb.AppendLine($"            SubjectType = global::Aiel.Permissions.PermissionSubjectTypeName.From(\"{model.SubjectType}\"),");
            sb.AppendLine($"            DisplayName = \"{EscapeString(model.DisplayName)}\",");
            sb.AppendLine($"            Description = \"{EscapeString(model.Description)}\",");
            sb.AppendLine($"            Lifecycle = global::Aiel.Permissions.PermissionLifecycle.{LifecycleName(model.LifecycleValue)},");
            sb.AppendLine($"            PreviousNames = {BuildPreviousNamesArray(model.PreviousNames)},");
            sb.AppendLine("        };");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("GeneratedPermissions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static String EscapeString(String value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static String BuildPreviousNamesArray(ImmutableArray<String> previousNames)
    {
        if (previousNames.IsDefaultOrEmpty)
        {
            return "global::System.Array.Empty<global::Aiel.Permissions.PermissionName>()";
        }

        var values = String.Join(
            ", ",
            previousNames.Select(name => $"global::Aiel.Permissions.PermissionName.From(\"{EscapeString(name)}\")"));

        return $"new global::Aiel.Permissions.PermissionName[] {{ {values} }}";
    }

    private static String LifecycleName(Int32 value) => value switch
    {
        0 => "Active",
        1 => "Deprecated",
        2 => "Removed",
        _ => "Active",
    };

    private sealed class PermissionModel(
        String actionFqn,
        String actionName,
        String? ns,
        String hintName,
        String permissionName,
        String scopeType,
        String subjectType,
        String displayName,
        String description,
        Int32 lifecycleValue,
        ImmutableArray<String> previousNames,
        String stableId)
    {
        public String ActionFqn { get; } = actionFqn;
        public String ActionName { get; } = actionName;
        public String? Namespace { get; } = ns;
        public String HintName { get; } = hintName;
        public String PermissionName { get; } = permissionName;
        public String ScopeType { get; } = scopeType;
        public String SubjectType { get; } = subjectType;
        public String DisplayName { get; } = displayName;
        public String Description { get; } = description;
        public Int32 LifecycleValue { get; } = lifecycleValue;
        public ImmutableArray<String> PreviousNames { get; } = previousNames;
        public String StableId { get; } = stableId;
    }
}
