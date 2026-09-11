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

using Aiel.Testing.StrongIds;

namespace Aiel.StrongIds;

[SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "It's freaking unit tests!")]
public class StrongIdTests
{
    [Fact]
    public void StrongId_WithSameValue_AreEqual()
    {
        // The only real value of this test is as a shape/smoke test confirming
        // the generated type is a record struct and not accidentally a class.

        var id1 = new GuidAllowDefaultFalseId(Guid.NewGuid());
        var id2 = new GuidAllowDefaultFalseId(id1.Value);
        id1.Should().Be(id2);
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void GivenAllowDefaultIsFalse_WhenValueIsDefault_NewThrowsArgumentException()
    {
        var act = () => new GuidAllowDefaultFalseId(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAllowDefaultIsFalse_WhenValueIsDefault_FromThrowsArgumentException()
    {
        var act = () => GuidAllowDefaultFalseId.From(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAllowDefaultIsFalse_WhenValueIsDefault_TryFromReturnsFalse()
    {
        GuidAllowDefaultFalseId.TryFrom(Guid.Empty, out _).Should().BeFalse();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenValueIsDefault_TryFromReturnsTrue()
    {
        Int32AllowDefaultTrueId.TryFrom(0, out _).Should().BeTrue();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenValueIsDefault_NewShouldNotThrow()
    {
        Action act = () => new Int32AllowDefaultTrueId(0);

        act.Should().NotThrow();
    }

    [Fact]
    public void GivenAllowDefaultIsFalse_WhenStringIsNull_NewThrowsArgumentException()
    {
        Action act = () => new StringAllowDefaultFalseId(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAllowDefaultIsFalse_WhenStringIsWhitespace_NewThrowsArgumentException()
    {
        Action act = () => new StringAllowDefaultFalseId("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenStringIsNull_NewDoesNotThrow()
    {
        Action act = () => new StringAllowDefaultTrueId(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenStringIsEmpty_NewDoesNotThrow()
    {
        Action act = () => new StringAllowDefaultTrueId(String.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenStringIsWhitespace_NewDoesNotThrow()
    {
        Action act = () => new StringAllowDefaultTrueId("   ");
        act.Should().NotThrow();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenStringIsNull_ValueIsEmpty()
    {
        var id = new StringAllowDefaultTrueId(null!);
        id.Value.Should().NotBeNull();
        id.Value.Should().BeEmpty();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenStringIsWhitespace_ValueIsEmpty()
    {
        var id = new StringAllowDefaultTrueId("   ");
        id.Value.Should().NotBeNull();
        id.Value.Should().BeEmpty();
    }

    [Fact]
    public void GivenAllowDefaultIsTrue_WhenValueIsDefault_IsEmptyReturnsTrue()
    {
        var id = new Int32AllowDefaultTrueId(0);
        id.IsDefault.Should().BeTrue();
    }

    #region IComparable<T> Tests for Guid-based StrongIds

    [Fact]
    public void GivenGuidStrongIds_WhenComparingSameValue_ReturnsZero()
    {
        var guid = Guid.NewGuid();
        var id1 = new GuidAllowDefaultFalseId(guid);
        var id2 = new GuidAllowDefaultFalseId(guid);

        id1.CompareTo(id2).Should().Be(0);
    }

    [Fact]
    public void GivenGuidStrongIds_WhenComparingLesserValue_ReturnsNegative()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        // Ensure guid1 is actually less than guid2
        var lesserGuid = guid1.CompareTo(guid2) < 0 ? guid1 : guid2;
        var greaterGuid = guid1.CompareTo(guid2) < 0 ? guid2 : guid1;

        var idLesser = new GuidAllowDefaultFalseId(lesserGuid);
        var idGreater = new GuidAllowDefaultFalseId(greaterGuid);

        idLesser.CompareTo(idGreater).Should().BeNegative();
    }

    [Fact]
    public void GivenGuidStrongIds_WhenComparingGreaterValue_ReturnsPositive()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        // Ensure guid1 is actually greater than guid2
        var lesserGuid = guid1.CompareTo(guid2) < 0 ? guid1 : guid2;
        var greaterGuid = guid1.CompareTo(guid2) < 0 ? guid2 : guid1;

        var idLesser = new GuidAllowDefaultFalseId(lesserGuid);
        var idGreater = new GuidAllowDefaultFalseId(greaterGuid);

        idGreater.CompareTo(idLesser).Should().BePositive();
    }

    [Fact]
    public void GivenGuidStrongIds_CanBeSorted()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();

        var ids = new[]
        {
            new GuidAllowDefaultFalseId(guid3),
            new GuidAllowDefaultFalseId(guid1),
            new GuidAllowDefaultFalseId(guid2)
        };

        var sorted = ids.OrderBy(x => x).ToList();

        sorted[0].Value.CompareTo(sorted[1].Value).Should().BeNegative();
        sorted[1].Value.CompareTo(sorted[2].Value).Should().BeNegative();
    }

    #endregion

    #region IComparable<T> Tests for Integer-based StrongIds

    [Fact]
    public void GivenInt32StrongIds_WhenComparingSameValue_ReturnsZero()
    {
        var id1 = new Int32AllowDefaultTrueId(42);
        var id2 = new Int32AllowDefaultTrueId(42);

        id1.CompareTo(id2).Should().Be(0);
    }

    [Fact]
    public void GivenInt32StrongIds_WhenComparingLesserValue_ReturnsNegative()
    {
        var id1 = new Int32AllowDefaultTrueId(10);
        var id2 = new Int32AllowDefaultTrueId(20);

        id1.CompareTo(id2).Should().BeNegative();
    }

    [Fact]
    public void GivenInt32StrongIds_WhenComparingGreaterValue_ReturnsPositive()
    {
        var id1 = new Int32AllowDefaultTrueId(30);
        var id2 = new Int32AllowDefaultTrueId(15);

        id1.CompareTo(id2).Should().BePositive();
    }

    [Fact]
    public void GivenInt32StrongIds_CanBeSorted()
    {
        var ids = new[]
        {
            new Int32AllowDefaultTrueId(50),
            new Int32AllowDefaultTrueId(10),
            new Int32AllowDefaultTrueId(30),
            new Int32AllowDefaultTrueId(20)
        };

        var sorted = ids.OrderBy(x => x).ToList();

        sorted[0].Value.Should().Be(10);
        sorted[1].Value.Should().Be(20);
        sorted[2].Value.Should().Be(30);
        sorted[3].Value.Should().Be(50);
    }

    [Fact]
    public void GivenInt32StrongIds_WithDefaultValue_CanBeCompared()
    {
        var idDefault = new Int32AllowDefaultTrueId(0);
        var idPositive = new Int32AllowDefaultTrueId(5);

        idDefault.CompareTo(idPositive).Should().BeNegative();
    }

    [Fact]
    public void GivenInt64StrongIds_WhenComparingSameValue_ReturnsZero()
    {
        var id1 = new Int64AllowDefaultTrueId(9223372036854775800L);
        var id2 = new Int64AllowDefaultTrueId(9223372036854775800L);

        id1.CompareTo(id2).Should().Be(0);
    }

    [Fact]
    public void GivenInt64StrongIds_WhenComparingLesserValue_ReturnsNegative()
    {
        var id1 = new Int64AllowDefaultTrueId(100L);
        var id2 = new Int64AllowDefaultTrueId(200L);

        id1.CompareTo(id2).Should().BeNegative();
    }

    [Fact]
    public void GivenInt64StrongIds_WhenComparingGreaterValue_ReturnsPositive()
    {
        var id1 = new Int64AllowDefaultTrueId(300L);
        var id2 = new Int64AllowDefaultTrueId(150L);

        id1.CompareTo(id2).Should().BePositive();
    }

    #endregion

    #region IComparable<T> Tests for String-based StrongIds

    [Fact]
    public void GivenStringStrongIds_WhenComparingSameValue_ReturnsZero()
    {
        var id1 = new StringAllowDefaultFalseId("test-id");
        var id2 = new StringAllowDefaultFalseId("test-id");

        id1.CompareTo(id2).Should().Be(0);
    }

    [Fact]
    public void GivenStringStrongIds_WhenComparingLesserValue_ReturnsNegative()
    {
        var id1 = new StringAllowDefaultFalseId("alpha");
        var id2 = new StringAllowDefaultFalseId("beta");

        id1.CompareTo(id2).Should().BeNegative();
    }

    [Fact]
    public void GivenStringStrongIds_WhenComparingGreaterValue_ReturnsPositive()
    {
        var id1 = new StringAllowDefaultFalseId("zebra");
        var id2 = new StringAllowDefaultFalseId("apple");

        id1.CompareTo(id2).Should().BePositive();
    }

    [Fact]
    public void GivenStringStrongIds_CanBeSorted()
    {
        var ids = new[]
        {
            new StringAllowDefaultFalseId("zebra"),
            new StringAllowDefaultFalseId("apple"),
            new StringAllowDefaultFalseId("mango"),
            new StringAllowDefaultFalseId("banana")
        };

        var sorted = ids.OrderBy(x => x).ToList();

        sorted[0].Value.Should().Be("apple");
        sorted[1].Value.Should().Be("banana");
        sorted[2].Value.Should().Be("mango");
        sorted[3].Value.Should().Be("zebra");
    }

    [Fact]
    public void GivenStringStrongIds_IsCaseSensitiveInComparison()
    {
        var id1 = new StringAllowDefaultFalseId("Test");
        var id2 = new StringAllowDefaultFalseId("test");

        // String comparison is case-sensitive by default (lowercase comes before uppercase)
        id1.CompareTo(id2).Should().BePositive();
    }

    [Fact]
    public void GivenStringAllowDefaultTrueIds_WhenComparingEmptyWithNonEmpty_ReturnsNegative()
    {
        var idEmpty = new StringAllowDefaultTrueId(String.Empty);
        var idNonEmpty = new StringAllowDefaultTrueId("value");

        idEmpty.CompareTo(idNonEmpty).Should().BeNegative();
    }

    [Fact]
    public void GivenStringAllowDefaultTrueIds_WhenComparingNullWithNonEmpty_ReturnsNegative()
    {
        var idNull = new StringAllowDefaultTrueId(null!);
        var idNonEmpty = new StringAllowDefaultTrueId("value");

        // null is converted to empty string, so should be less than non-empty
        idNull.CompareTo(idNonEmpty).Should().BeNegative();
    }

    #endregion
}
