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
using Aiel.Framework.DependencyInjection;
using Microsoft.Extensions.Configuration;
using static AwesomeAssertions.FluentActions;

namespace Aiel.Framework;

public abstract class AielDependencyManagerTests : PhaseLogCollector
{
    protected readonly ConfigurationContext Context = new(FakeAielEnvironment.Create(), new ConfigurationBuilder().Build(), new ObservableServiceCollectionWrapper());

    public abstract DependencyManager CreateDependencyManager();
    public abstract InitializationContext CreateInitializationContextAsync();

    [Fact]
    public async Task ConfigureAsync_Throws_When_Circular_Dependency()
    {
        var manager = CreateDependencyManager();

        var a = new DependencyNode(
            type: typeof(CircularA),
            depth: 0,
            new CircularA(),
            dependencies: [new DependencyNode(
                type: typeof(CircularB),
                depth: 1,
                new CircularB(),
                dependencies: [])]);

        var b = new DependencyNode(
            type: typeof(CircularB),
            depth: 0,
            new CircularB(),
            dependencies: [new DependencyNode(
                type: typeof(CircularA),
                depth: 1,
                new CircularA(),
                dependencies: [])]);

        await Invoking(async () => await manager.ConfigureAsync([a, b], TestContext.Current.CancellationToken))
            .Should().ThrowAsync<CircularDependencyException>();
    }

    [Fact]
    public async Task ConfigureAsync_Invokes_Each_Configurator_Once_In_Diamond_Graph()
    {
        var manager = CreateDependencyManager();

        var a = new DependencyNode(
            type: typeof(DiamondA),
            depth: 0,
            new DiamondA(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondB),
                depth: 1,
                new DiamondB(),
                dependencies: []), new DependencyNode(
                type: typeof(DiamondC),
                depth: 1,
                new DiamondC(),
                dependencies: [])]);

        var b = new DependencyNode(
            type: typeof(DiamondB),
            depth: 0,
            new DiamondB(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondD),
                depth: 1,
                new DiamondD(),
                dependencies: [])]);

        var c = new DependencyNode(
            type: typeof(DiamondC),
            depth: 0,
            new DiamondC(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondD),
                depth: 1,
                new DiamondD(),
                dependencies: [])]);

        var d = new DependencyNode(
            type: typeof(DiamondD),
            depth: 0,
            new DiamondD(),
            dependencies: []);

        await manager.ConfigureAsync([a, b, c, d], TestContext.Current.CancellationToken);

        a.Instance.Should().BeOfType<DiamondA>().Which.ConfigureCount.Should().Be(1);
        b.Instance.Should().BeOfType<DiamondB>().Which.ConfigureCount.Should().Be(1);
        c.Instance.Should().BeOfType<DiamondC>().Which.ConfigureCount.Should().Be(1);
        d.Instance.Should().BeOfType<DiamondD>().Which.ConfigureCount.Should().Be(1);
    }

    [Fact]
    public async Task InitializeAsync_Invokes_Each_Initializer_Once_In_Linear_Graph()
    {
        var manager = CreateDependencyManager();

        var a = new DependencyNode(
            type: typeof(LinearA),
            depth: 0,
            new LinearA(),
            dependencies: [new DependencyNode(
                type: typeof(LinearB),
                depth: 1,
                new LinearB(),
                dependencies: [])]);

        var b = new DependencyNode(
            type: typeof(LinearB),
            depth: 0,
            new LinearB(),
            dependencies: [new DependencyNode(
                type: typeof(LinearC),
                depth: 1,
                new LinearC(),
                dependencies: [])]);

        var c = new DependencyNode(
            type: typeof(LinearC),
            depth: 0,
            new LinearC(),
            dependencies: []);

        await manager.ConfigureAsync([a, b, c]);

        var context = CreateInitializationContextAsync();

        await manager.InitializeAsync(context, CancellationToken.None);

        a.Instance.Should().BeOfType<LinearA>().Which.InitializeCount.Should().Be(1);
        b.Instance.Should().BeOfType<LinearB>().Which.InitializeCount.Should().Be(1);
        c.Instance.Should().BeOfType<LinearC>().Which.InitializeCount.Should().Be(1);
    }

    [Fact]
    public async Task PreConfigureAsync_Is_Invoked_Once_Per_Configurator_In_Diamond_Graph()
    {
        var manager = CreateDependencyManager();

        var a = new DependencyNode(
            type: typeof(DiamondA),
            depth: 0,
            new DiamondA(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondB),
                depth: 1,
                new DiamondB(),
                dependencies: []), new DependencyNode(
                type: typeof(DiamondC),
                depth: 1,
                new DiamondC(),
                dependencies: [])]);

        var b = new DependencyNode(
            type: typeof(DiamondB),
            depth: 0,
            new DiamondB(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondD),
                depth: 1,
                new DiamondD(),
                dependencies: [])]);

        var c = new DependencyNode(
            type: typeof(DiamondC),
            depth: 0,
            new DiamondC(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondD),
                depth: 1,
                new DiamondD(),
                dependencies: [])]);

        var d = new DependencyNode(
            type: typeof(DiamondD),
            depth: 0,
            new DiamondD(),
            dependencies: []);

        await manager.ConfigureAsync([a, b, c, d], TestContext.Current.CancellationToken);

        a.Instance.Should().BeOfType<DiamondA>().Which.PreConfigureCount.Should().Be(1);
        b.Instance.Should().BeOfType<DiamondB>().Which.PreConfigureCount.Should().Be(1);
        c.Instance.Should().BeOfType<DiamondC>().Which.PreConfigureCount.Should().Be(1);
        d.Instance.Should().BeOfType<DiamondD>().Which.PreConfigureCount.Should().Be(1);
    }

    [Fact]
    public async Task ConfigureAsync_Runs_All_PreConfigureAsync_Before_Any_ConfigureAsync_In_Linear_Graph()
    {
        PhaseLog.Clear();

        var manager = CreateDependencyManager();

        var a = new DependencyNode(
            type: typeof(PhaseA),
            depth: 0,
            new PhaseA(),
            dependencies: [new DependencyNode(
                type: typeof(PhaseB),
                depth: 1,
                new PhaseB(),
                dependencies: [])]);

        var b = new DependencyNode(
            type: typeof(PhaseB),
            depth: 0,
            new PhaseB(),
            dependencies: []);

        await manager.ConfigureAsync([a, b], TestContext.Current.CancellationToken);

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
