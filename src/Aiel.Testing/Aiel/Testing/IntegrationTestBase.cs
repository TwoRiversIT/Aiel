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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Aiel.Testing;

/// <summary>
/// Provides a base class for integration tests that use a test fixture with dependency injection.
/// </summary>
/// <typeparam name="TFixture">The type of the test fixture providing services and configuration.</typeparam>
/// <remarks>
/// <para>
/// This class is created once per test in the derived test class, and receives
/// the same fixture instance for all tests in the derived class.
/// </para>
/// <para>
/// The fixture provides shared setup, resources, or state that can be reused across multiple tests.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="SystemUnderTestBase{TSut, TFixture}"/> class.
/// </remarks>
/// <param name="fixture">The test fixture providing services and configuration.</param>
/// <param name="output">The test output helper for logging test output.</param>
public abstract class IntegrationTestBase<TFixture>(TFixture fixture, ITestOutputHelper output)
    : TestBase(output), IClassFixture<TFixture>
    where TFixture : IntegrationTestFixture
{
    private readonly TFixture _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    private IServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;
    private FakeTimeProvider? _timeProvider;

    internal override async ValueTask BeginInstanceTestsAsync(CancellationToken cancellationToken = default)
    {
        _serviceProvider = _fixture.GetTestServiceProvider();
    }

    /// <summary>
    /// Gets the dependency injection service provider from the test fixture.
    /// </summary>
    protected IServiceProvider Services => _serviceProvider
        ?? throw new InvalidOperationException(TestFixtureBase.IncorrectFixtureSetup);

    /// <summary>
    /// Gets the configuration from the test fixture.
    /// </summary>
    protected IConfiguration Configuration => _configuration
        ??= Services.GetRequiredService<IConfiguration>();

    protected FakeTimeProvider FakeTime => _timeProvider
        ??= Services.GetRequiredService<FakeTimeProvider>();

    /// <summary>
    /// Do not override this method! You have been warned!
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
        => await Services.SafelyDisposeAsync();
}
