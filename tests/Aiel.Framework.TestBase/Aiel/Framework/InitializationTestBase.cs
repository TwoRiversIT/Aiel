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

namespace Aiel.Framework;

public abstract class InitializationTestBase
{
    public abstract Task InitializeAsync<TApplication>(IEnumerable<DependencyNode> descriptors)
        where TApplication : class, IApplicationConfigurator, new();

    [Fact]
    public async Task Given_DiamondGraph_Initialize_Invokes_ConfigureAsync_Only_Once()
    {
        var a = new DependencyNode(
            type: typeof(DiamondA),
            depth: 0,
            instance: new DiamondA(),
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
            instance: new DiamondB(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondD),
                depth: 1,
                new DiamondD(),
                dependencies: [])]);
        var c = new DependencyNode(
            type: typeof(DiamondC),
            depth: 0,
            instance: new DiamondC(),
            dependencies: [new DependencyNode(
                type: typeof(DiamondD),
                depth: 1,
                new DiamondD(),
                dependencies: [])]);
        var d = new DependencyNode(
            type: typeof(DiamondD),
            depth: 0,
            instance: new DiamondD(),
            dependencies: []);

        var dependencies = new List<DependencyNode>() { a, b, c, d };

        await InitializeAsync<DiamondA>(dependencies);

        a.Instance.Should().BeOfType<DiamondA>()
            .Which.InitializeCount.Should().Be(1);
        b.Instance.Should().BeOfType<DiamondB>()
            .Which.InitializeCount.Should().Be(1);
        c.Instance.Should().BeOfType<DiamondC>()
            .Which.InitializeCount.Should().Be(1);
        d.Instance.Should().BeOfType<DiamondD>()
            .Which.InitializeCount.Should().Be(1);
    }
}
