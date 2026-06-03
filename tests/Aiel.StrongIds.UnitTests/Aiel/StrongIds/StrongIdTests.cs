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

[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct MyGuidId;

[StrongId<Int32>(DisallowDefault = false)]
public readonly partial record struct MyInt32Id;

[StrongId<String>(DisallowDefault = true)]
public readonly partial record struct DisallowDefaultStringId;

[StrongId<String>(DisallowDefault = false)]
public readonly partial record struct AllowDefaultStringId;

public class StrongIdTests
{

    [Fact]
    public void StrongId_DoesNotAllowDefault_WhenDisallowDefaultIsTrue()
    {
        Action act = () => new MyGuidId(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StrongId_AllowsDefault_WhenDisallowDefaultIsFalse()
    {
        Action act = () => new MyInt32Id(0);

        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsTrue_WhenStringIsNull_ThrowsArgumentException()
    {
        Action act = () => new DisallowDefaultStringId(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsTrue_WhenStringIsWhitespace_ThrowsArgumentException()
    {
        Action act = () => new DisallowDefaultStringId("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsNull_DoesNotThrow()
    {
        Action act = () => new AllowDefaultStringId(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsEmpty_DoesNotThrow()
    {
        Action act = () => new AllowDefaultStringId(String.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsWhitespace_DoesNotThrow()
    {
        Action act = () => new AllowDefaultStringId("   ");
        act.Should().NotThrow();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsNull_ValueIsEmpty()
    {
        var id = new AllowDefaultStringId(null!);
        id.Value.Should().NotBeNull();
        id.Value.Should().BeEmpty();
    }

    [Fact]
    public void StrongId_GivenDisallowDefaultIsFalse_WhenStringIsWhitespace_ValueIsEmpty()
    {
        var id = new AllowDefaultStringId("   ");
        id.Value.Should().NotBeNull();
        id.Value.Should().BeEmpty();
    }

    [Fact]
    public void StrongId_IsEmpty_ReturnsTrue_WhenValueIsDefault()
    {
        var id = new MyInt32Id(0);
        id.IsDefault.Should().BeTrue();
    }
}
