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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static AwesomeAssertions.FluentActions;

namespace Aiel.Framework;

public sealed class AielDependencyManagerTests : PhaseLogCollector
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

        Invoking(() => new DependencyManager(descriptors))
            .Should().Throw<InvalidOperationException>();
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

        Invoking(() => new DependencyManager([descriptor]))
            .Should().Throw<InvalidOperationException>();
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

        Invoking(() => new DependencyManager([a, b]))
            .Should().ThrowExactly<CircularDependencyException>();
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
        var context = new ConfigurationContext(FakeAielEnvironment.Create(), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        DependencyDConfigurator.InvokeCount.Should().Be(1);
        DependencyBConfigurator.InvokeCount.Should().Be(1);
        DependencyCConfigurator.InvokeCount.Should().Be(1);
        DependencyAConfigurator.InvokeCount.Should().Be(1);
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
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<IAielEnvironment>(FakeAielEnvironment.Create());

        var serviceProvider = services.BuildServiceProvider();
        var context = new InitializationContext(serviceProvider);

        await manager.InitializeAsync(context, CancellationToken.None);

        DependencyBInitializer.InvokeCount.Should().Be(1);
        DependencyAInitializer.InvokeCount.Should().Be(1);
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
        var context = new ConfigurationContext(FakeAielEnvironment.Create(), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        DependencyDPreConfigurator.PreConfigureCount.Should().Be(1);
        DependencyBPreConfigurator.PreConfigureCount.Should().Be(1);
        DependencyCPreConfigurator.PreConfigureCount.Should().Be(1);
        DependencyAPreConfigurator.PreConfigureCount.Should().Be(1);
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
        var context = new ConfigurationContext(FakeAielEnvironment.Create(), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        PhaseLog.Should().HaveCount(4);

        var lastPreIndex = PhaseLog
            .Select(static (entry, i) => (entry, i))
            .Where(static x => x.entry.EndsWith(":Pre"))
            .Max(static x => x.i);

        var firstConfigureIndex = PhaseLog
            .Select(static (entry, i) => (entry, i))
            .Where(static x => x.entry.EndsWith(":Configure"))
            .Min(static x => x.i);

        (lastPreIndex < firstConfigureIndex).Should().BeTrue(
            $"All PreConfigureAsync calls must complete before any ConfigureAsync begins. Actual order: [{String.Join(", ", PhaseLog)}]");
    }
}
