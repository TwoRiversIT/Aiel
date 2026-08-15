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
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class AielServiceCollectionExtensions
{
    public static async Task AddApplicationAsync<TApplication>(
        this IServiceCollection services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
        where TApplication : class, IApplicationConfigurator, new()
    {
        services.AddLogging();

        var environment = await services.RegisterEnvironment<TApplication>();

        var dependencyManager = new DependencyManager<TApplication>();

        services.AddSingleton<IDependencyManager>(dependencyManager);

        var context = new ConfigurationContext(environment, services, configuration);

        await dependencyManager.ConfigureAsync(context, cancellationToken);
    }

    private static async Task<AielEnvironment> RegisterEnvironment<TApplication>(this IServiceCollection services)
        where TApplication : class, IApplicationConfigurator, new()
    {
        var app = new TApplication();
        var environment = new AielEnvironment()
        {
            ApplicationInstance = Guid.NewGuid(),
            ApplicationName = app.ApplicationName,
            ApplicationVersion = app.ApplicationVersion,
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
        };
        await app.SafelyDisposeAsync();

        services.AddSingleton<IAielEnvironment>(environment);

        return environment;
    }
}
