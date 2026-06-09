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
using System.Diagnostics.CodeAnalysis;
using static FluentAssertions.FluentActions;

namespace Aiel.InternetTypes;

public class DomainNameTests
{
    private const String LongDomain = "abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.com";

    [Fact]
    public void Must_return_the_domain_when_ToString_is_called()
    {
        new DomainName("www.example.com").ToString().Should().Be("www.example.com");
    }

    [Fact]
    public void Must_parse_valid_domain_names()
    {
        DomainName.Parse("example.com").Should().Be(new DomainName("example.com"));
        DomainName.Parse("example.com.").Should().Be(new DomainName("example.com"));
    }

    [Fact]
    public void Must_throw_when_Parse_receives_invalid_domain_names()
    {
        Invoking(() => DomainName.Parse("example")).Should().Throw<ArgumentException>();
        Invoking(() => DomainName.Parse(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Must_return_true_when_TryParse_receives_valid_domain_names()
    {
        DomainName.TryParse("example.com", out var domainName).Should().BeTrue();
        domainName.Should().Be(new DomainName("example.com"));
    }

    [Fact]
    public void Must_return_false_when_TryParse_receives_invalid_domain_names()
    {
        DomainName.TryParse("example", out var domainName).Should().BeFalse();
        domainName.Should().Be(DomainName.Empty);
    }

    [Fact]
    public void Must_be_comparable()
    {
        DomainName a = "example.com";
        DomainName b = "example.com";
        DomainName c = "apple.com";
        DomainName d = "orange.com";

        a.CompareTo(b).Should().Be(0);
        b.CompareTo(a).Should().Be(0);

        b.CompareTo(c).Should().Be(1);
        b.CompareTo(d).Should().Be(-1);
    }

    [Fact]
    public void Must_be_equatable()
    {
        DomainName a = "example.com";
        DomainName b = "example.com";
        DomainName c = "apple.com";
        DomainName d = "orange.com";

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
    public void Must_be_equatable_to_Empty()
    {
        var a = new DomainName("example.com");
        a.Equals(DomainName.Empty).Should().Be(false);
    }

    [Fact]
    public void Must_be_equatable_to_String()
    {
        var a = new DomainName("example.com");
        a.Equals("example.com").Should().Be(true);
    }

    [Fact]
    public void Must_be_assignable_to_String()
    {
        String s = new DomainName("example.com");
        s.Should().Be("example.com");
    }

    [Fact]
    public void Must_be_assignable_from_String()
    {
        DomainName a = "example.com";
        a.Should().Be(new DomainName("example.com"));
    }

    [Fact]
    public void Must_be_comparable_to_String()
        => new DomainName("example.com").CompareTo("example.com").Should().Be(0);

    [Fact]
    public void Must_throw_ArgumentException_when_domain_exceeds_255_characters()
        => Invoking(() => new DomainName(LongDomain)).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_any_label_exceeds_63_characters()
    {
        // First label is too long, 2nd Level Domain
        Invoking(() => new DomainName("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.com")).Should().Throw<ArgumentException>();
        // First label is too long, 3rd Level Domain
        Invoking(() => new DomainName("www.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.com")).Should().Throw<ArgumentException>();
        // Middle label is too long, 3rd Level Domain
        Invoking(() => new DomainName("www.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.com")).Should().Throw<ArgumentException>();
        // Last Label is too long, 2nd Level Domain
        Invoking(() => new DomainName("example.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")).Should().Throw<ArgumentException>();
        // Last Label is too long, 3rd Level Domain
        Invoking(() => new DomainName("www.example.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Must_throw_ArgumentException_when_domain_starts_with_a_period()
        => Invoking(() => new DomainName(".example.com")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_remove_trailing_period_when_domain_ends_with_a_period()
        => new DomainName("example.com.").Should().Be(new DomainName("example.com"));

    [Fact]
    public void Must_throw_ArgumentException_when_domain_contains_two_consecutive_periods()
        => Invoking(() => new DomainName("example..com")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_empty()
        => Invoking(() => new DomainName("")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_whitespace()
        => Invoking(() => new DomainName("   ")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_a_single_label()
        => Invoking(() => new DomainName("example")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentNullException_when_domain_null()
        => Invoking(() => new DomainName(null!)).Should().Throw<ArgumentNullException>();

    [Fact]
    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    public void Must_accept_IPv4_or_IPv6_addresses()
    {
        DomainName a = "192.168.1.1";
        DomainName b = "2001:0db8:0000:0000:0000:ff00:0042:8329";
        DomainName c = "2001:db8:0:0:0:ff00:42:8329";
        DomainName d = "2001:db8::ff00:42:8329";
    }
}
