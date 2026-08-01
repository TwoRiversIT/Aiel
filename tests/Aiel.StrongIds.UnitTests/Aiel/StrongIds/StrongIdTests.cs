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

namespace Aiel.StrongIds;

[SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "Its freaking unit tests!")]
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
}
