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
using System.Xml;
using System.Xml.Serialization;
using static FluentAssertions.FluentActions;

namespace Aiel.Emailing;

public class EmailTests
{
    [Fact]
    public void Email_can_be_created()
    {
        var b = new Email("a@x.yz");
        b.Should().NotBe(Email.Empty);

        var c = new Email(String.Empty);
        c.Should().Be(Email.Empty);
    }

    [Fact]
    public void Email_can_be_assigned_from_string()
    {
        Email a = "a@x.yz";
        a.ToString().Should().Be("a@x.yz");
    }

    [Fact]
    public void Email_can_be_assigned_to_string()
    {
        String b = new Email("a@x.yz");
        b.Should().Be("a@x.yz");
    }

    [Fact]
    public void Email_can_be_created_from_null_string()
    {
        new Email(null!).Should().Be(Email.Empty);
    }

    [Fact]
    public void Email_cannot_be_created_from_malformed_email()
    {
        Invoking(() => new Email("Bob's Burger")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_is_Equatable()
    {
        new Email("a@x.yz").Should().Be(new Email("a@x.yz"));
        new Email("a@x.yz").Should().NotBe(new Email("b@x.yz"));

        // Local is case sensitive, Domain is not case sensitive
        new Email("a@x.yz").Should().NotBe(new Email("A@X.YZ")); // Local upper, domain upper
        new Email("a@x.yz").Should().NotBe(new Email("A@x.yz")); // Local upper, domain lower
        new Email("a@x.yz").Should().Be(new Email("a@X.YZ")); // Local lower, domain upper
    }

    [Fact]
    public void Email_is_Comparable()
    {
        var a = new Email("a@x.yz");
        var b = new Email("a@x.yz");
        var c = new Email("b@x.yz");

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
    public void Email_is_Parsable()
    {
        var a = Email.Parse("a@x.yz");
        a.ToString().Should().Be("a@x.yz");

        Invoking(() => Email.Parse("z at x dot yz")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_can_be_serialized_to_and_from_JSON()
    {
        var before = new Email("a@x.yz");

        var json = JsonSerializer.Serialize(before);

        var after = JsonSerializer.Deserialize<Email>(json);

        after.Should().Be(before);
    }

    [Fact]
    public void Email_can_be_serialized_to_and_from_XML()
    {
        var email = new Email("a@x.yz");
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        // Serialization
        var serializer = new XmlSerializer(typeof(Email));
        using (var writer = new StringWriter())
        {
            serializer.Serialize(writer, email);
            var xmlString = writer.ToString();

            // Deserialization using XmlReader for safer processing
            using (var reader = XmlReader.Create(new StringReader(xmlString), settings))
            {
                var deserializedEmail = (Email)serializer.Deserialize(reader)!;

                // Assert
                deserializedEmail.Should().Be(email);
            }
        }
    }
}
