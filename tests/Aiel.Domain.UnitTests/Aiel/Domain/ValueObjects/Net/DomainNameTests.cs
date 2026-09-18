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

using static AwesomeAssertions.FluentActions;

namespace Aiel.Domain.ValueObjects.Net;

public class DomainNameTests
{
    private const String LongDomain = "abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.com";

    [Fact]
    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    public void Must_accept_IPv4_or_IPv6_addresses()
    {
        var a = DomainName.From("192.168.1.1");
        var b = DomainName.From("2001:0db8:0000:0000:0000:ff00:0042:8329");
        var c = DomainName.From("2001:db8:0:0:0:ff00:42:8329");
        var d = DomainName.From("2001:db8::ff00:42:8329");
    }

    [Fact]
    public void Must_be_Castable_from_String()
    {
        var a = (DomainName)"example.com";
        a.Should().Be(DomainName.From("example.com"));
    }

    [Fact]
    public void Must_be_Assignable_to_String()
    {
        String s = DomainName.From("example.com");
        s.Should().Be("example.com");
    }

    [Fact]
    public void Must_be_Comparable()
    {
        var a = DomainName.From("example.com");
        var b = DomainName.From("example.com");
        var c = DomainName.From("apple.com");
        var d = DomainName.From("orange.com");

        a.CompareTo(b).Should().Be(0);
        b.CompareTo(a).Should().Be(0);

        b.CompareTo(c).Should().BePositive();
        b.CompareTo(d).Should().BeNegative();
    }

    [Fact]
    public void Must_be_Comparable_to_String()
        => DomainName.From("example.com").CompareTo("example.com").Should().Be(0);

    [Fact]
    public void Must_be_Constructable_from_String()
    {
        var a = DomainName.From("example.com");
        a.Should().Be(DomainName.From("example.com"));
    }

    [Fact]
    public void Must_be_Equatable()
    {
        var a = DomainName.From("example.com");
        var b = DomainName.From("example.com");
        var c = DomainName.From("apple.com");
        var d = DomainName.From("orange.com");

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
    public void Must_be_Equatable_to_Empty()
    {
        var a = DomainName.From("example.com");
        a.Equals(DomainName.Empty).Should().Be(false);
    }

    [Fact]
    public void Must_be_Equatable_to_String()
    {
        var a = DomainName.From("example.com");
        a.Equals("example.com").Should().Be(true);
    }

    [Fact]
    public void Must_parse_valid_domain_names()
    {
        DomainName.Parse("example.com").Should().Be(DomainName.From("example.com"));
        DomainName.Parse("example.com.").Should().Be(DomainName.From("example.com"));
    }

    [Fact]
    public void Must_remove_trailing_period_when_domain_ends_with_a_period()
        => DomainName.From("example.com.").Should().Be(DomainName.From("example.com"));

    [Fact]
    public void Must_return_false_when_TryParse_receives_invalid_domain_names()
    {
        DomainName.TryParse("example", out var domainName).Should().BeFalse();
        domainName.Should().Be(DomainName.Empty);
    }

    [Fact]
    public void Must_return_the_domain_when_ToString_is_called()
    {
        DomainName.From("www.example.com").ToString().Should().Be("www.example.com");
    }

    [Fact]
    public void Must_return_true_when_TryParse_receives_valid_domain_names()
    {
        DomainName.TryParse("example.com", out var domainName).Should().BeTrue();
        domainName.Should().Be(DomainName.From("example.com"));
    }

    [Fact]
    public void Must_throw_ArgumentException_when_any_label_exceeds_63_characters()
    {
        // First label is too long, 2nd Level Domain
        Invoking(() => DomainName.Parse("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.com")).Should().Throw<ArgumentException>();
        // First label is too long, 3rd Level Domain
        Invoking(() => DomainName.Parse("www.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.com")).Should().Throw<ArgumentException>();
        // Middle label is too long, 3rd Level Domain
        Invoking(() => DomainName.Parse("www.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.com")).Should().Throw<ArgumentException>();
        // Last Label is too long, 2nd Level Domain
        Invoking(() => DomainName.Parse("example.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")).Should().Throw<ArgumentException>();
        // Last Label is too long, 3rd Level Domain
        Invoking(() => DomainName.Parse("www.example.abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Must_throw_ArgumentException_when_domain_contains_two_consecutive_periods()
        => Invoking(() => DomainName.Parse("example..com")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_Domain_exceeds_255_characters()
        => Invoking(() => DomainName.Parse(LongDomain)).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_a_single_label()
        => Invoking(() => DomainName.Parse("example")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_empty()
        => Invoking(() => DomainName.Parse("")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_is_whitespace()
        => Invoking(() => DomainName.Parse("   ")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentException_when_domain_starts_with_a_period()
        => Invoking(() => DomainName.Parse(".example.com")).Should().Throw<ArgumentException>();

    [Fact]
    public void Must_throw_ArgumentNullException_when_domain_null()
        => Invoking(() => DomainName.Parse(null!)).Should().Throw<ArgumentNullException>();

    [Fact]
    public void Must_throw_when_Parse_receives_invalid_domain_names()
    {
        Invoking(() => DomainName.Parse("example")).Should().Throw<ArgumentException>();
        Invoking(() => DomainName.Parse(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Parameterless_Constructor_produces_DomainName_Empty()
    {
        var domainName = new DomainName();
        domainName.Should().Be(DomainName.Empty);
    }

    [Fact]
    public void When_ToString_is_called_on_Empty_Returns_StringEmpty()
    {
        DomainName.Empty.ToString().Should().Be(String.Empty);
    }
}
