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

namespace Aiel.Verifiers;

public static class Stubs
{
    // Create stub definitions for Aiel.Dependencies types since they won't be available in the test compilation
    public const String AielDependencies = """
            using Aiel.Dependencies;
            using Microsoft.Extensions.DependencyInjection;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using System.Threading;
            using System;
                        
            namespace Aiel
            {
                public sealed class AielFramework : AielDependencyConfigurator
                {
                }
            }

            namespace Aiel.Dependencies
            {
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
                public static class DependencyManagerExtensions
                {
                    public static Task RegisterDependenciesAsync(
                        this Object builder,
                        IReadOnlyCollection<DependencyDescriptor> dependencies,
                        CancellationToken cancellationToken = default) => Task.CompletedTask;
                }
            }
            """;

    public const String HostApplication = """
            using Aiel.Dependencies;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;
            using System.Collections.Generic;
            using System.Threading.Tasks;
            using System.Threading;
            using System;

            namespace Microsoft.Extensions.DependencyInjection
            {
                public static class ApplicationRegistrationExtensions
                {
                    public static async Task<IHostApplicationBuilder> AddApplicationAsync<TApplication>(this IHostApplicationBuilder builder, CancellationToken cancellationToken = default)
                        where TApplication : AielApplication, new()
                    {
                        return builder;
                    }
                }
            }

            namespace Microsoft.Extensions.Hosting
            {
                public interface IHostBuilder
                {
                }

                public interface IHostApplicationBuilder
                {
                }

                public class HostApplicationBuilderSettings
                {
                    public string? EnvironmentName { get; set; }
                }

                public class HostApplicationBuilder : IHostApplicationBuilder
                {
                }

                public class FakeHostBuilder : IHostBuilder, IHostApplicationBuilder
                {
                }

                public static class Host
                {
                    public static HostApplicationBuilder CreateApplicationBuilder(string[] args) => new HostApplicationBuilder();
                    public static HostApplicationBuilder CreateEmptyApplicationBuilder(HostApplicationBuilderSettings? settings = null) => new HostApplicationBuilder();
                    public static IHostBuilder CreateDefaultBuilder(string[] args) => new FakeHostBuilder();
                }
            }
            """;

    public const String WebApplication = """
            namespace Microsoft.AspNetCore.Builder
            {
                public class WebApplicationBuilder
                {
                }

                public static class WebApplication
                {
                    public static WebApplicationBuilder CreateBuilder() => new WebApplicationBuilder();
                    public static WebApplicationBuilder CreateBuilder(string[] args) => new WebApplicationBuilder();
                    public static WebApplicationBuilder CreateSlimBuilder(string[] args) => new WebApplicationBuilder();
                    public static WebApplicationBuilder CreateEmptyBuilder() => new WebApplicationBuilder();
                }
            }
            """;

    public const String WebAssembly = """
            namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting
            {
                public class WebAssemblyHostBuilder
                {
                    public static WebAssemblyHostBuilder CreateDefault(string[] args) => new WebAssemblyHostBuilder();
                }
            }
            """;
}
