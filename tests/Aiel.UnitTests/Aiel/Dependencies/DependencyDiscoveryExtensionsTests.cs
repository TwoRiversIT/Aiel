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
using Microsoft.Extensions.Hosting;

namespace Aiel.Dependencies;

public sealed class DependencyDiscoveryExtensionsTests
{
    [Fact]
    public async Task ConfigureDependenciesAsync_InvokesSharedDependencyOnce_InDiamondGraph()
    {
        DiamondSharedDependency.Reset();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new AielEnvironment("TestApp", "1.0.0", "Development", Guid.NewGuid());
        var context = new DependencyConfigurationContext(environment, services, configuration);

        var root = context.BuildDependencyTree<DiamondRootDependency>();

        await root.ConfigureDependenciesAsync(context, TestContext.Current.CancellationToken);

        Assert.Equal(1, DiamondSharedDependency.PreConfigureCount);
        Assert.Equal(1, DiamondSharedDependency.ConfigureCount);
    }

    [Fact]
    public async Task InitializeApplicationAsync_InvokesSharedInitializerOnce_InDiamondGraph()
    {
        InitializerSharedDependency.Reset();

        var services = new ServiceCollection();
        services.AddLogging();

        var environment = new AielEnvironment("TestApp", "1.0.0", "Development", Guid.NewGuid());
        var configuration = new ConfigurationBuilder().Build();

        services.AddSingleton(environment);
        services.AddSingleton<IConfiguration>(configuration);

        var context = new DependencyConfigurationContext(environment, services, configuration);
        var root = context.BuildDependencyTree<InitializerRootDependency>();
        services.AddSingleton(root);

        var serviceProvider = services.BuildServiceProvider();
        var host = new TestHost(serviceProvider);

        await host.InitializeApplicationAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, InitializerSharedDependency.InitializeCount);
    }

    [DependsOn(typeof(DiamondLeftDependency))]
    [DependsOn(typeof(DiamondRightDependency))]
    private sealed class DiamondRootDependency : AielDependencyConfigurator;

    [DependsOn(typeof(DiamondSharedDependency))]
    private sealed class DiamondLeftDependency : AielDependencyConfigurator;

    [DependsOn(typeof(DiamondSharedDependency))]
    private sealed class DiamondRightDependency : AielDependencyConfigurator;

    private sealed class DiamondSharedDependency : AielDependencyConfigurator
    {
        public static Int32 PreConfigureCount { get; private set; }

        public static Int32 ConfigureCount { get; private set; }

        public static void Reset()
        {
            PreConfigureCount = 0;
            ConfigureCount = 0;
        }

        public override ValueTask PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigureCount++;
            return ValueTask.CompletedTask;
        }

        public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            ConfigureCount++;
            return ValueTask.CompletedTask;
        }
    }

    [DependsOn(typeof(InitializerLeftDependency))]
    [DependsOn(typeof(InitializerRightDependency))]
    private sealed class InitializerRootDependency : AielDependencyConfigurator;

    [DependsOn(typeof(InitializerSharedDependency))]
    private sealed class InitializerLeftDependency : AielDependencyConfigurator;

    [DependsOn(typeof(InitializerSharedDependency))]
    private sealed class InitializerRightDependency : AielDependencyConfigurator;

    private sealed class InitializerSharedDependency : AielDependencyConfigurator, IDependencyInitializer
    {
        public static Int32 InitializeCount { get; private set; }

        public static void Reset() => InitializeCount = 0;

        public Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken)
        {
            InitializeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestHost(IServiceProvider services) : IHost
    {
        public IServiceProvider Services { get; } = services;

        public void Dispose()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
