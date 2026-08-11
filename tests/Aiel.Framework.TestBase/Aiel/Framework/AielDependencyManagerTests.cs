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

public abstract class AielDependencyManagerTests : PhaseLogCollector
{
    public abstract DependencyManager CreateDependencyManager(IEnumerable<DependencyDescriptor> descriptors);
    public abstract InitializationContext CreateInitializationContextAsync();

    [Fact]
    public void Constructor_Throws_When_Duplicate_Dependency_Types()
    {
        var a1 = new DependencyDescriptor(
            name: nameof(DiamondA),
            dependencyType: typeof(DiamondA),
            new DiamondA(),
            dependencies: []);

        var a2 = new DependencyDescriptor(
            name: nameof(DiamondA),
            dependencyType: typeof(DiamondA),
            new DiamondA(),
            dependencies: []);

        var descriptors = new[] { a1, a2 };

        Invoking(() => CreateDependencyManager(descriptors))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_Throws_When_Unknown_Dependency()
    {
        var a = new DependencyDescriptor(
            name: nameof(DiamondA),
            dependencyType: typeof(DiamondA),
            new DiamondA(),
            dependencies: [typeof(DiamondB)]);

        Invoking(() => CreateDependencyManager([a]))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_Throws_When_Circular_Dependency()
    {
        var a = new DependencyDescriptor(
            name: nameof(CircularA),
            dependencyType: typeof(CircularA),
            new CircularA(),
            dependencies: [typeof(CircularB)]);

        var b = new DependencyDescriptor(
            name: nameof(CircularB),
            dependencyType: typeof(CircularB),
            new CircularB(),
            dependencies: [typeof(CircularA)]);

        Invoking(() => CreateDependencyManager([a, b]))
            .Should().ThrowExactly<CircularDependencyException>();
    }

    [Fact]
    public async Task ConfigureAsync_Invokes_Each_Configurator_Once_In_Diamond_Graph()
    {
        var a = new DependencyDescriptor(
            name: nameof(DiamondA),
            dependencyType: typeof(DiamondA),
            new DiamondA(),
            dependencies: [typeof(DiamondB), typeof(DiamondC)]);

        var b = new DependencyDescriptor(
            name: nameof(DiamondB),
            dependencyType: typeof(DiamondB),
            new DiamondB(),
            dependencies: [typeof(DiamondD)]);

        var c = new DependencyDescriptor(
            name: nameof(DiamondC),
            dependencyType: typeof(DiamondC),
            new DiamondC(),
            dependencies: [typeof(DiamondD)]);

        var d = new DependencyDescriptor(
            name: nameof(DiamondD),
            dependencyType: typeof(DiamondD),
            new DiamondD(),
            dependencies: []);

        var manager = CreateDependencyManager([a, b, c, d]);

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var context = new ConfigurationContext(FakeAielEnvironment.Create(), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        a.Instance.Should().BeOfType<DiamondA>().Which.ConfigureCount.Should().Be(1);
        b.Instance.Should().BeOfType<DiamondB>().Which.ConfigureCount.Should().Be(1);
        c.Instance.Should().BeOfType<DiamondC>().Which.ConfigureCount.Should().Be(1);
        d.Instance.Should().BeOfType<DiamondD>().Which.ConfigureCount.Should().Be(1);
    }

    [Fact]
    public async Task InitializeAsync_Invokes_Each_Initializer_Once_In_Linear_Graph()
    {
        var a = new DependencyDescriptor(
            name: nameof(LinearA),
            dependencyType: typeof(LinearA),
            new LinearA(),
            dependencies: [typeof(LinearB)]);

        var b = new DependencyDescriptor(
            name: nameof(LinearB),
            dependencyType: typeof(LinearB),
            new LinearB(),
            dependencies: [typeof(LinearC)]);

        var c = new DependencyDescriptor(
            name: nameof(LinearC),
            dependencyType: typeof(LinearC),
            new LinearC(),
            dependencies: []);

        var manager = CreateDependencyManager([a, b, c]);

        var context = CreateInitializationContextAsync();

        await manager.InitializeAsync(context, CancellationToken.None);

        a.Instance.Should().BeOfType<LinearA>().Which.InitializeCount.Should().Be(1);
        b.Instance.Should().BeOfType<LinearB>().Which.InitializeCount.Should().Be(1);
        c.Instance.Should().BeOfType<LinearC>().Which.InitializeCount.Should().Be(1);
    }

    [Fact]
    public async Task PreConfigureAsync_Is_Invoked_Once_Per_Configurator_In_Diamond_Graph()
    {
        var a = new DependencyDescriptor(
            name: nameof(DiamondA),
            dependencyType: typeof(DiamondA),
            new DiamondA(),
            dependencies: [typeof(DiamondB), typeof(DiamondC)]);

        var b = new DependencyDescriptor(
            name: nameof(DiamondB),
            dependencyType: typeof(DiamondB),
            new DiamondB(),
            dependencies: [typeof(DiamondD)]);

        var c = new DependencyDescriptor(
            name: nameof(DiamondC),
            dependencyType: typeof(DiamondC),
            new DiamondC(),
            dependencies: [typeof(DiamondD)]);

        var d = new DependencyDescriptor(
            name: nameof(DiamondD),
            dependencyType: typeof(DiamondD),
            new DiamondD(),
            dependencies: []);

        var manager = CreateDependencyManager([a, b, c, d]);
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var context = new ConfigurationContext(FakeAielEnvironment.Create(), services, configuration);

        await manager.ConfigureAsync(context, TestContext.Current.CancellationToken);

        a.Instance.Should().BeOfType<DiamondA>().Which.PreConfigureCount.Should().Be(1);
        b.Instance.Should().BeOfType<DiamondB>().Which.PreConfigureCount.Should().Be(1);
        c.Instance.Should().BeOfType<DiamondC>().Which.PreConfigureCount.Should().Be(1);
        d.Instance.Should().BeOfType<DiamondD>().Which.PreConfigureCount.Should().Be(1);
    }

    [Fact]
    public async Task ConfigureAsync_Runs_All_PreConfigureAsync_Before_Any_ConfigureAsync_In_Linear_Graph()
    {
        PhaseLog.Clear();

        var a = new DependencyDescriptor(
            name: nameof(PhaseA),
            dependencyType: typeof(PhaseA),
            new PhaseA(),
            dependencies: [typeof(PhaseB)]);

        var b = new DependencyDescriptor(
            name: nameof(PhaseB),
            dependencyType: typeof(PhaseB),
            new PhaseB(),
            dependencies: []);

        var manager = CreateDependencyManager([a, b]);
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
