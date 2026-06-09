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

namespace Aiel.Domain;

public sealed class ValueObjectTests
{
    // -----------------------------------------------------------------------
    // Equals — structural equality
    // -----------------------------------------------------------------------

    [Fact]
    public void Equals_SameComponents_ReturnsTrue()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(9.99m, "USD");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentComponents_ReturnsFalse()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(5.00m, "USD");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var a = new Money(9.99m, "USD");

        a.Equals((ValueObject?)null).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var a = new Money(9.99m, "USD");

        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentType_SameComponents_ReturnsFalse()
    {
        // Money and Price share the same component shape but are distinct types.
        var money = new Money(9.99m, "USD");
        var price = new Price(9.99m, "USD");

        money.Equals(price).Should().BeFalse();
    }

    [Fact]
    public void Equals_ComponentOrderMatters_ReturnsFalse()
    {
        var forward = new OrderedPair(1, 2);
        var reversed = new OrderedPair(2, 1);

        forward.Equals(reversed).Should().BeFalse();
    }

    [Fact]
    public void Equals_NullComponent_MatchingNulls_ReturnsTrue()
    {
        var a = new FullName(null, "Smith");
        var b = new FullName(null, "Smith");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullComponent_NullVsNonNull_ReturnsFalse()
    {
        var a = new FullName(null, "Smith");
        var b = new FullName("Alice", "Smith");

        a.Equals(b).Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Equals — object overload
    // -----------------------------------------------------------------------

    [Fact]
    public void Equals_ObjectOverload_SameComponents_ReturnsTrue()
    {
        var a = new Money(1.00m, "GBP");
        Object b = new Money(1.00m, "GBP");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_ObjectOverload_NonValueObject_ReturnsFalse()
    {
        var a = new Money(1.00m, "GBP");

        a.Equals((Object)"not a value object").Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // GetHashCode
    // -----------------------------------------------------------------------

    [Fact]
    public void GetHashCode_EqualInstances_ReturnSameCode()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(9.99m, "USD");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_UnequalInstances_ReturnDifferentCodes()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(5.00m, "EUR");

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_RepeatedCalls_ReturnSameValue()
    {
        var a = new Money(9.99m, "USD");

        var first = a.GetHashCode();
        var second = a.GetHashCode();

        first.Should().Be(second);
    }

    // -----------------------------------------------------------------------
    // == operator
    // -----------------------------------------------------------------------

    [Fact]
    public void EqualityOperator_EqualInstances_ReturnsTrue()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(9.99m, "USD");

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_UnequalInstances_ReturnsFalse()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(1.00m, "USD");

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_LeftNull_ReturnsFalse()
    {
        Money? a = null;
        var b = new Money(9.99m, "USD");

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_RightNull_ReturnsFalse()
    {
        var a = new Money(9.99m, "USD");
        Money? b = null;

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        ValueObject? a = null;
        ValueObject? b = null;

        (a == b).Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // != operator
    // -----------------------------------------------------------------------

    [Fact]
    public void InequalityOperator_EqualInstances_ReturnsFalse()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(9.99m, "USD");

        (a != b).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_UnequalInstances_ReturnsTrue()
    {
        var a = new Money(9.99m, "USD");
        var b = new Money(1.00m, "USD");

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_LeftNull_ReturnsTrue()
    {
        Money? a = null;
        var b = new Money(9.99m, "USD");

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_RightNull_ReturnsTrue()
    {
        var a = new Money(9.99m, "USD");
        Money? b = null;

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_BothNull_ReturnsFalse()
    {
        ValueObject? a = null;
        ValueObject? b = null;

        (a != b).Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Edge cases
    // -----------------------------------------------------------------------

    [Fact]
    public void SingleComponent_EqualInstances_AreEqual()
    {
        var a = new Tag("hello");
        var b = new Tag("hello");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void MultiComponent_EqualInstances_AreEqual()
    {
        var a = new Money(100m, "CAD");
        var b = new Money(100m, "CAD");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void MixedTypes_EqualInstances_AreEqual()
    {
        var a = new Composite(42, "hello");
        var b = new Composite(42, "hello");

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void MixedTypes_DifferentInstances_AreNotEqual()
    {
        var a = new Composite(42, "hello");
        var b = new Composite(42, "world");

        a.Equals(b).Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Test fixtures
    // -----------------------------------------------------------------------

    private sealed class Money(Decimal amount, String currency) : ValueObject
    {
        protected override IEnumerable<Object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    // Same component shape as Money; used to verify type identity is checked.
    private sealed class Price(Decimal amount, String currency) : ValueObject
    {
        protected override IEnumerable<Object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    private sealed class Tag(String? value) : ValueObject
    {
        protected override IEnumerable<Object?> GetEqualityComponents()
        {
            yield return value;
        }
    }

    private sealed class FullName(String? first, String? last) : ValueObject
    {
        protected override IEnumerable<Object?> GetEqualityComponents()
        {
            yield return first;
            yield return last;
        }
    }

    private sealed class Composite(Int32 number, String text) : ValueObject
    {
        protected override IEnumerable<Object?> GetEqualityComponents()
        {
            yield return number;
            yield return text;
        }
    }

    private sealed class OrderedPair(Int32 first, Int32 second) : ValueObject
    {
        protected override IEnumerable<Object?> GetEqualityComponents()
        {
            yield return first;
            yield return second;
        }
    }
}
