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
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class AielHostApplicationBuilderExtensions
{
    /// <summary>
    /// This method is responsible for configuring the application. It builds the
    /// dependency tree by reflecting over <typeparamref name="TApplication"/> and
    /// following all the <see cref="DependsOnAttribute"/> attributes.
    /// </summary>
    /// <typeparam name="TApplication">A descendant of <see cref="AielApplicationConfigurator"/>.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining.</returns>
    public static async Task<IHostApplicationBuilder> AddApplicationAsync<TApplication>(
        this IHostApplicationBuilder builder,
        CancellationToken cancellationToken = default
        )
        where TApplication : AielApplicationConfigurator, new()
    {
        ArgumentNullException.ThrowIfNull(builder);

        var environment = await builder.RegisterAielEnvironmentAsync<TApplication>();

        var context = new ConfigurationContext(
            environment,
            builder.Services,
            builder.Configuration);

        var root = context.BuildDependencyTree<TApplication>();

        builder.Services.AddSingleton(root);

        await root.ConfigureDependenciesAsync(context, cancellationToken);

        return builder;
    }

    /// <summary>
    /// Configures the application using a precomputed dependency manager. This overload is intended for
    /// applications that use the source-generated dependency graph (recommended).
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="dependencyDescriptors">The collection of dependency descriptors.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining.</returns>
    public static async Task<IHostApplicationBuilder> RegisterDependenciesAsync(
        this IHostApplicationBuilder builder,
        IEnumerable<DependencyDescriptor> dependencyDescriptors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var environment = builder.Services.GetInstance<IAielEnvironment>()
            ?? throw new AielException("Failed to retrieve the Aiel environment. Did you remember to call `builder.AddApplicationAsync()`?");

        await DependencyManager.ConfigureDependenciesAsync(environment, dependencyDescriptors, builder.Services, builder.Configuration, cancellationToken);

        return builder;
    }

    public static async Task<IAielEnvironment> RegisterAielEnvironmentAsync<TApplication>(this IHostApplicationBuilder builder)
        where TApplication : AielApplicationConfigurator, new()
    {
        var app = new TApplication();
        var environment = new AielHostEnvironment()
        {
            ApplicationInstance = Guid.NewGuid(),
            ApplicationName = app.ApplicationName,
            ApplicationVersion = app.ApplicationVersion,
            ContentRootFileProvider = builder.Environment.ContentRootFileProvider,
            EnvironmentName = builder.Environment.EnvironmentName,
            ContentRootPath = builder.Environment.ContentRootPath
        };

        builder.Services.AddSingleton<IAielEnvironment>(environment);
        await app.SafelyDisposeAsync();
        return environment;
    }
}
