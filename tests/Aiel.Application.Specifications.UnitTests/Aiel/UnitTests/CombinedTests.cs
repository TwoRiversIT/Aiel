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

using Aiel.Specifications;

namespace Aiel.UnitTests;

public class Specifications
{
    internal class NullSpecification : Specification<String>
    {
        public NullSpecification() { }
    }

    [Fact]
    public void Behaves_appropriately_when_Specification_is_null()
    {
        var spec = new NullSpecification();
        spec.IsSatisfiedBy(null!).Should().BeFalse();
        spec.IsSatisfiedBy(String.Empty).Should().BeFalse();
        spec.IsSatisfiedBy("Something").Should().BeFalse();
    }

    [Fact]
    public void Throws_when_constructed_with_null_parameters()
    {
        Assert.Throws<ArgumentNullException>(() => new Specification<String>(null!));
    }

    internal class IsEven : Specification<Int32>
    {
        public IsEven() : base(n => n % 2 == 0) { }
    }

    internal class IsMultiple(Int32 factor) : Specification<Int32>(n => n % factor == 0)
    {
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(5, false)]
    [InlineData(9, false)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(15, false)]
    [InlineData(20, true)]
    public void Can_be_combined(Int32 input, Boolean expected)
    {
        var isTen = new IsEven().And(new IsMultiple(5));

        isTen.IsSatisfiedBy(input).Should().Be(expected);
    }
}
