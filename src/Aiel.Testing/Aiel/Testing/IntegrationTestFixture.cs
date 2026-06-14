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
public class IntegrationTestFixture : DisposableBase, IAsyncTestFixture, IAsyncLifetime
{
    private IHost? _host;

    private IServiceScope? _testScope;
    private Int32 _initializationCount;
    private Int32 _beginCount;
    private Int32 _disposalCount;
    private Int32 _endCount;

    /// <summary>
    /// Gets or sets the test output helper used to capture and display test output.
    /// </summary>
    /// <remarks>Use this property to write diagnostic messages or additional information during test
    /// execution. The value may be null if no output helper is available.</remarks>
    public ITestOutputHelper TestOutputHelper { get; set; } = default!;

    /// <summary>
    /// Gets the configuration for the test fixture.
    /// </summary>
    public IConfiguration Configuration { get; private set; } = default!;

    public FakeTimeProvider TimeProvider { get; } = new FakeTimeProvider();

    /// <summary>
    /// Gets the dependency injection service provider for the current test scope.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the fixture has not been initialized.</exception>
    public IServiceProvider Services => _testScope?.ServiceProvider
        ?? throw new InvalidOperationException("Test scope has not been started. Call BeginTestAsync() before accessing Services.");

    /// <summary>
    /// Configures the fixture before tests run. Implementers should override <see cref="InitializeFixtureAsync(IServiceProvider)"/> to customize initialization.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _initializationCount++;

        var settings = new HostApplicationBuilderSettings()
        {
            EnvironmentName = "Testing"
        };

        await ConfigureSettingsAsync(settings, TestContext.Current.CancellationToken);

        var builder = Host.CreateEmptyApplicationBuilder(settings);

        await ConfigureBuilderAsync(builder, TestContext.Current.CancellationToken);

        await ConfigureConfigurationAsync(builder.Configuration, TestContext.Current.CancellationToken);

        await ConfigureLoggingAsync(builder.Logging, TestContext.Current.CancellationToken);

        builder.Services.AddSingleton<ILoggerProvider>(_ => new XUnitLoggerProvider(TestOutputHelper, new XUnitLoggerOptions() { IncludeLogLevel = true, IncludeScopes = true }));

        await ConfigureServicesAsync(builder.Services, builder.Configuration, TestContext.Current.CancellationToken);

        _host = builder.Build();

        Configuration = _host.Services.GetRequiredService<IConfiguration>();

        using var scope = _host.Services.CreateScope();
        await InitializeFixtureAsync(scope.ServiceProvider, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Called to configure services application builder settings before the services is created.
    /// </summary>
    /// <param name="settings">The services application builder settings to configure.</param>
    /// <remarks>
    /// Override this method in derived classes to customize services settings.
    /// </remarks>
    protected virtual void ConfigureSettings(HostApplicationBuilderSettings settings) { }
    protected virtual ValueTask ConfigureSettingsAsync(HostApplicationBuilderSettings settings, CancellationToken cancellationToken = default)
    {
        ConfigureSettings(settings);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Allows derived fixtures to configure the base fixture's application builder.
    /// </summary>
    /// <remarks>This method is called during fixture initialization to allow customization of the host builder.
    /// Derived classes can override this method to register services, modify configuration, or perform other setup
    /// tasks before the application is built.</remarks>
    /// <param name="builder">The host application builder to configure. Provides access to services, configuration, and other application
    /// setup features.</param>
    protected virtual void ConfigureBuilder(IHostApplicationBuilder builder) { }
    protected virtual ValueTask ConfigureBuilderAsync(IHostApplicationBuilder builder, CancellationToken cancellationToken = default)
    {
        ConfigureBuilder(builder);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the base path used when loading integration-test configuration files.
    /// </summary>
    /// <remarks>
    /// Override this method when a fixture needs to load configuration from a directory other than the current working
    /// directory.
    /// </remarks>
    protected virtual String GetConfigurationBasePath()
    {
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Called to configure the configuration builder before services are configured.
    /// </summary>
    /// <param name="builder">The configuration builder to configure.</param>
    /// <remarks>
    /// By default, this loads appsettings.Testing.json from <see cref="GetConfigurationBasePath"/> when the file is
    /// present and otherwise uses the host defaults.
    /// Override this method in derived classes to customize configuration loading.
    /// </remarks>
    protected virtual void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        var basePath = GetConfigurationBasePath();

        builder.SetBasePath(basePath)
            // appsettings.Testing.json is optional so local overrides never need to be committed for the fixture to load.
            .AddJsonFile("appsettings.Testing.json", optional: true);
    }
    protected virtual ValueTask ConfigureConfigurationAsync(IConfigurationBuilder builder, CancellationToken cancellationToken = default)
    {
        ConfigureConfiguration(builder);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Configures the application's logging services.
    /// </summary>
    /// <remarks>Override this method to customize logging configuration for the application. By default, no
    /// additional configuration is applied.</remarks>
    /// <param name="logging">The <see cref="ILoggingBuilder"/> instance used to configure logging providers and settings.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> instance after applying any custom logging configuration.</returns>
    protected virtual void ConfigureLogging(ILoggingBuilder logging) { }
    protected virtual ValueTask ConfigureLoggingAsync(ILoggingBuilder logging, CancellationToken cancellationToken = default)
    {
        ConfigureLogging(logging);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called to configure dependency injection services for the test services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration instance to use.</param>
    /// <remarks>
    /// Override this method in derived classes to register services needed for tests.
    /// </remarks>
    protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }
    protected virtual ValueTask ConfigureServicesAsync(IServiceCollection services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ConfigureServices(services, configuration);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Provides an opportunity to configure the services instance before tests run.
    /// </summary>
    /// <remarks>Override this method in a derived class to apply custom configuration to the services. This
    /// method is called before the tests are started, allowing for additional setup or service registration.</remarks>
    /// <param name="services">The services to configure. Cannot be null.</param>
    protected virtual ValueTask InitializeFixtureAsync(IServiceProvider services, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    /// <summary>
    /// Called before each test to initialize the test scope.
    /// </summary>
    ValueTask IAsyncTestFixture.BeginTestAsync()
    {
        if (_initializationCount != 1 || _host is null)
        {
            throw new InvalidOperationException("Fixture has not been initialized. Ensure your fixture is overriding InitializeFixtureAsync(IServiceProvider) and not InitializeAsync() before beginning tests.");
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
