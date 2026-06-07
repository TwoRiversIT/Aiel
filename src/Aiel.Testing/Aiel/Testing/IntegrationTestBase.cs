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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Aiel.Testing;

/// <summary>
/// Provides a base class for integration tests that use a test fixture with dependency injection.
/// </summary>
/// <typeparam name="TFixture">The type of the test fixture providing services and configuration.</typeparam>
public abstract class IntegrationTestBase<TFixture>
    : DisposableBase, IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : IntegrationTestFixture
{
    /// <summary>
    /// Gets the shared test fixture instance for the current test context.
    /// </summary>
    /// <remarks>The fixture provides shared setup, resources, or state that can be reused across multiple
    /// tests. Use this property to access common dependencies or configuration required by the test class.</remarks>
    protected TFixture Fixture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase{TSut, TFixture}"/> class.
    /// </summary>
    /// <param name="fixture">The test fixture providing services and configuration.</param>
    /// <param name="output">The test output helper for logging test output.</param>
    protected IntegrationTestBase(TFixture fixture, ITestOutputHelper output)
    {
        ArgumentNullException.ThrowIfNull(output);
        Fixture = fixture;
        Fixture.TestOutputHelper = output;
    }

    /// <summary>
    /// Asynchronously initializes the test fixture, preparing it for use in test execution.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async ValueTask InitializeAsync()
    {
        if (Fixture is IAsyncTestFixture asyncFixture)
        {
            await asyncFixture.BeginTestAsync();
        }
    }

    /// <summary>
    /// Gets the configuration from the test fixture.
    /// </summary>
    protected IConfiguration Configuration => Fixture.Configuration;

    /// <summary>
    /// Gets the dependency injection service provider from the test fixture.
    /// </summary>
    protected IServiceProvider Services => Fixture.Services;

    /// <summary>
    /// Gets the cancellation token from the current test context.
    /// </summary>
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected FakeTimeProvider TimeProvider => Fixture.TimeProvider;

    protected ITestOutputHelper TestOutput => Fixture.TestOutputHelper;

    /// <summary>
    /// Releases managed resources used by the test.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        if (Fixture is IAsyncTestFixture asyncFixture)
        {
            await asyncFixture.EndTestAsync();
        }
    }
}

/// <summary>
/// Provides a base class for integration tests that test a specific service, Service Under Test (SUT), and a test fixture with
/// configured service dependencies.
/// </summary>
/// <remarks>This class supports integration testing scenarios where the SUT is resolved from the test fixture's
/// service provider. The SUT instance is created lazily and is available to derived test classes via the protected SUT
/// property.</remarks>
/// <typeparam name="TSut">The type of the System Under Test (SUT) to be resolved from the service provider. Must not be null.</typeparam>
/// <typeparam name="TFixture">The type of the integration test fixture that supplies services and configuration for the test environment.</typeparam>
public abstract class IntegrationTestBase<TSut, TFixture>
    : IntegrationTestBase<TFixture>
    where TSut : notnull
    where TFixture : IntegrationTestFixture
{
    private readonly Lazy<TSut> _lazySut;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase{TSut, TFixture}"/> class.
    /// </summary>
    /// <param name="fixture">The test fixture providing services and configuration.</param>
    /// <param name="output">The test output helper for logging test output.</param>
    protected IntegrationTestBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
        _lazySut = new Lazy<TSut>(Fixture.Services.GetRequiredService<TSut>);
    }

    /// <summary>
    /// Gets the System Under Test (SUT) instance from the service provider.
    /// </summary>
    /// <remarks>
    /// The SUT is lazily instantiated on first access.
    /// </remarks>
    protected TSut SUT => _lazySut.Value;
}
