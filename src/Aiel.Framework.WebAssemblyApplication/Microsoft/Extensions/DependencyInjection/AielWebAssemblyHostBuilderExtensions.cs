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

using Aiel.Framework;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class AielWebAssemblyHostBuilderExtensions
{
    public static async Task BootstrapAsync<TApplication>(
        this WebAssemblyHostBuilder builder,
        IEnumerable<DependencyDescriptor> dependencyDescriptors,
        CancellationToken cancellationToken = default)
        where TApplication : IApplicationConfigurator, new()
    {
        if (!dependencyDescriptors.Any())
        {
            throw new AielException("No dependency descriptors were provided. At least one dependency descriptor is required to bootstrap the application.");
        }

        var environment = await RegisterEnvironment<TApplication>(builder);

        var dependencyManager = new WebAssemblyDependencyManager(dependencyDescriptors);

        builder.Services.AddSingleton<IDependencyManager>(dependencyManager);

        var context = new ConfigurationContext(environment, builder.Services, builder.Configuration);

        await dependencyManager.ConfigureAsync(context, cancellationToken);
    }

    private static async Task<AielWebAssemblyEnvironment> RegisterEnvironment<TApplication>(WebAssemblyHostBuilder builder)
        where TApplication : IApplicationConfigurator, new()
    {
        var app = new TApplication();
        var environment = new AielWebAssemblyEnvironment()
        {
            ApplicationInstance = Guid.NewGuid(),
            ApplicationName = app.ApplicationName,
            ApplicationVersion = app.ApplicationVersion,
            BaseAddress = builder.HostEnvironment.BaseAddress,
            Environment = builder.HostEnvironment.Environment,
            EnvironmentName = builder.HostEnvironment.Environment
        };
        await app.SafelyDisposeAsync();

        builder.Services.AddSingleton<IAielEnvironment>(environment);

        return environment;
    }
}
