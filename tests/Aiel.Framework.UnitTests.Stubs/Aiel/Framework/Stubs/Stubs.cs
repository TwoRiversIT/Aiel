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

using Aiel.Resources;

namespace Aiel.Framework.Stubs;

public static class Stubs
{
    // Load stub definitions for Aiel.Framework types from the original source files in the Aiel.Framework project.
    // This ensures that the stubs are always up to date with the actual code, and reduces the maintenance burden
    // of keeping the stubs in sync with the real code. It also means that when the tests fail due to changes in
    // the Aiel.Framework code, then either the test is now bad, or the source generator is now bad, but the stubs
    // are always correct, so we can be confident that the tests are failing for the right reasons.
    public static String[] AielDependencies => RH.GetStrings<Placeholder>(
        "AielApplicationConfigurator.txt",
        "AielDependency.txt",
        "AielDependencyConfigurator.txt",
        "AielEnvironment.txt",
        //"AielFrameworkAbstractions.txt", // Not needed for the tests, and triggers diagnostic warnings.
        "DependencyConfigurationContext.txt",
        "DependencyContext.txt",
        "DependencyDescriptor.txt",
        "DependencyInitializationContext.txt",
        "DependsOnAttribute.txt",
        "IConfigurator.txt",
        "IDependencyManager.txt",
        "IInitializer.txt",
        "ObservableServiceCollection.txt",
        "Usings.txt"
        ).ToArray();

    public const String HostApplication = """
            using Aiel.Framework;
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
                        where TApplication : AielApplicationConfigurator, new()
                    {
                        return builder;
                    }

                    public static async Task<IHostApplicationBuilder> RegisterDependenciesAsync(
                        this IHostApplicationBuilder builder,
                        IEnumerable<DependencyDescriptor> dependencyDescriptors,
                        CancellationToken cancellationToken = default)
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
