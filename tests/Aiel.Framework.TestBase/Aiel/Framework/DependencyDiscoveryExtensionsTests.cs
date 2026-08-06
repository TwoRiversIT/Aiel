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

namespace Aiel.Framework;

public abstract class DependencyDiscoveryExtensionsTests
{
    public abstract Task InitializeApplicationAsync(IServiceProvider serviceProvider);

    [Fact]
    public async Task ConfigureDependenciesAsync_InvokesSharedDependencyOnce_InDiamondGraph()
    {
        DiamondSharedDependency.Reset();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new AielEnvironment("TestApp", "1.0.0", "Development", Guid.NewGuid());
        var context = new ConfigurationContext(environment, services, configuration);

        var root = context.BuildDependencyTree<DiamondRootDependency>();

        await root.ConfigureDependenciesAsync(context, TestContext.Current.CancellationToken);

        DiamondSharedDependency.PreConfigureCount.Should().Be(1);
        DiamondSharedDependency.ConfigureCount.Should().Be(1);
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

        var context = new ConfigurationContext(environment, services, configuration);
        var root = context.BuildDependencyTree<InitializerRootDependency>();
        services.AddSingleton(root);

        var serviceProvider = services.BuildServiceProvider();

        await InitializeApplicationAsync(serviceProvider);

        InitializerSharedDependency.InitializeCount.Should().Be(1);
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

        public override Task PreConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigureCount++;
            return Task.CompletedTask;
        }

        public override Task ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
        {
            ConfigureCount++;
            return Task.CompletedTask;
        }
    }

    [DependsOn(typeof(InitializerLeftDependency))]
    [DependsOn(typeof(InitializerRightDependency))]
    private sealed class InitializerRootDependency : AielDependencyConfigurator;

    [DependsOn(typeof(InitializerSharedDependency))]
    private sealed class InitializerLeftDependency : AielDependencyConfigurator;

    [DependsOn(typeof(InitializerSharedDependency))]
    private sealed class InitializerRightDependency : AielDependencyConfigurator;

    private sealed class InitializerSharedDependency : AielDependencyConfigurator, IInitializer
    {
        public static Int32 InitializeCount { get; private set; }

        public static void Reset() => InitializeCount = 0;

        public Task InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
        {
            InitializeCount++;
            return Task.CompletedTask;
        }
    }
}
