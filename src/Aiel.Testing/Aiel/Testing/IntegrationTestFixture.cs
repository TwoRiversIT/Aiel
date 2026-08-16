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
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Aiel.Testing;

/// <summary>
/// Provides a concrete implementation of a test services that sets up a full .NET services with dependency injection for integration tests.
/// </summary>
public class IntegrationTestFixture : DisposableBase, IAsyncTestFixture, IAsyncLifetime, IConfigurator, IInitializer
{
    private IHost? _host;

    private IServiceScope? _testScope;
    private Int32 _initializationCount;
    private Int32 _beginCount;
    private Int32 _disposalCount;
    private Int32 _endCount;

    /// <summary>
    /// Gets the configuration for the test fixture.
    /// </summary>
    public IConfiguration Configuration { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the test output helper used to capture and display test output.
    /// </summary>
    /// <remarks>Use this property to write diagnostic messages or additional information during test
    /// execution. The value may be null if no output helper is available.</remarks>
    public ITestOutputHelper TestOutputHelper { get; set; } = default!;

    /// <summary>
    /// Gets the dependency injection service provider for the current test scope.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the fixture has not been initialized.</exception>
    public IServiceProvider Services => _testScope?.ServiceProvider
        ?? throw new InvalidOperationException("The test scope has not been created. Did you override `InitializeAsync()` without calling `await base.InitializeAsync()`?");

    /// <summary>
    /// Gets the fake time provider used to control and manipulate time during tests.
    /// </summary>
    public FakeTimeProvider TimeProvider { get; } = new FakeTimeProvider();

    /// <summary>
    /// Do not override this method! You have been warned!
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is automatically called by the test framework to initialize the test fixture.
    /// If you think you need to override this method, you are wrong.
    /// Override <see cref="InitializeAsync(InitializationContext)"/> instead.
    /// </para>
    /// <para>
    /// If the fixture is supplied via dependency injection, then this method will be called
    /// before <see cref="IntegrationTestBase{TFixture}.InitializeAsync"/>
    /// </para>
    /// </remarks>
    public async ValueTask InitializeAsync()
    {
        _initializationCount++;

        var builder = CreateBuilder();

        var configContext = new ConfigurationContext(FakeAielEnvironment.Create(), builder.Services, builder.Configuration);

        await PreConfigureAsync(configContext, TestContext.Current.CancellationToken);

        await ConfigureAsync(configContext, TestContext.Current.CancellationToken);

        _host = builder.Build();

        Configuration = _host.Services.GetRequiredService<IConfiguration>();

        using (var scope = _host.Services.CreateScope())
        {
            var initContext = new InitializationContext(scope.ServiceProvider);

            await InitializeAsync(initContext, TestContext.Current.CancellationToken);
        }
    }

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

        builder.Services.AddSingleton<ILoggerProvider>(_ => new XUnitLoggerProvider(TestOutputHelper, new XUnitLoggerOptions()
        {
            IncludeLogLevel = true,
            IncludeScopes = true
        }));

        builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider);

        builder.Services.AddSingleton<IAielEnvironment>(FakeAielEnvironment.Create());

        return builder;
    }

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

    /// <summary>
    /// Called before each test to ensure the test scope has been created.
    /// </summary>
    ValueTask IAsyncTestFixture.BeginTestAsync()
    {
        if (_initializationCount != 1 || _host is null)
        {
            throw new InvalidOperationException("Fixture has not been initialized. Ensure your fixture is overriding InitializeAsync(InitializationContext) and not InitializeAsync().");
        }

        _beginCount++;
        _testScope = _host.Services.CreateScope();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called after each test to dispose of the test scope.
    /// </summary>
    ValueTask IAsyncTestFixture.EndTestAsync()
    {
        _endCount++;
        _testScope?.Dispose();
        _testScope = null;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Asynchronously disposes resources used by the fixture.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    protected override ValueTask DisposeAsyncCore()
    {
        _disposalCount++;
        _host?.Dispose();

        Console.WriteLine($"IntegrationTestFixture disposed. InitializationCount={_initializationCount}, BeginCount={_beginCount}, EndCount={_endCount}, DisposalCount={_disposalCount}");

        return base.DisposeAsyncCore();
    }
}
