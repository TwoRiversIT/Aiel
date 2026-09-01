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

namespace Aiel.Domain.Contacts;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("234-567-8901")]
    [InlineData("(234) 567-8901")]
    [InlineData("+1 234 567 8901")]
    [InlineData("1.234.567.8901")]
    [InlineData("2345678901")]
    public void TryParse_ValidNumberFormats_ShouldReturnTrue(String input)
    {
        // Act
        var parsed = PhoneNumber.TryParse(input, out var number);

        // Assert
        parsed.Should().BeTrue();
        number.Should().NotBeNull();
        number.AreaCode.Should().Be("234");
        number.Exchange.Should().Be("567");
        number.SubscriberNumber.Should().Be("8901");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void TryParse_NullOrWhitespace_ShouldReturnFalse(String? input)
    {
        // Act
        var parsed = PhoneNumber.TryParse(input, out var number);

        // Assert
        parsed.Should().BeFalse();
        number.Should().BeNull();
    }

    [Theory]
    [InlineData("123-456-7890")]
    [InlineData("234-056-7890")]
    [InlineData("234-567-890")]
    [InlineData("234-567-89012")]
    [InlineData("abc-def-ghij")]
    public void TryParse_InvalidNumbers_ShouldReturnFalse(String input)
    {
        // Act
        var parsed = PhoneNumber.TryParse(input, out var number);

        // Assert
        parsed.Should().BeFalse();
        number.Should().BeNull();
    }

    [Fact]
    public void Parse_ValidNumber_ShouldPopulateAllFormats()
    {
        // Act
        var phoneNumber = PhoneNumber.Parse("234-567-8901");

        // Assert
        phoneNumber.AreaCode.Should().Be("234");
        phoneNumber.Exchange.Should().Be("567");
        phoneNumber.SubscriberNumber.Should().Be("8901");
        phoneNumber.Digits.Should().Be("2345678901");
        phoneNumber.Hyphenated.Should().Be("234-567-8901");
        phoneNumber.Dashes.Should().Be("234-567-8901");
        phoneNumber.National.Should().Be("(234) 567-8901");
        phoneNumber.E164.Should().Be("+12345678901");
        phoneNumber.RFC3966.Should().Be("tel:+1-234-567-8901");
    }

    [Fact]
    public void Parse_InvalidNumber_ShouldThrowFormatException()
    {
        // Act
        var action = () => PhoneNumber.Parse("not-a-number");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*not-a-number*");
    }

    [Fact]
    public void ToString_WithoutFormat_ShouldReturnHyphenated()
    {
        // Arrange
        var number = PhoneNumber.Parse("234-567-8901");

        // Act
        var formatted = number.ToString();

        // Assert
        formatted.Should().Be("234-567-8901");
    }

    [Theory]
    [InlineData("H", "234-567-8901")]
    [InlineData("N", "(234) 567-8901")]
    [InlineData("D", "234-567-8901")]
    [InlineData("E", "+12345678901")]
    [InlineData("R", "tel:+1-234-567-8901")]
    [InlineData("G", "2345678901")]
    public void ToString_WithKnownFormat_ShouldReturnExpected(String format, String expected)
    {
        // Arrange
        var number = PhoneNumber.Parse("234-567-8901");

        // Act
        var formatted = number.ToString(format);

        // Assert
        formatted.Should().Be(expected);
    }

    [Fact]
    public void ToString_UnknownFormat_ShouldThrowFormatException()
    {
        // Arrange
        var number = PhoneNumber.Parse("234-567-8901");

        // Act
        var action = () => number.ToString("X");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*X*");
    }

    [Fact]
    public void Equality_SameDigitsDifferentInputFormats_ShouldBeEqual()
    {
        // Arrange
        var left = PhoneNumber.Parse("234-567-8901");
        var right = PhoneNumber.Parse("(234) 567-8901");

        // Act
        var areEqual = left.Equals(right);

        // Assert
        areEqual.Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentDigits_ShouldNotBeEqual()
    {
        // Arrange
        var left = PhoneNumber.Parse("234-567-8901");
        var right = PhoneNumber.Parse("234-567-8902");

        // Assert
        left.Equals(right).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void CompareTo_Null_ShouldReturnOne()
    {
        // Arrange
        var number = PhoneNumber.Parse("234-567-8901");

        // Act
        var comparison = number.CompareTo(null);

        // Assert
        comparison.Should().Be(1);
    }

    [Fact]
    public void ComparisonOperators_ShouldOrderByDigits()
    {
        // Arrange
        var smaller = PhoneNumber.Parse("234-567-8901");
        var larger = PhoneNumber.Parse("234-567-8902");

        // Assert
        (smaller < larger).Should().BeTrue();
        (smaller <= larger).Should().BeTrue();
        (larger > smaller).Should().BeTrue();
        (larger >= smaller).Should().BeTrue();
        smaller.CompareTo(larger).Should().BeNegative();
        larger.CompareTo(smaller).Should().BePositive();
    }

    [Fact]
    public void Empty_ShouldExposeDefaultValues()
    {
        // Arrange
        var empty = PhoneNumber.Empty;

        // Assert
        empty.AreaCode.Should().BeEmpty();
        empty.Exchange.Should().BeEmpty();
        empty.SubscriberNumber.Should().BeEmpty();
        empty.Digits.Should().BeEmpty();
        empty.E164.Should().Be("+1");
        empty.Hyphenated.Should().Be("--");
        empty.National.Should().Be("() -");
        empty.RFC3966.Should().Be("tel:+1---");
    }
}
