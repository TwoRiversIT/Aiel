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

using Aiel.Fakes;
using Aiel.Framework;
using Aiel.Framework.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Aiel.Testing;

/// <summary>
/// Provides integration testing infrastructure with full dependency injection
/// support and core services like <seealso cref="IConfiguration"/>,
/// <see cref="IServiceProvider"/>, <seealso cref="TimeProvider"/>, etc.
/// </summary>
public abstract class TestFixtureBase : DisposableBase, IAsyncLifetime, IConfigurator, IInitializer
{
    internal const String IncorrectFixtureSetup = "Incorrect fixture setup. Ensure your fixture is overriding InitializeAsync(InitializationContext, CancellationToken) and not InitializeAsync().";

    private IHost? _host;

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// Do not override this method! You have been warned!
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is automatically called by the test framework to initialize the test fixture.
    /// If you think you need to override this method, you are wrong.
    /// Override <see cref="InitializeAsync(InitializationContext, CancellationToken)"/> instead.
    /// </para>
    /// <para>
    /// If the fixture is supplied via dependency injection, then this method will be called
    /// before <see cref="IntegrationTestBase{TFixture}.InitializeAsync"/>
    /// </para>
    /// </remarks>
    public async ValueTask InitializeAsync()
    {
        var builder = CreateBuilder();

        var configContext = new ConfigurationContext(FakeAielEnvironment.Create(), builder.Configuration, builder.Services);

        await ConfigureFixtureAsync(configContext, CancellationToken);

        await PreConfigureAsync(configContext, CancellationToken);

        await ConfigureAsync(configContext, CancellationToken);

        _host = builder.Build();

        using (var scope = _host.Services.CreateScope())
        {
            var initContext = new TestInitializationContext(scope.ServiceProvider);

            await InitializeAsync(initContext, CancellationToken);
        }

        var context = new TestInitializationContext(_host.Services);
        await InitializeFixtureAsync(context, CancellationToken);
    }

    // This is called once per test run, after all fixtures have been initialized, but before any tests are executed.
    internal IServiceProvider GetTestServiceProvider()
    {
        if (_host is null)
        {
            throw new InvalidOperationException(IncorrectFixtureSetup);
        }

        return new ServiceProviderWrapper(_host.Services.CreateScope());
    }

    internal abstract ValueTask ConfigureFixtureAsync(ConfigurationContext context, CancellationToken cancellationToken);

    internal abstract ValueTask InitializeFixtureAsync(InitializationContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the base path used when loading integration-test configuration files.
    /// </summary>
    /// <remarks>
    /// Override this method when a fixture needs to load appsettings.json from a
    /// directory other than the current working directory.
    /// </remarks>
    protected virtual String GetConfigurationBasePath() => Directory.GetCurrentDirectory();

    // <inheritdoc />
    public virtual ValueTask PreConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    // <inheritdoc />
    public virtual ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    // <inheritdoc />
    public virtual ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    protected virtual HostApplicationBuilder CreateBuilder()
    {
        var settings = new HostApplicationBuilderSettings()
        {
            EnvironmentName = "Testing"
        };

        var builder = Host.CreateEmptyApplicationBuilder(settings);

        // appsettings.Testing.json is optional so local overrides never need to be committed for the fixture to load.
        builder.Configuration
            .SetBasePath(GetConfigurationBasePath())
            .AddJsonFile("appsettings.Testing.json", optional: true);

        builder.Services.AddScoped(_ => new FakeTimeProvider());
        builder.Services.AddScoped<TimeProvider>(sp => sp.GetRequiredService<FakeTimeProvider>());
        builder.Services.AddScoped<IAielEnvironment>(_ => FakeAielEnvironment.Create());

        return builder;
    }

    /// <summary>
    /// Asynchronously disposes resources used by the fixture.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        _host?.Dispose();
        _host = null;

        await base.DisposeAsyncCore();
    }

    private sealed class ServiceProviderWrapper(IServiceScope serviceScope) : IServiceProvider, IDisposable
    {
        private readonly IServiceScope _serviceScope = serviceScope;

        public Object? GetService(Type serviceType) => _serviceScope.ServiceProvider.GetService(serviceType);

        public void Dispose()
        {
            _serviceScope.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
