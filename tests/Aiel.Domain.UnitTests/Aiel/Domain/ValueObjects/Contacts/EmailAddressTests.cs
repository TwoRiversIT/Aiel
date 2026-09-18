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

namespace Aiel.Domain.ValueObjects.Contacts;

public class EmailAddressTests
{
    [Fact]
    public void EmailAddress_Explicit_Conversion_From_String_Returns_EmailAddress_When_String_Is_Valid()
    {
        // Arrange
        const String expectedName = "Jane Doe";
        const String expectedEmail = "jane@example.org";
        var stringValue = $"{expectedName} <{expectedEmail}>";

        // Act
        EmailAddress emailAddress = (EmailAddress)stringValue;

        // Assert
        emailAddress.Name.Should().Be(expectedName);
        emailAddress.Email.ToString().Should().Be(expectedEmail);
    }

    [Fact]
    public void EmailAddress_Explicit_Conversion_From_String_Throws_FormatException_When_String_Is_Invalid()
    {
        // Arrange
        const String invalidStringValue = "Invalid String";

        // Act
        Action act = () => { var emailAddress = (EmailAddress)invalidStringValue; };

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void EmailAddress_Implicit_Conversion_To_String_Returns_Formatted_Name_and_Email()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        var expected = $"{name} <{email}>";

        // Act
        String result = EmailAddress.From(name, new(email));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EmailAddress_ToString_Returns_Formatted_String()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        var expected = $"{name} <{email}>";

        // Act
        var result = EmailAddress.From(name, email).ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EmailAddress_Must_Be_Equal_To_EmailAddress_With_Same_Name_And_Email()
    {
        // Arrange
        var emailAddress1 = EmailAddress.From("Jane Doe", new("jane@example.org"));
        var emailAddress2 = EmailAddress.From("Jane Doe", new("jane@example.org"));

        // Act & Assert
        emailAddress1.Should().Be(emailAddress2);
    }

    [Fact]
    public void EmailAddress_Must_Be_Equal_To_String_With_Same_Name_And_Email()
    {
        // Arrange
        var emailAddress = EmailAddress.From("Jane Doe", "jane@example.org");

        // Act & Assert
        emailAddress.Equals("Jane Doe <jane@example.org>").Should().BeTrue();
    }

    [Fact]
    public void From_Returns_EmailAddress_When_Name_And_Email_Are_Provided()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";

        // Act
        var emailAddress = EmailAddress.From(name, Email.From(email));

        // Assert
        emailAddress.Name.Should().Be(name);
        emailAddress.Email.ToString().Should().Be(email);
    }

    [Fact]
    public void From_Returns_EmailAddress_When_Name_And_Email_String_Are_Provided()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";

        // Act
        var emailAddress = EmailAddress.From(name, email);

        // Assert
        emailAddress.Name.Should().Be(name);
        emailAddress.Email.ToString().Should().Be(email);
    }

    [Fact]
    public void From_Returns_Empty_When_Empty_String_Is_Provided()
    {
        // Act
        var emailAddress = EmailAddress.From(String.Empty);

        // Assert
        emailAddress.Name.Should().Be(String.Empty);
        emailAddress.Email.ToString().Should().Be(String.Empty);
    }

    [Fact]
    public void From_Returns_Empty_When_Only_Email_Is_Provided()
    {
        // Arrange
        const String email = "jane@example.org";

        // Act
        var emailAddress = EmailAddress.From(email);

        // Assert
        emailAddress.Name.Should().BeEmpty();
        emailAddress.Email.ToString().Should().BeEmpty();
        emailAddress.ToString().Should().BeEmpty();
    }

    [Fact]
    public void From_Returns_Empty_When_Only_Name_Is_Provided()
    {
        // Arrange
        const String name = "Jane Doe";

        // Act
        var emailAddress = EmailAddress.From(name);

        // Assert
        emailAddress.Name.Should().BeEmpty();
        emailAddress.Email.ToString().Should().BeEmpty();
        emailAddress.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Parse_Returns_EmailAddress_When_Email_Is_In_Angle_Brackets()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        var input = $"{name} <{email}>";

        // Act
        var emailAddress = EmailAddress.Parse(input);

        // Assert
        emailAddress.Name.Should().Be(name);
        emailAddress.Email.ToString().Should().Be(email);
    }

    [Fact]
    public void Parse_Returns_EmailAddress_When_Name_Is_In_Angle_Brackets()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        var input = $"<{name}> {email}";

        // Act
        var emailAddress = EmailAddress.Parse(input);

        // Assert
        emailAddress.Name.Should().Be(name);
        emailAddress.Email.ToString().Should().Be(email);
    }

    [Fact]
    public void TryParse_Returns_True_And_EmailAddress_When_Email_Is_In_Angle_Brackets()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        var input = $"<{name}> {email}";

        var got = EmailAddress.TryParse(input, out var emailAddress);

        got.Should().BeTrue();
        emailAddress.Name.Should().Be(name);
        emailAddress.Email.ToString().Should().Be(email);
    }
}
