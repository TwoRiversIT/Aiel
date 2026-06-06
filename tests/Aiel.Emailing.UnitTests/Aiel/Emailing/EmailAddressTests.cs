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

namespace Aiel.Emailing;

public class EmailAddressTests
{
    [Fact]
    public void EmailAddress_can_be_created_with_name_and_email()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";

        // Act
        var emailAddress = new EmailAddress(name, email);

        // Assert
        Assert.Equal(name, emailAddress.Name);
        Assert.Equal(email, emailAddress.Email);
    }

    [Fact]
    public void EmailAddress_to_string_returns_correct_format_with_name()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        const String expected = "Jane Doe <jane@example.org>";
        var emailAddress = new EmailAddress(name, email);

        // Act
        var result = emailAddress.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EmailAddress_to_string_returns_correct_format_without_name()
    {
        // Arrange
        const String email = "jane@example.org";
        var emailAddress = new EmailAddress(String.Empty, email);

        // Act
        var result = emailAddress.ToString();

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void EmailAddress_parse_returns_correct_email_address_when_email_is_in_angle_brackets()
    {
        // Arrange
        const String emailAddressString = "Jane Doe <jane@example.org>";
        const String expectedName = "Jane Doe";
        const String expectedEmail = "jane@example.org";

        // Act
        var emailAddress = EmailAddress.Parse(emailAddressString);

        // Assert
        Assert.Equal(expectedName, emailAddress.Name);
        Assert.Equal(expectedEmail, emailAddress.Email);
    }

    [Fact]
    public void EmailAddress_parse_returns_correct_email_address_when_name_is_in_angle_brackets()
    {
        // Arrange
        const String emailAddressString = "<Jane Doe> jane@example.org";
        const String expectedName = "Jane Doe";
        const String expectedEmail = "jane@example.org";

        // Act
        var emailAddress = EmailAddress.Parse(emailAddressString);

        // Assert
        Assert.Equal(expectedName, emailAddress.Name);
        Assert.Equal(expectedEmail, emailAddress.Email);
    }

    [Fact]
    public void EmailAddress_implicit_conversion_to_string_returns_correct_format_with_name()
    {
        // Arrange
        const String name = "Jane Doe";
        const String email = "jane@example.org";
        var expected = $"{name} <{email}>";

        // Act
        String result = new EmailAddress(name, email);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EmailAddress_implicit_conversion_to_string_returns_correct_format_without_name()
    {
        // Arrange
        const String email = "jane@example.org";

        // Act
        String result = new EmailAddress(String.Empty, email);

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void EmailAddress_implicit_conversion_from_string_returns_correct_email_address()
    {
        // Arrange
        const String emailAddressString = "Jane Doe <jane@example.org>";
        const String expectedName = "Jane Doe";
        const String expectedEmail = "jane@example.org";

        // Act
        EmailAddress emailAddress = emailAddressString;

        // Assert
        Assert.Equal(expectedName, emailAddress.Name);
        Assert.Equal(expectedEmail, emailAddress.Email);
    }
}
