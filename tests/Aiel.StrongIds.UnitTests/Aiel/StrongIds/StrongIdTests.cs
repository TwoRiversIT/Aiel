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

using System.Diagnostics.CodeAnalysis;

namespace Aiel.StrongIds;

[SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "Its freaking unit tests!")]
public class StrongIdTests
{
    [Fact]
    public void StrongId_WithSameValue_AreEqual()
    {
        // The only real value of this test is as a shape/smoke test confirming
        // the generated type is a record struct and not accidentally a class.

        var id1 = new GuidDisallowDefaultId(Guid.NewGuid());
        var id2 = new GuidDisallowDefaultId(id1.Value);
        id1.Should().Be(id2);
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void StrongId_DoesNotAllowDefault_WhenDisallowDefaultIsTrue()
    {
        Action act = () => new GuidDisallowDefaultId(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StrongId_AllowsDefault_WhenDisallowDefaultIsFalse()
    {
        Action act = () => new Int32AllowDefaultId(0);

        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsTrue_WhenStringIsNull_ThrowsArgumentException()
    {
        Action act = () => new StringDisallowDefaultId(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsTrue_WhenStringIsWhitespace_ThrowsArgumentException()
    {
        Action act = () => new StringDisallowDefaultId("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsNull_DoesNotThrow()
    {
        Action act = () => new StringAllowDefaultId(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsEmpty_DoesNotThrow()
    {
        Action act = () => new StringAllowDefaultId(String.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsWhitespace_DoesNotThrow()
    {
        Action act = () => new StringAllowDefaultId("   ");
        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsNull_ValueIsEmpty()
    {
        var id = new StringAllowDefaultId(null!);
        id.Value.Should().NotBeNull();
        id.Value.Should().BeEmpty();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsWhitespace_ValueIsEmpty()
    {
        var id = new StringAllowDefaultId("   ");
        id.Value.Should().NotBeNull();
        id.Value.Should().BeEmpty();
    }

    [Fact]
    public void StrongId_IsEmpty_ReturnsTrue_WhenValueIsDefault()
    {
        var id = new Int32AllowDefaultId(0);
        id.IsDefault.Should().BeTrue();
    }
}
