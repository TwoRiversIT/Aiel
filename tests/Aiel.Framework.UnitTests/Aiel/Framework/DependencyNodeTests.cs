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

public sealed class DependencyNodeTests
{
    [Fact]
    public void Constructor_Assigns_Properties()
    {
        var descriptor = new DependencyNode(
            type: typeof(TestConfigurator),
            depth: 99,
            new TestConfigurator(),
            dependencies: [new DependencyNode(
                type: typeof(Dependency),
                depth: 100,
                instance: new Dependency(),
                dependencies: [])]);

        descriptor.Type.Should().Be<TestConfigurator>();
        descriptor.Depth.Should().Be(99);
        descriptor.Instance.Should().BeOfType<TestConfigurator>();
        descriptor.Dependencies.Should().ContainSingle().Which.Type.Should().Be<Dependency>();
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_Type_Is_Null()
    {
        Action act = () => _ = new DependencyNode(
            type: null!,
            depth: 0,
            instance: new TestConfigurator(),
            dependencies: []);
        act.Should().Throw<ArgumentNullException>().WithParameterName("type");
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_Instance_Is_Null()
    {
        Action act = () => _ = new DependencyNode(
            type: typeof(TestConfigurator),
            depth: 0,
            instance: null!,
            dependencies: []);
        act.Should().Throw<ArgumentNullException>().WithParameterName("instance");
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_Dependencies_Is_Null()
    {
        Action act = () => _ = new DependencyNode(
            type: typeof(TestConfigurator),
            depth: 0,
            instance: new TestConfigurator(),
            dependencies: null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("dependencies");
    }

    [Fact]
    public void Equals_Returns_True_For_Same_Type()
    {
        var node1 = new DependencyNode(
            type: typeof(TestConfigurator),
            depth: 0,
            instance: new TestConfigurator(),
            dependencies: []);

        var node2 = new DependencyNode(
            type: typeof(TestConfigurator),
            depth: 1,
            instance: new TestConfigurator(),
            dependencies: []);

        node1.Equals(node2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Returns_False_For_Different_Type()
    {
        var node1 = new DependencyNode(
            type: typeof(TestConfigurator),
            depth: 0,
            instance: new TestConfigurator(),
            dependencies: []);

        var node2 = new DependencyNode(
            type: typeof(Dependency),
            depth: 0,
            instance: new Dependency(),
            dependencies: []);

        node1.Equals(node2).Should().BeFalse();
    }

    [DependsOn(typeof(Dependency))]
    private sealed class TestConfigurator : AielDependency;

    private sealed class Dependency : AielDependency;
}
