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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aiel.Dependencies;

public sealed class AielDependencyManagerTests
{
    [Fact]
    public void Constructor_Throws_When_Duplicate_Dependency_Types()
    {
        var descriptor = new DependencyDescriptor(
            name: "Test.Dependency",
            dependencyType: typeof(DependencyA),
            dependencies: [],
            configurators: [],
            initializers: []);

        var descriptors = new[] { descriptor, descriptor };

        Assert.Throws<InvalidOperationException>(() => new DependencyManager(descriptors));
    }

    [Fact]
    public void Constructor_Throws_When_Unknown_Dependency()
    {
        var descriptor = new DependencyDescriptor(
            name: "Test.Dependency",
            dependencyType: typeof(DependencyA),
            dependencies: [typeof(DependencyB)],
            configurators: [],
            initializers: []);

        Assert.Throws<InvalidOperationException>(() => new DependencyManager([descriptor]));
    }

    [Fact]
    public void Constructor_Throws_When_Circular_Dependency()
    {
        var a = new DependencyDescriptor(
            name: "Dependency.A",
            dependencyType: typeof(DependencyA),
            dependencies: [typeof(DependencyB)],
            configurators: [],
            initializers: []);

        var b = new DependencyDescriptor(
            name: "Dependency.B",
            dependencyType: typeof(DependencyB),
            dependencies: [typeof(DependencyA)],
            configurators: [],
            initializers: []);

        Assert.Throws<CircularDependencyException>(() => new DependencyManager([a, b]));
    }

    [Fact]
    public async Task ConfigureAsync_Invokes_Each_Configurator_Once_In_Diamond_Graph()
    {
        DependencyAConfigurator.Reset();
        DependencyBConfigurator.Reset();
        DependencyCConfigurator.Reset();
        DependencyDConfigurator.Reset();

        var d = new DependencyDescriptor(
            name: "Dependency.D",
            dependencyType: typeof(DependencyD),
            dependencies: [],
            configurators: [typeof(DependencyDConfigurator)],
            initializers: []);

        var b = new DependencyDescriptor(
            name: "Dependency.B",
            dependencyType: typeof(DependencyB),
            dependencies: [typeof(DependencyD)],
            configurators: [typeof(DependencyBConfigurator)],
            initializers: []);

        var c = new DependencyDescriptor(
            name: "Dependency.C",
            dependencyType: typeof(DependencyC),
            dependencies: [typeof(DependencyD)],
            configurators: [typeof(DependencyCConfigurator)],
            initializers: []);

        var a = new DependencyDescriptor(
            name: "Dependency.A",
            dependencyType: typeof(DependencyA),
            dependencies: [typeof(DependencyB), typeof(DependencyC)],
            configurators: [typeof(DependencyAConfigurator)],
            initializers: []);

        var manager = new DependencyManager([a, b, c, d]);

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var context = new DependencyConfigurationContext(new AielEnvironment("TestApp", "1.0.0", "Development", Guid.NewGuid()), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        Assert.Equal(1, DependencyDConfigurator.InvokeCount);
        Assert.Equal(1, DependencyBConfigurator.InvokeCount);
        Assert.Equal(1, DependencyCConfigurator.InvokeCount);
        Assert.Equal(1, DependencyAConfigurator.InvokeCount);
    }

    [Fact]
    public async Task InitializeAsync_Invokes_Each_Initializer_Once_In_Linear_Graph()
    {
        DependencyAInitializer.Reset();
        DependencyBInitializer.Reset();

        var b = new DependencyDescriptor(
            name: "Dependency.B",
            dependencyType: typeof(DependencyB),
            dependencies: [],
            configurators: [],
            initializers: [typeof(DependencyBInitializer)]);

        var a = new DependencyDescriptor(
            name: "Dependency.A",
            dependencyType: typeof(DependencyA),
            dependencies: [typeof(DependencyB)],
            configurators: [],
            initializers: [typeof(DependencyAInitializer)]);

        var manager = new DependencyManager([a, b]);

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var env = new TestHostEnvironment();
        var logger = serviceProvider.GetRequiredService<ILogger<DependencyInitializationContext>>();
        var context = new DependencyInitializationContext(new AielEnvironment(env.ApplicationName, "1.0.0", env.EnvironmentName, Guid.NewGuid()), configuration, logger, serviceProvider);

        await manager.InitializeAsync(context, CancellationToken.None);

        Assert.Equal(1, DependencyBInitializer.InvokeCount);
        Assert.Equal(1, DependencyAInitializer.InvokeCount);
    }

    [Fact]
    public async Task PreConfigureAsync_Is_Invoked_Once_Per_Configurator_In_Diamond_Graph()
    {
        DependencyAPreConfigurator.Reset();
        DependencyBPreConfigurator.Reset();
        DependencyCPreConfigurator.Reset();
        DependencyDPreConfigurator.Reset();

        var d = new DependencyDescriptor(
            name: "Dependency.D",
            dependencyType: typeof(DependencyD),
            dependencies: [],
            configurators: [typeof(DependencyDPreConfigurator)],
            initializers: []);

        var b = new DependencyDescriptor(
            name: "Dependency.B",
            dependencyType: typeof(DependencyB),
            dependencies: [typeof(DependencyD)],
            configurators: [typeof(DependencyBPreConfigurator)],
            initializers: []);

        var c = new DependencyDescriptor(
            name: "Dependency.C",
            dependencyType: typeof(DependencyC),
            dependencies: [typeof(DependencyD)],
            configurators: [typeof(DependencyCPreConfigurator)],
            initializers: []);

        var a = new DependencyDescriptor(
            name: "Dependency.A",
            dependencyType: typeof(DependencyA),
            dependencies: [typeof(DependencyB), typeof(DependencyC)],
            configurators: [typeof(DependencyAPreConfigurator)],
            initializers: []);

        var manager = new DependencyManager([a, b, c, d]);
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var context = new DependencyConfigurationContext(new AielEnvironment("TestApp", "1.0.0", "Development", Guid.NewGuid()), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        Assert.Equal(1, DependencyDPreConfigurator.PreConfigureCount);
        Assert.Equal(1, DependencyBPreConfigurator.PreConfigureCount);
        Assert.Equal(1, DependencyCPreConfigurator.PreConfigureCount);
        Assert.Equal(1, DependencyAPreConfigurator.PreConfigureCount);
    }

    [Fact]
    public async Task ConfigureAsync_Runs_All_PreConfigureAsync_Before_Any_ConfigureAsync_In_Linear_Graph()
    {
        PhaseLog.Clear();

        // A depends on B
        var b = new DependencyDescriptor(
            name: "Dependency.B",
            dependencyType: typeof(DependencyB),
            dependencies: [],
            configurators: [typeof(DependencyBPhaseConfigurator)],
            initializers: []);

        var a = new DependencyDescriptor(
            name: "Dependency.A",
            dependencyType: typeof(DependencyA),
            dependencies: [typeof(DependencyB)],
            configurators: [typeof(DependencyAPhaseConfigurator)],
            initializers: []);

        var manager = new DependencyManager([a, b]);
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var context = new DependencyConfigurationContext(new AielEnvironment("TestApp", "1.0.0", "Development", Guid.NewGuid()), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        Assert.Equal(4, PhaseLog.Count);

        var lastPreIndex = PhaseLog
            .Select(static (entry, i) => (entry, i))
            .Where(static x => x.entry.EndsWith(":Pre"))
            .Max(static x => x.i);

        var firstConfigureIndex = PhaseLog
            .Select(static (entry, i) => (entry, i))
            .Where(static x => x.entry.EndsWith(":Configure"))
            .Min(static x => x.i);

        Assert.True(
            lastPreIndex < firstConfigureIndex,
            $"All PreConfigureAsync calls must complete before any ConfigureAsync begins. Actual order: [{String.Join(", ", PhaseLog)}]");
    }

    private static readonly List<String> PhaseLog = [];

    private sealed class DependencyA;
    private sealed class DependencyB;
    private sealed class DependencyC;
    private sealed class DependencyD;

    private sealed class DependencyAConfigurator : IDependencyConfigurator
    {
        public static Int32 InvokeCount { get; private set; }

        public static void Reset() => InvokeCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class DependencyBConfigurator : IDependencyConfigurator
    {
        public static Int32 InvokeCount { get; private set; }

        public static void Reset() => InvokeCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class DependencyCConfigurator : IDependencyConfigurator
    {
        public static Int32 InvokeCount { get; private set; }

        public static void Reset() => InvokeCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class DependencyDConfigurator : IDependencyConfigurator
    {
        public static Int32 InvokeCount { get; private set; }

        public static void Reset() => InvokeCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class DependencyAInitializer : IDependencyInitializer
    {
        public static Int32 InvokeCount { get; private set; }

        public static void Reset() => InvokeCount = 0;

        public Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken)
        {
            InvokeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class DependencyBInitializer : IDependencyInitializer
    {
        public static Int32 InvokeCount { get; private set; }

        public static void Reset() => InvokeCount = 0;

        public Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken)
        {
            InvokeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public String ApplicationName { get; set; } = "TestApp";
        public String EnvironmentName { get; set; } = "Development";
        public String ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = default!; // Use a Mock or a NullFileProvider if needed
    }

    private sealed class DependencyAPreConfigurator : IDependencyConfigurator
    {
        public static Int32 PreConfigureCount { get; private set; }

        public static void Reset() => PreConfigureCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigureCount++;
            return Task.CompletedTask;
        }

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DependencyBPreConfigurator : IDependencyConfigurator
    {
        public static Int32 PreConfigureCount { get; private set; }

        public static void Reset() => PreConfigureCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigureCount++;
            return Task.CompletedTask;
        }

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DependencyCPreConfigurator : IDependencyConfigurator
    {
        public static Int32 PreConfigureCount { get; private set; }

        public static void Reset() => PreConfigureCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigureCount++;
            return Task.CompletedTask;
        }

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DependencyDPreConfigurator : IDependencyConfigurator
    {
        public static Int32 PreConfigureCount { get; private set; }

        public static void Reset() => PreConfigureCount = 0;

        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigureCount++;
            return Task.CompletedTask;
        }

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DependencyAPhaseConfigurator : IDependencyConfigurator
    {
        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PhaseLog.Add("A:Pre");
            return Task.CompletedTask;
        }

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PhaseLog.Add("A:Configure");
            return Task.CompletedTask;
        }
    }

    private sealed class DependencyBPhaseConfigurator : IDependencyConfigurator
    {
        public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PhaseLog.Add("B:Pre");
            return Task.CompletedTask;
        }

        public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PhaseLog.Add("B:Configure");
            return Task.CompletedTask;
        }
    }
}
