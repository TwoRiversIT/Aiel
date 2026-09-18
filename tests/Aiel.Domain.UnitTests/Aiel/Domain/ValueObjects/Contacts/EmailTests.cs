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

using System.Text.Json;
using static AwesomeAssertions.FluentActions;

namespace Aiel.Domain.ValueObjects.Contacts;

public class EmailTests
{
    [Fact]
    public void From_Returns_Email_When_Input_Is_Valid()
    {
        var b = Email.From("a@x.yz");
        b.ToString().Should().Be("a@x.yz");
    }

    [Fact]
    public void Implicit_Conversion_To_String_Returns_Formatted_String()
    {
        String b = Email.From("a@x.yz");
        b.Should().Be("a@x.yz");
    }

    [Fact]
    public void From_Returns_Empty_When_Input_Is_Null()
    {
        var email = Email.From(null!);
        email.Should().Be(Email.Empty);
    }

    [Fact]
    public void From_Returns_Empty_When_Input_Is_Invalid()
    {
        var email = Email.From("Bob's Burger");
        email.Should().Be(Email.Empty);
    }

    [Fact]
    public void Email_Is_Equatable_To_Email()
    {
        Email.From("a@x.yz").Should().Be(Email.From("a@x.yz"));
        Email.From("a@x.yz").Should().NotBe(Email.From("b@x.yz"));

        // Local is case sensitive, Domain is not case sensitive
        Email.From("a@x.yz").Should().NotBe(Email.From("A@X.YZ")); // Local upper, domain upper
        Email.From("a@x.yz").Should().NotBe(Email.From("A@x.yz")); // Local upper, domain lower
        Email.From("a@x.yz").Should().Be(Email.From("a@X.YZ")); // Local lower, domain upper
    }

    [Fact]
    public void Email_Must_Be_Equal_To_String_With_Same_Email()
    {
        Email.From("a@x.yz").Equals("a@x.yz").Should().BeTrue();

        // Local is case sensitive, Domain is not case sensitive
        Email.From("a@x.yz").Equals("A@X.YZ").Should().BeFalse(); // Local upper, domain upper
        Email.From("a@x.yz").Equals("A@x.yz").Should().BeFalse(); // Local upper, domain lower
        Email.From("a@x.yz").Equals("a@X.YZ").Should().BeTrue(); // Local lower, domain upper
    }

    [Fact]
    public void Email_is_Comparable()
    {
        var a = Email.From("a@x.yz");
        var b = Email.From("a@x.yz");
        var c = Email.From("b@x.yz");

        (a == b).Should().BeTrue();
        (a < b).Should().BeFalse();
        (a < c).Should().BeTrue();

        (c == a).Should().BeFalse();
        (c < a).Should().BeFalse();
        (c > a).Should().BeTrue();

        a.CompareTo(b).Should().Be(0);
        b.CompareTo(c).Should().Be(-1);
        c.CompareTo(a).Should().Be(1);
    }

    [Fact]
    public void From_Returns_Email_For_Valid_Input()
    {
        var a = Email.From("a@x.yz");
        a.ToString().Should().Be("a@x.yz");
    }

    [Fact]
    public void From_Returns_Empty_For_Invalid_Input()
    {
        var b = Email.From("z at x dot yz");
        b.ToString().Should().Be(String.Empty);
    }

    [Fact]
    public void Parse_Returns_Email_For_Valid_Input()
    {
        Email.Parse("a@x.yz").Should().Be(Email.From("a@x.yz"));
    }

    [Fact]
    public void Parse_Throws_For_Invalid_Input()
    {
        Invoking(() => Email.Parse("z at x dot yz")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_can_be_serialized_to_and_from_JSON()
    {
        var before = Email.From("a@x.yz");

        var json = JsonSerializer.Serialize(before);

        var after = JsonSerializer.Deserialize<Email>(json);

        after.Should().Be(before);
    }
}
