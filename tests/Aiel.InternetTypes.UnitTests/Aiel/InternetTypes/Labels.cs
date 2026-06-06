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

using Aiel.Internet;
using static FluentAssertions.FluentActions;

namespace Aiel.InternetTypes;

public class Labels
{
    [Fact]
    public void Must_be_comparable()
    {
        Label a = "example.com.";
        Label b = "example.com.";
        Label c = "apple.com.";
        Label d = "orange.com.";

        a.CompareTo(b).Should().Be(0);
        b.CompareTo(a).Should().Be(0);

        b.CompareTo(c).Should().Be(1);
        b.CompareTo(d).Should().Be(-1);
    }

    [Fact]
    public void Must_be_equatable()
    {
        Label a = "example.com.";
        Label b = "example.com.";
        Label c = "apple.com.";
        Label d = "orange.com.";

        (a == b).Should().BeTrue();
        (b == a).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        b.Equals(a).Should().BeTrue();

        (c != d).Should().BeTrue();
        (d != c).Should().BeTrue();
        c.Equals(d).Should().BeFalse();
        d.Equals(c).Should().BeFalse();
    }

    [Fact]
    public void Must_be_assignable_to_String()
    {
        String s = new Label("example.com.");
        s.Should().Be("example.com.");
    }

    [Fact]
    public void Must_be_assignable_from_String()
    {
        Label a = "example.com.";
        a.Should().Be("example.com.");
    }

    [Fact]
    public void Must_be_comparable_to_String()
    {
        var d = new Label("example.com.");
        d.CompareTo("example.com.").Should().Be(0);
    }

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_empty_or_whitespace()
    {
        Invoking(() => new Label("")).Should().Throw<ArgumentException>();

        Invoking(() => new Label("   ")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Must_throws_ArgumentNullException_when_domain_null()
    {
        Invoking(() => new Label(null!)).Should().Throw<ArgumentNullException>();
    }
}
