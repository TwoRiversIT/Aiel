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

public abstract class BootstrapTestsBase
{
    public abstract Task BootstrapAsync<TApplication>(IEnumerable<DependencyDescriptor> descriptors)
        where TApplication : class, IApplicationConfigurator, new();

    [Fact]
    public async Task Given_DiamondGraph_Bootstrap_Invokes_ConfigureAsync_Only_Once()
    {
        var a = new DependencyDescriptor(
            name: nameof(DiamondA),
            dependencyType: typeof(DiamondA),
            instance: new DiamondA(),
            dependencies: [typeof(DiamondB), typeof(DiamondC)]);
        var b = new DependencyDescriptor(
            name: nameof(DiamondB),
            dependencyType: typeof(DiamondB),
            instance: new DiamondB(),
            dependencies: [typeof(DiamondD)]);
        var c = new DependencyDescriptor(
            name: nameof(DiamondC),
            dependencyType: typeof(DiamondC),
            instance: new DiamondC(),
            dependencies: [typeof(DiamondD)]);
        var d = new DependencyDescriptor(
            name: nameof(DiamondD),
            dependencyType: typeof(DiamondD),
            instance: new DiamondD(),
            dependencies: []);

        var dependencies = new List<DependencyDescriptor>() { a, b, c, d };

        await BootstrapAsync<DiamondA>(dependencies);

        a.Instance.Should().BeOfType<DiamondA>()
            .Which.ConfigureCount.Should().Be(1);
        b.Instance.Should().BeOfType<DiamondB>()
            .Which.ConfigureCount.Should().Be(1);
        c.Instance.Should().BeOfType<DiamondC>()
            .Which.ConfigureCount.Should().Be(1);
        d.Instance.Should().BeOfType<DiamondD>()
            .Which.ConfigureCount.Should().Be(1);
    }
}
