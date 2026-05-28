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

using System.Text;

namespace Aiel.Gps.HP;

[Trait("Category", "Unit")]
[Trait("Category", "Lexer")]
public class LexerTests
{
    [Fact]
    public void PeekIdentifier_ReturnsIdentifier_FromStandardSentence()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        var lexer = new Lexer(sentence);

        // Act
        var identifier = lexer.PeekIdentifier();

        // Assert
        Encoding.UTF8.GetString(identifier).Should().Be("GPGLL");
    }

    [Fact]
    public void PeekIdentifier_ReturnsIdentifier_FromCustomSentence()
    {
        // Arrange
        var sentence = "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8;
        var lexer = new Lexer(sentence);

        // Act
        var identifier = lexer.PeekIdentifier();

        // Assert
        Encoding.UTF8.GetString(identifier).Should().Be("GFDTA");
    }

    [Fact]
    public void PeekIdentifier_DoesNotAdvanceLexerPosition()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        var lexer = new Lexer(sentence);

        // Act - call PeekIdentifier twice
        var first = lexer.PeekIdentifier();
        var second = lexer.PeekIdentifier();

        // Assert - should get the same result
        Encoding.UTF8.GetString(first).Should().Be(Encoding.UTF8.GetString(second));
    }

    [Fact]
    public void ConsumeString_AdvancesPastFirstField()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        var lexer = new Lexer(sentence);

        // Act
        lexer.ConsumeString(); // Skip identifier

        // Assert - next field should be latitude
        var latitude = lexer.NextDouble();
        latitude.Should().BeApproximately(4916.45, 0.001);
    }

    [Fact]
    public void NextDouble_ParsesValidDouble()
    {
        // Arrange - sentence with just a double value
        var sentence = "$TEST,123.456,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextDouble();

        // Assert
        value.Should().BeApproximately(123.456, 0.0001);
    }

    [Fact]
    public void NextDouble_ReturnsNaN_ForEmptyField()
    {
        // Arrange - sentence with empty field
        var sentence = "$TEST,,next,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextDouble();

        // Assert
        value.Should().Be(Double.NaN);
    }

    [Fact]
    public void NextInteger_ParsesValidInteger()
    {
        // Arrange
        var sentence = "$TEST,42,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextInteger();

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void NextInteger_ReturnsZero_ForEmptyField()
    {
        // Arrange
        var sentence = "$TEST,,next,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextInteger();

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void NextChar_ParsesValidChar()
    {
        // Arrange
        var sentence = "$TEST,A,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextChar();

        // Assert
        value.Should().Be('A');
    }

    [Fact]
    public void NextChar_ReturnsMinValue_ForEmptyField()
    {
        // Arrange
        var sentence = "$TEST,,next,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextChar();

        // Assert
        value.Should().Be(Char.MinValue);
    }

    [Fact]
    public void NextString_ParsesValidString()
    {
        // Arrange
        var sentence = "$TEST,Hello,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextString();

        // Assert
        value.Should().Be("Hello");
    }

    [Fact]
    public void NextString_ReturnsEmpty_ForEmptyField()
    {
        // Arrange
        var sentence = "$TEST,,next,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextString();

        // Assert
        value.Should().BeEmpty();
    }

    [Fact]
    public void NextTime_ParsesHHMMSS_Format()
    {
        // Arrange
        var sentence = "$TEST,225444,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextTime();

        // Assert
        value.Should().Be(new TimeOnly(22, 54, 44));
    }

    [Fact]
    public void NextTime_ParsesHHMMSS_WithMilliseconds()
    {
        // Arrange
        var sentence = "$TEST,225444.123,*00\r\n"u8;
        var lexer = new Lexer(sentence);
        lexer.ConsumeString(); // Skip identifier

        // Act
        var value = lexer.NextTime();

        // Assert
        value.Should().Be(new TimeOnly(22, 54, 44, 123));
    }
}
