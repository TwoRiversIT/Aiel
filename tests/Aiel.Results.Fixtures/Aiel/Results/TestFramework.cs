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

using Aiel.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Aiel.Results;

/// <summary>
/// The tests in this assembly are testing the compiled Aiel.Results assembly.
/// They are not testing the project source code. If you are seeing unexpected results,
/// clean the solution, rebuild, and run the tests again.
/// </summary>
public class ResultsIntegrationTestFixture : IntegrationTestFixture
{
    protected override void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        // This override suppresses the requirement for the appsettings.Testing.json file.
        // These tests do not require any configuration settings at this time.
    }

    /// <summary>
    /// Registers the Results infrastructure. This is necessary to ensure that the
    /// JsonConverters and other infrastructure are properly registered for the tests. If you
    /// comment it out you should see a warning about using Results without registering
    /// services, and many tests SHOULD fail due to missing converters.
    /// </summary>
    /// <param name="services">the service collection to which Results services will be added</param>
    /// <param name="configuration">the configuration instance for the test fixture</param>
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddResultPattern();
    }

    protected override ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<ResultsIntegrationTestFixture>>();
        logger.LogInformation("""
            Initializing Results integration test fixture. The module initializers in the
            Aiel.Results.UnitTests.CustomErrors assembly should have run by now.
            """);

        return ValueTask.CompletedTask;
    }
}

public class ResultsUnitTestBase(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase<ResultsIntegrationTestFixture>(fixture, output)
{
}
