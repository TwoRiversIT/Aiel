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

using Aiel.Generators.Internal;
using Microsoft.CodeAnalysis.CSharp;
using GenerateCS = Aiel.Verifiers.SourceGeneratorVerifier<Aiel.Generators.DependencyGraphSourceGenerator>;
using VerifyCS = Aiel.Verifiers.CSharpSourceGeneratorVerifier<Aiel.Generators.DependencyGraphSourceGenerator>;

namespace Aiel.Generators;

public class DependencyGraphSourceGeneratorTests
{
    [Fact]
    public async Task Generate_ProducesNoOutput_WhenNoApplicationFound()
    {
        const String source = """
            namespace Test;

            public class RegularClass
            {
            }
            """;

        var generated = "";

        await VerifyCS.TestAsync(source, generated, includeHostApplication: true);
    }

    [Fact]
    public async Task Generate_IgnoresAbstractApplication()
    {
        // There shoud be an analyzer that reports abstract applications as a warning, but the
        // source generator should simply ignore them since they won't be processed correctly.

        const String source = """
            namespace Test;
            using Aiel.Dependencies;

            public abstract class AbstractDependency : AielApplication
            {
            }
            """;

        var generated = "";

        await VerifyCS.TestAsync(source, generated, includeHostApplication: true);
    }

    [Fact]
    public async Task Generate_IgnoresUnsealedApplication()
    {
        // There shoud be an analyzer that reports unsealed applications as a warning, but the
        // source generator should simply ignore them since they won't be processed correctly.

        const String source = """
                namespace Test;
                using Aiel.Dependencies;

                public class UnsealedApplication : AielApplication
                {
                }
                """;

        var generated = "";

        await VerifyCS.TestAsync(source, generated, includeWebApplication: true);
    }

    [Fact]
    public async Task Generate_IgnoresAielDependencyConfigurator()
    {
        // The dependency graph source generator should only consider types that inherit from
        // AielApplication, not AielDependencyConfigurator, since the latter are not intended
        // to be application entry point.
        const String source = """
                namespace Test;
                using Aiel.Dependencies;

                public sealed class MyApplication : AielDependencyConfigurator
                {
                }
                """;

        var generated = "";

        await VerifyCS.TestAsync(source, generated, includeWebAssembly: true);
    }

    [Fact]
    public void Generate_CreatesGraph_ForSingleDependencyWithSingleAttribute()
    {
        const String source = """
            using Aiel.Dependencies;
            using System;

            namespace Aiel.WorkerService;

            [DependsOn(typeof(AielFramework))]
            public sealed class AielWorkerService : AielApplication
            {
            }
            """;

        var result = GenerateCS.Generate(source, includeHostApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("\"Aiel.WorkerService.AielWorkerService\"");
        sourceText.Should().Contain("typeof(global::Aiel.WorkerService.AielWorkerService)");
        sourceText.Should().Contain("typeof(global::Aiel.AielFramework)");
        sourceText.Should().Contain("\"Aiel.AielFramework\"");
        sourceText.Should().Contain("// Project Type: HostApplication");
        sourceText.Should().Contain("Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
    }

    [Fact]
    public void Generate_CreatesGraph_FollowsDependsOnToRoot()
    {
        const String source = """

                namespace Aiel.WorkerService
                {
                    using Aiel.Dependencies;
                    using Aiel.WorkerService.Shared;

                    [DependsOn(typeof(AielWorkerServiceShared))]
                    public sealed class AielWorkerService : AielApplication
                    {
                    }
                }

                namespace Aiel.WorkerService.Shared
                {
                    using Aiel.Dependencies;

                    [DependsOn(typeof(AielFramework))]
                    public sealed class AielWorkerServiceShared : AielDependencyConfigurator;
                }
                """;

        var result = GenerateCS.Generate(source, includeHostApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("\"Aiel.WorkerService.AielWorkerService\"");
        sourceText.Should().Contain("typeof(global::Aiel.WorkerService.Shared.AielWorkerServiceShared)");
        sourceText.Should().Contain("\"Aiel.WorkerService.Shared.AielWorkerServiceShared\"");
        sourceText.Should().Contain("typeof(global::Aiel.AielFramework)");
        sourceText.Should().Contain("\"Aiel.AielFramework\"");
        sourceText.Should().Contain("// Project Type: HostApplication");
        sourceText.Should().Contain("Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
    }

    [Fact]
    public void Generate_CreatesGraph_ForMultipleDependsOnAttributes()
    {
        const String source = """
                using Aiel.Dependencies;

                namespace Test
                {
                    public sealed class DepA : AielDependencyConfigurator
                    {
                    }

                    public sealed class DepB : AielDependencyConfigurator
                    {
                    }

                    [DependsOn(typeof(DepA))]
                    [DependsOn(typeof(DepB))]
                    public sealed class Root : AielApplication
                    {
                    }
                }
                """;

        var result = GenerateCS.Generate(source, includeHostApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("\"Test.Root\"");
        sourceText.Should().Contain("typeof(global::Test.DepA)");
        sourceText.Should().Contain("typeof(global::Test.DepB)");
        sourceText.Should().Contain("\"Test.DepA\"");
        sourceText.Should().Contain("\"Test.DepB\"");
        sourceText.Should().Contain("// Project Type: HostApplication");
        sourceText.Should().Contain("Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
    }

    [Fact]
    public void Generate_BuildsTransitiveClosure_ForNestedDependencies()
    {
        const String source = """
            using Aiel;
            using Aiel.Dependencies;

            namespace Test
            {
                [DependsOn(typeof(DepB))]
                public sealed class DepA : AielApplication
                {
                }

                [DependsOn(typeof(DepC))]
                public sealed class DepB : AielDependencyConfigurator
                {
                }

                [DependsOn(typeof(AielFramework))]
                public sealed class DepC : AielDependencyConfigurator
                {
                }
            }
            """;

        var result = GenerateCS.Generate(source, includeHostApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        // All four nodes of the transitive closure must be present
        sourceText.Should().Contain("\"Test.DepA\"");
        sourceText.Should().Contain("typeof(global::Test.DepB)");
        sourceText.Should().Contain("\"Test.DepB\"");
        sourceText.Should().Contain("typeof(global::Test.DepC)");
        sourceText.Should().Contain("\"Test.DepC\"");
        sourceText.Should().Contain("typeof(global::Aiel.AielFramework)");
        sourceText.Should().Contain("\"Aiel.AielFramework\"");
        sourceText.Should().Contain("// Project Type: HostApplication");
        sourceText.Should().Contain("Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
    }

    [Fact]
    public void Generate_PrefersWebApplication_WhenWebAndHostBuildersAreAvailable()
    {
        const String source = """
            using Aiel.Dependencies;
            using Microsoft.AspNetCore.Builder;

            namespace Test
            {
                public sealed class MyApplication : AielApplication
                {
                }

                public static class Program
                {
                    public static void Main()
                    {
                        var builder = WebApplication.CreateBuilder();
                    }
                }
            }
            """;

        var result = GenerateCS.Generate(source, includeHostApplication: true, includeWebApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("// Project Type: WebApplication");
        sourceText.Should().Contain("Task<global::Microsoft.AspNetCore.Builder.WebApplicationBuilder> AddApplicationAsync");
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

        var result = GenerateCS.Generate(source, includeHostApplication: true, includeWebApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("// Project Type: HostApplication");
        sourceText.Should().Contain("Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
        sourceText.Should().NotContain("Project Type: WebApplication");
    }

    [Fact]
    public async Task Generate_RaisesDiagnostic_WhenMultipleProjectTypesDetectedInOneAssembly()
    {
        const String source = """
            using Aiel.Dependencies;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.Hosting;

            namespace Test
            {
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
            }
            """;

        await VerifyCS.TestAsync(source, String.Empty, expectedDiagnostics: [DiagnosticDescriptors.AmbiguousProjectType], includeHostApplication: true, includeWebApplication: true);
    }

    [Fact]
    public void Generate_PrefersHostApplication_WhenCreateEmptyApplicationBuilderUsageDetected()
    {
        const String source = """
                using Aiel.Dependencies;
                using Microsoft.Extensions.Hosting;

                namespace Test;

                public sealed class MyApplication : AielApplication
                {
                }

                public static class Program
                {
                    public static void Main()
                    {
                        var settings = new HostApplicationBuilderSettings()
                        {
                            EnvironmentName = "Testing"
                        };

                        var builder = Host.CreateEmptyApplicationBuilder(settings);
                    }
                }
                """;

        var result = GenerateCS.Generate(source, includeHostApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("// Project Type: HostApplication");
        sourceText.Should().Contain("Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
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

        var result = GenerateCS.Generate(source, includeHostApplication: true, includeWebApplication: true);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        sourceText.Should().Contain("Project Type: HostApplication");
        sourceText.Should().Contain("public static async Task<global::Microsoft.Extensions.Hosting.HostApplicationBuilder> AddApplicationAsync");
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

        var result = GenerateCS.Generate(source, includeHostApplication: true, includeWebApplication: true);

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

        var result = GenerateCS.Generate(source, includeHostApplication: true, includeWebApplication: true);

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

        var result = GenerateCS.Generate(source, includeWebAssembly: true);

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

        var result = GenerateCS.Generate(source, includeHostApplication: false, includeWebAssembly: false);

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

        var result = GenerateCS.Generate(source);

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

        var result = GenerateCS.Generate(source);

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

        var result = GenerateCS.Generate(source);

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

        var result = GenerateCS.Generate(source);

        result.GeneratedSources.Should().HaveCount(1);
        result.Diagnostics.Should().BeEmpty();

        var generatedSource = result.GeneratedSources[0].SourceText.ToString();
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedSource, cancellationToken: TestContext.Current.CancellationToken);
        var diagnostics = syntaxTree.GetDiagnostics(TestContext.Current.CancellationToken);

        diagnostics.Should().BeEmpty("generated source should be syntactically valid");
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

        var first = GenerateCS.Generate(source);
        var second = GenerateCS.Generate(source);

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

        var result = GenerateCS.Generate(source);

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

        var result = GenerateCS.Generate(source);

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

        var result = GenerateCS.Generate(source);

        result.GeneratedSources.Should().HaveCount(1);
        var sourceText = result.GeneratedSources[0].SourceText.ToString();

        // Both configurators and initializers must contain the type.
        sourceText.Should().Contain(
            "new Type[] { typeof(global::Test.MyApplication) }, new Type[] { typeof(global::Test.MyApplication) }");
    }
}
