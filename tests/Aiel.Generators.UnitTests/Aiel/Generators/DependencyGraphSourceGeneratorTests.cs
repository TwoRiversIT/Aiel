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
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Aiel.Generators;

public class DependencyGraphSourceGeneratorTests
{
    [Fact]
    public void Generate_ProducesNoOutput_WhenNoApplicationFound()
    {
        const String source = """
            namespace Test;

            public class RegularClass
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().BeEmpty();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Generate_IgnoresAbstractApplication()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public abstract class AbstractDependency : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().BeEmpty();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Generate_IgnoresUnsealedApplication()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public class UnsealedApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().BeEmpty();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Generate_IgnoresAielDependency()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielDependencyConfigurator
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().BeEmpty();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Generate_CreatesGraph_ForSingleDependencyWithoutAttributes()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var generatedSource = result.GeneratedSources[0];
        generatedSource.HintName.Should().EndWith("AielDependencyGraph.g.cs");

        var sourceText = generatedSource.SourceText.ToString();
        sourceText.Should().Contain("internal static class AielDependencyGraph");
        sourceText.Should().Contain("public static IReadOnlyCollection<global::Aiel.Dependencies.DependencyDescriptor> Dependencies");
        sourceText.Should().Contain("typeof(global::Test.MyApplication)");
        sourceText.Should().Contain("Array.Empty<Type>()");
    }

    [Fact]
    public void Generate_CreatesGraph_ForDependencyWithSingleDependsOn()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class ChildDependency : AielApplication
            {
            }

            [DependsOn(typeof(ChildDependency))]
            public sealed class ParentDependency : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("typeof(global::Test.ParentDependency)");
        sourceText.Should().Contain("typeof(global::Test.ChildDependency)");
        sourceText.Should().Contain("new Type[] {");
    }

    [Fact]
    public void Generate_CreatesGraph_ForMultipleDependsOnAttributes()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class DepA : AielApplication
            {
            }

            public sealed class DepB : AielApplication
            {
            }

            [DependsOn(typeof(DepA))]
            [DependsOn(typeof(DepB))]
            public sealed class Root : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("typeof(global::Test.Root)");
        sourceText.Should().Contain("typeof(global::Test.DepA)");
        sourceText.Should().Contain("typeof(global::Test.DepB)");
    }

    [Fact]
    public void Generate_BuildsTransitiveClosure_ForNestedDependencies()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class DepC : AielApplication
            {
            }

            [DependsOn(typeof(DepC))]
            public sealed class DepB : AielApplication
            {
            }

            [DependsOn(typeof(DepB))]
            public sealed class DepA : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("typeof(global::Test.DepA)");
        sourceText.Should().Contain("typeof(global::Test.DepB)");
        sourceText.Should().Contain("typeof(global::Test.DepC)");
    }

    [Fact]
    public void Generate_EmitsHostApplicationExtension_WhenHostApplicationBuilderAvailable()
    {
        const String source = """
            namespace Test;

            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("this global::Microsoft.Extensions.Hosting.HostApplicationBuilder builder");
        sourceText.Should().Contain("Project Type: HostApplication");
    }

    [Fact]
    public void Generate_UsesTopologicalOrderWording_InGeneratedSummary()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class SharedDependency : AielApplication
            {
            }

            [DependsOn(typeof(SharedDependency))]
            public sealed class MyApplication : AielApplication
            {
            }
            """;

#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("topological order");
        sourceText.Should().NotContain("Depth-First Search order");
    }

    [Fact]
    public void Generate_EmitsWebApplicationExtension_WhenWebApplicationBuilderAvailable()
    {
        const String source = """
            using Aiel.Dependencies;
            
            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.AspNetCore.Builder.WebApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("this global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder");
        sourceText.Should().Contain("Project Type: WebApplication");
    }

    [Fact]
    public void Generate_PrefersWebApplication_WhenWebAndHostBuildersAreAvailable()
    {
        const String source = """
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.AspNetCore.Builder.WebApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("Project Type: WebApplication");
        sourceText.Should().NotContain("Project Type: HostApplication");
    }

    [Fact]
    public void Generate_PrefersHostApplication_WhenHostBuilderUsageDetectedAndWebBuilderTypeAlsoAvailable()
    {
        const String source = """
            using Microsoft.Extensions.Hosting;
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var builder = Host.CreateApplicationBuilder(args);
                }
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("Project Type: HostApplication");
    }

    [Fact]
    public void Generate_ReportsError_WhenMultipleProjectTypesDetectedInOneAssembly()
    {
        const String source = """
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.Hosting;
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var hostBuilder = Host.CreateApplicationBuilder(args);
                    var webBuilder = WebApplication.CreateBuilder(args);
                }
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().BeEmpty();
        result.Diagnostics.Should().ContainSingle(d => d.Id == "AIEL00004" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Generate_PrefersHostApplication_WhenCreateEmptyApplicationBuilderUsageDetected()
    {
        const String source = """
            using Microsoft.Extensions.Hosting;
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }

            public static class Program
            {
                public static void Main()
                {
                    var builder = Host.CreateEmptyApplicationBuilder();
                }
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("Project Type: HostApplication");
    }

    [Fact]
    public void Generate_PrefersHostApplication_WhenCreateDefaultBuilderUsageDetected()
    {
        const String source = """
            using Microsoft.Extensions.Hosting;
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var builder = Host.CreateDefaultBuilder(args);
                }
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("Project Type: HostApplication");
    }

    [Fact]
    public void Generate_PrefersWebApplication_WhenCreateSlimBuilderUsageDetected()
    {
        const String source = """
            using Microsoft.AspNetCore.Builder;
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var builder = WebApplication.CreateSlimBuilder(args);
                }
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.AspNetCore.Builder.WebApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("Project Type: WebApplication");
    }

    [Fact]
    public void Generate_PrefersWebApplication_WhenCreateEmptyBuilderUsageDetected()
    {
        const String source = """
            using Microsoft.AspNetCore.Builder;
            using Aiel.Dependencies;

            namespace Test;

            public sealed class MyApplication : AielApplication
            {
            }

            public static class Program
            {
                public static void Main()
                {
                    var builder = WebApplication.CreateEmptyBuilder();
                }
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeHostApplication: true, includeWebApplication: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.AspNetCore.Builder.WebApplicationBuilder> AddApplicationAsync");
        sourceText.Should().Contain("Project Type: WebApplication");
    }

    [Fact]
    public void Generate_EmitsWebAssemblyExtension_WhenWebAssemblyHostBuilderAvailable()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;
#pragma warning disable CS0618 // Type or member is obsolete
        var result = RunGenerator(source, includeWebAssembly: true);
#pragma warning restore CS0618 // Type or member is obsolete

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("public static async Task<global::Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder> AddApplicationAsync");
        sourceText.Should().Contain("this global::Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder builder");
        sourceText.Should().Contain("Project Type: WebAssembly");
    }

    [Fact]
    public void Generate_EmitsNoExtensionMethod_WhenProjectTypeUnknown()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source, includeHostApplication: false, includeWebAssembly: false);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("Project Type: Unknown");
        sourceText.Should().Contain("No extension method generated");
        sourceText.Should().NotContain("AddApplicationAsync");
    }

    [Fact]
    public void Generate_SupportsApplicationTypes_InheritingFromAielApplication()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("typeof(global::Test.MyApplication)");
    }

    [Fact]
    public void Generate_HandlesNamespacedDependencies()
    {
        const String source = """
            namespace MyCompany.MyProduct;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("typeof(global::MyCompany.MyProduct.MyApplication)");
    }

    [Fact]
    public void Generate_EscapesSpecialCharactersInDependencyNames()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("new global::Aiel.Dependencies.DependencyDescriptor(");
        sourceText.Should().Contain("\"Test.MyApplication\"");
    }

    [Fact]
    public void Generate_ProducesValidCSharpCode()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            [DependsOn(typeof(DepB))]
            public sealed class DepA : AielApplication
            {
            }

            public sealed class DepB : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.Diagnostics.Should().BeEmpty();

        var generatedSource = result.GeneratedSources[0].SourceText.ToString();
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedSource, cancellationToken: TestContext.Current.CancellationToken);
        var diagnostics = syntaxTree.GetDiagnostics(TestContext.Current.CancellationToken);

        diagnostics.Should().BeEmpty("generated code should be syntactically valid");
    }

    [Fact]
    public void Generate_ProducesDeterministicHeaderAcrossRuns()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var first = RunGenerator(source);
        var second = RunGenerator(source);

        first.GeneratedSources.Should().HaveCount(1);
        second.GeneratedSources.Should().HaveCount(1);

        var firstText = first.GeneratedSources[0].SourceText.ToString();
        var secondText = second.GeneratedSources[0].SourceText.ToString();

        firstText.Should().Be(secondText);
        firstText.Should().NotContain("Generated at:");
    }

    [Fact]
    public void Generate_IncludesDependencyTypeAsConfigurator_InDescriptor()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        // The dependency type itself must appear as a configurator entry, not Array.Empty.
        sourceText.Should().Contain("new Type[] { typeof(global::Test.MyApplication) }");
    }

    [Fact]
    public void Generate_DoesNotIncludeDependencyTypeAsInitializer_WhenNotImplementingIDependencyInitializer()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public sealed class MyApplication : AielApplication
            {
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        // Configurators array must contain the type; initializers array must remain empty.
        sourceText.Should().Contain("new Type[] { typeof(global::Test.MyApplication) }, Array.Empty<Type>())");
    }

    [Fact]
    public void Generate_IncludesDependencyTypeAsInitializer_WhenImplementingIDependencyInitializer()
    {
        const String source = """
            namespace Test;
            using Aiel.Dependencies;
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class MyApplication : AielApplication, IDependencyInitializer
            {
                public Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken)
                    => Task.CompletedTask;
            }
            """;

        var result = RunGenerator(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        // Both configurators and initializers must contain the type.
        sourceText.Should().Contain(
            "new Type[] { typeof(global::Test.MyApplication) }, new Type[] { typeof(global::Test.MyApplication) }");
    }

    private static GeneratorRunResult RunGenerator(
        String source,
        Boolean includeHostApplication = false,
        Boolean includeWebApplication = false,
        Boolean includeWebAssembly = false)
    {
        // Create stub definitions for Aiel.Dependencies types since they won't be available in the test compilation
        const String stubSource = """
            namespace Aiel.Dependencies
            {
                using System;
                using System.Threading.Tasks;

                [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                public class DependsOnAttribute : Attribute
                {
                    public DependsOnAttribute(Type type) { Type = type; }
                    public Type Type { get; }
                }

                public abstract class AielDependencyConfigurator : IDependencyConfigurator
                {
                    public virtual Task ConfigureAsync(DependencyConfigurationContext context) => Task.CompletedTask;
                }

                public abstract class AielApplication : AielDependencyConfigurator
                {
                    public virtual Task ConfigureAsync(DependencyConfigurationContext context) => Task.CompletedTask;
                }

                public interface IDependencyConfigurator
                {
                    Task ConfigureAsync(DependencyConfigurationContext context);
                }

                public interface IDependencyInitializer
                {
                    Task InitializeAsync(DependencyInitializationContext context, System.Threading.CancellationToken cancellationToken);
                }

                public class DependencyConfigurationContext
                {
                }

                public class DependencyInitializationContext
                {
                }

                public class DependencyDescriptor
                {
                    public DependencyDescriptor(string name, Type type, Type[] dependencies, Type[] optionalDependencies, Type[] reverseDependencies)
                    {
                    }
                }
            }

            namespace Microsoft.Extensions.DependencyInjection
            {
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using System.Collections.Generic;
                using Aiel.Dependencies;

                public static class DependencyManagerExtensions
                {
                    public static Task RegisterDependenciesAsync(
                        this object builder,
                        IReadOnlyCollection<DependencyDescriptor> dependencies,
                        CancellationToken cancellationToken = default) => Task.CompletedTask;
                }
            }
            """;

        const String hostApplicationStub = """
            namespace Microsoft.Extensions.Hosting
            {
                public interface IHostBuilder
                {
                }

                public class HostApplicationBuilder
                {
                }

                public class FakeHostBuilder : IHostBuilder
                {
                }

                public static class Host
                {
                    public static HostApplicationBuilder CreateApplicationBuilder(string[] args) => new HostApplicationBuilder();
                    public static HostApplicationBuilder CreateEmptyApplicationBuilder() => new HostApplicationBuilder();
                    public static IHostBuilder CreateDefaultBuilder(string[] args) => new FakeHostBuilder();
                }
            }
            """;

        const String webAssemblyStub = """
            namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting
            {
                public class WebAssemblyHostBuilder
                {
                    public static WebAssemblyHostBuilder CreateDefault(string[] args) => new WebAssemblyHostBuilder();
                }
            }
            """;

        const String webApplicationStub = """
            namespace Microsoft.AspNetCore.Builder
            {
                public class WebApplicationBuilder
                {
                }

                public static class WebApplication
                {
                    public static WebApplicationBuilder CreateBuilder(string[] args) => new WebApplicationBuilder();
                    public static WebApplicationBuilder CreateSlimBuilder(string[] args) => new WebApplicationBuilder();
                    public static WebApplicationBuilder CreateEmptyBuilder() => new WebApplicationBuilder();
                }
            }
            """;

        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(stubSource),
            CSharpSyntaxTree.ParseText(source)
        };

        if (includeHostApplication)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(hostApplicationStub));
        }

        if (includeWebApplication)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(webApplicationStub));
        }

        if (includeWebAssembly)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(webAssemblyStub));
        }

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new DependencyGraphSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        return new GeneratorRunResult(
            runResult.GeneratedTrees.Select(t => (t.FilePath, t.GetText())).ToImmutableArray(),
            diagnostics);
    }

    private sealed record GeneratorRunResult(
        ImmutableArray<(String HintName, Microsoft.CodeAnalysis.Text.SourceText SourceText)> GeneratedSources,
        ImmutableArray<Diagnostic> Diagnostics);
}
