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

using Aiel.StrongIds.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Aiel.StrongIds;

public class StrongIdSourceGeneratorTests
{
    [Fact]
    public void WithTypeGuid_EmitsGuidBackedRecordStructMembers()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();

        var generatedSource = result.GeneratedSources[0].SourceText.ToString();
        generatedSource.Should().Contain("public readonly partial record struct OrderId : global::Aiel.StrongIds.IStrongId<global::System.Guid>");
        generatedSource.Should().Contain("public global::System.Guid Value { get; }");
        generatedSource.Should().Contain("public OrderId(global::System.Guid value)");
        generatedSource.Should().Contain("public static OrderId From(global::System.Guid value) => new(value);");
        generatedSource.Should().Contain("public static bool TryFrom(global::System.Guid value, out OrderId id)");
        generatedSource.Should().Contain("public static bool TryParse(string? value, global::System.IFormatProvider? provider, out OrderId id)");
        generatedSource.Should().Contain("public bool IsDefault => Value == global::System.Guid.Empty;");
        generatedSource.Should().Contain("public override string ToString() => Value.ToString();");
    }

    [Fact]
    public void WhenAllowDefaultIsFalse_BackingTypeIsStruct_OmitsNone()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>(AllowDefault = false)]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();
        result.GeneratedSources[0].SourceText.ToString().Should().NotContain("public static readonly OrderId None = new(default);");
    }

    [Fact]
    public void WhenAllowDefaultIsTrue_BackingTypeIsStruct_IncludesNone()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>(AllowDefault = true)]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();
        result.GeneratedSources[0].SourceText.ToString().Should().Contain("public static readonly OrderId None = new(default);");
    }

    [Fact]
    public void WhenAllowDefaultIsTrue_BackingTypeIsString_IncludesNone()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<String>(AllowDefault = true)]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();
        result.GeneratedSources[0].SourceText.ToString().Should().Contain("public static readonly OrderId None = new(global::System.String.Empty);");
    }

    [Fact]
    public void WhenGenerateTryFromIsFalse_OmitsTryFrom()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>(GenerateTryFrom = false)]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();
        result.GeneratedSources[0].SourceText.ToString().Should().NotContain("TryFrom(");
    }

    [Fact]
    public void WhenGenerateTryParseIsFalse_OmitsTryParse()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>(GenerateTryParse = false)]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();
        result.GeneratedSources[0].SourceText.ToString().Should().NotContain("TryParse(");
    }

    [Fact]
    public void WhenAllowDefaultIsFalse_EmitsStringValidation_ForStringBackedIds()
    {
        const String source = """
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<string>]
            public readonly partial record struct ExternalSystemId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();

        var generatedSource = result.GeneratedSources[0].SourceText.ToString();
        generatedSource.Should().Contain("string.IsNullOrWhiteSpace(value)");
        generatedSource.Should().Contain("throw new global::System.ArgumentException(\"ExternalSystemId cannot be null, empty, or whitespace.\", nameof(value));");
        generatedSource.Should().Contain("Value = value.Trim();");
        generatedSource.Should().Contain("public bool IsDefault => Value == string.Empty;");
        generatedSource.Should().Contain("public override string ToString() => Value;");
    }

    [Fact]
    public void WhenTypeIsString_TrimsStringBackedStrongIdValues_WithoutChangingCase()
    {
        const String source = """
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<string>]
            public readonly partial record struct ExternalSystemId;
            """;

        var result = RunGenerator(source);
        var assembly = EmitAssembly(result);
        var type = assembly.GetType("Test.ExternalSystemId");
        var fromMethod = type?.GetMethod("From", BindingFlags.Public | BindingFlags.Static);

        fromMethod.Should().NotBeNull();

        var value = "  AbC-123_xYz  ";
        var id = fromMethod!.Invoke(null, [value]);
        var valueProperty = type!.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);

        valueProperty.Should().NotBeNull();
        valueProperty!.GetValue(id).Should().Be("AbC-123_xYz");
        id!.ToString().Should().Be("AbC-123_xYz");
    }

    [Fact]
    public void Generate_EmitsPrivateConstructor_ForReferenceBackedIds()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>(BackingKind = StrongIdBackingKind.Reference)]
            public sealed partial record OrderId;
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        result.GeneratedSources.Should().ContainSingle();
        result.GeneratedSources[0].SourceText.ToString().Should().Contain("private OrderId(global::System.Guid value)");
    }

    [Fact]
    public void WhenTypeIsNotPartialRecordType_SkipsSilently()
    {
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>]
            public readonly record struct OrderId;
            """;

        var result = RunGenerator(source);

        // Generator silently skips invalid shapes; diagnostics are handled by analyzers
        result.GeneratedSources.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void WhenPositionalRecordSyntaxUsed_EmitsCode()
    {
        // Even though positional syntax is invalid per the analyzer, the generator still emits code
        // for any partial record struct/sealed record shape. Diagnostics are handled by analyzers.
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>]
            public readonly partial record struct OrderId(Guid Value);
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
    }

    [Fact]
    public void WhenValueMemberAlreadyExists_EmitsCode()
    {
        // Even though declaring a Value member is invalid per the analyzer, the generator still emits code
        // for any partial record struct/sealed record shape. Diagnostics are handled by analyzers.
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>]
            public readonly partial record struct OrderId
            {
                public Guid Value { get; }
            }
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
    }

    [Fact]
    public void WithDefaultConditions_EmitsCode_WhenInstanceConstructorAlreadyExists()
    {
        // Even though declaring instance constructors is invalid per the analyzer, the generator still emits code
        // for any partial record struct/sealed record shape. Diagnostics are handled by analyzers.
        const String source = """
            using System;
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<Guid>]
            public readonly partial record struct OrderId
            {
                public OrderId(Guid value)
                {
                    Value = value;
                }
            }
            """;

        var result = RunGenerator(source);

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
    }

    [Fact]
    public void WhenBackingTypeIsUnsupported_SkipsSilently()
    {
        const String source = """
            using Aiel.StrongIds;

            namespace Test;

            [StrongId<decimal>]
            public readonly partial record struct OrderId;
            """;

        var result = RunGenerator(source);

        // Generator silently skips unsupported backing types; diagnostics are handled by analyzers
        result.GeneratedSources.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
    }

    private static GeneratorRunResult RunGenerator(String source, String strongIdNamespace = "Aiel.StrongIds")
    {
        List<SyntaxTree> syntaxTrees = [CSharpSyntaxTree.ParseText(source)];

        var references = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is String trustedPlatformAssemblies
            ? trustedPlatformAssemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
                .ToList()
            : [];

        var compilation = CSharpCompilation.Create(
            "StrongIdGeneratorTests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new StrongIdSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        var runResult = driver.GetRunResult();
        var compilationDiagnostics = outputCompilation.GetDiagnostics();

        return new GeneratorRunResult(
            runResult.GeneratedTrees.Select(static tree => (tree.FilePath, tree.GetText())).ToImmutableArray(),
            generatorDiagnostics,
            compilationDiagnostics,
            outputCompilation);
    }

    private static Assembly EmitAssembly(GeneratorRunResult result)
    {
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        using var stream = new MemoryStream();
        var emitResult = result.OutputCompilation.Emit(stream);

        emitResult.Success.Should().BeTrue(because: String.Join(Environment.NewLine, emitResult.Diagnostics));
        stream.Position = 0;

        return Assembly.Load(stream.ToArray());
    }

    private sealed record GeneratorRunResult(
        ImmutableArray<(String HintName, Microsoft.CodeAnalysis.Text.SourceText SourceText)> GeneratedSources,
        ImmutableArray<Diagnostic> GeneratorDiagnostics,
        ImmutableArray<Diagnostic> CompilationDiagnostics,
        Compilation OutputCompilation);
}
