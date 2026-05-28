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

using static FluentAssertions.FluentActions;

namespace Aiel.Collections;

public sealed class TypeSetTests
{
    [Fact]
    public void Add_ByInstance_Adds_RuntimeType()
    {
        var sut = new TypeSet<IAnimal>
        {
            new Dog()
        };

        sut.Should().Contain(typeof(Dog));
    }

    [Fact]
    public void Add_ByType_Throws_When_TypeIsNotAssignableToBase()
    {
        var sut = new TypeSet<IAnimal>();

        Invoking(() => sut.Add(typeof(String))).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Contains_ByInstance_ReturnsTrue_ForAddedRuntimeType()
    {
        var sut = new TypeSet<IAnimal>
        {
            typeof(Dog)
        };

        sut.Contains(new Dog()).Should().BeTrue();
    }

    [Fact]
    public void Remove_ByInstance_Removes_RuntimeType()
    {
        var sut = new TypeSet<IAnimal>
        {
            new Dog()
        };

        sut.Remove(new Dog()).Should().BeTrue();
    }

    [Fact]
    public void UnionWith_Adds_All_CompatibleTypes()
    {
        var sut = new TypeSet<IAnimal>();

        sut.UnionWith([typeof(Dog), typeof(Cat)]);

        sut.Count.Should().Be(2);
    }

    [Fact]
    public void UnionWith_Throws_When_SequenceContainsIncompatibleType()
    {
        var sut = new TypeSet<IAnimal>();

        Invoking(() => sut.UnionWith([typeof(Dog), typeof(String)])).Should().Throw<ArgumentException>();
    }

    private interface IAnimal;

    private sealed class Dog : IAnimal;

    private sealed class Cat : IAnimal;
}
