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

using Aiel.Framework.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Framework;

public class AielFrameworkRegistrationTests
{
    [Fact]
    public async Task ConfigureAsync_RegistersDefaultDependencyManager_WhenMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var context = CreateContext(services);
        var sut = new AielFramework();

        // Act
        await sut.ConfigureAsync(context);

        // Assert
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IDependencyManager)
            && descriptor.ImplementationType == typeof(DefaultDependencyManager));
    }

    [Fact]
    public async Task ConfigureAsync_DoesNotOverrideExistingDependencyManager()
    {
        // Arrange
        var services = new ServiceCollection();
        var existing = new StubDependencyManager(CreateContext(new ServiceCollection()));
        services.AddSingleton<IDependencyManager>(existing);

        var context = CreateContext(services);
        var sut = new AielFramework();

        // Act
        await sut.ConfigureAsync(context);

        // Assert
        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(IDependencyManager));

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IDependencyManager>().Should().BeSameAs(existing);
    }

    private static ConfigurationContext CreateContext(IServiceCollection services)
    {
        var environment = new AielEnvironment
        {
            ApplicationInstance = Guid.NewGuid(),
            ApplicationName = nameof(AielFrameworkRegistrationTests),
            ApplicationVersion = "1.0.0",
            EnvironmentName = "Testing"
        };

        return new ConfigurationContext(environment, new ConfigurationBuilder().Build(), services);
    }

    private sealed class StubDependencyManager(ConfigurationContext context) : DependencyManager(context)
    {
        protected override Task InitializeAsync(InitializationContext context, DependencyNode descriptor, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
