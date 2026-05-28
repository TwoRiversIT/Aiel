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

public abstract class EmailValidatorTestBase
{
    protected abstract IEmailValidator Validator { get; }

    [Theory]
    [InlineData("test@example.org")]
    [InlineData("prettyandsimple@example.org")]
    [InlineData("very.common@example.org")]
    [InlineData("disposable.style.email.with+symbol@example.org")]
    [InlineData("other.email-with-dash@example.org")]
    [InlineData("x@example.org")]
    [InlineData("\"much.more unusual\"@example.org")]
    [InlineData("\"very.unusual.@.unusual.com\"@example.org")]
    [InlineData("\"very.(),:;<>[]\\\".VERY.\\\"very@\\ \\\"very\\\".unusual\"@strange.example.org")]
    [InlineData("example-indeed@strange-example.org")]
    [InlineData("\"()<>[]:,;@\\\"!#$%&'-/\\=?^_`{}| ~.a\"@example.org")]
    [InlineData("\" \"@example.org")]
    [InlineData("\" \".\" \"@example.org")]
    [InlineData("example@s.solutions")]
    [InlineData("~`!#$%^&*'-_=+/?{|}@gmail.com")] // WTF!?
    [InlineData("user@[IPv6:2001:db8::1]")]
    [InlineData("user@[IPv6:2001:db8:1ff::a0b:dbd0]")]
    [InlineData("user@192.168.1.1")]
    public void Valid(String email)
    {
        Validator.IsValid(email).Should().BeTrue();
    }

    [Theory]
    [InlineData("John\\ Smith@example.org")] // This one could go either way. The space is escaped, but it is not in a quoted string.
    [InlineData("Abc\\@def@example.org")] // Contains escaped special in the local part but is not quoted.
    [InlineData("A@b@c@example.org")] // Contains unescaped special in the local part but is not quoted.
    [InlineData("Abc.example.org")] // Missing @
    [InlineData("a\"b(c)d,e:f;gi[j\\k]l@example.org")] // None of the special characters in this local part are allowed outside quotation marks
    [InlineData("example@localhost")] // Invalid unless allowLocal == true
    [InlineData("hannah.b.mffs@@gmail.com")] // Oops, double @
    [InlineData("just\"not\"right@example.org")] // quoted strings must be dot separated or the only element making up the local part
    [InlineData("this\"is not\\allowed@example.org")] // spaces, quotes, and backslashes may only exist when within quoted strings and preceded by a backslash
    [InlineData("this\\ still\\\"not\\allowed@example.org")] // even if escaped spaces, quotes, and backslashes must still be contained by quotes
    [InlineData("user@[192.168.1.1]")]
    [InlineData("user@[IPv6:2001:db8:::1]")] // Too many consecutive colons
    [InlineData("user@[IPv6:2001:db8:1ff::a0b:dbd0")] // Missing trailing ] literal.
    public void Invalid(String email)
    {
        Validator.IsValid(email).Should().BeFalse();
    }

    [Fact]
    public void Consecutive_dots_in_domain_should_return_False()
    {
        Validator.IsValid("test@example..com").Should().BeFalse();
    }

    [Fact]
    public void Consecutive_dots_in_local_part_should_return_False()
    {
        // caveat: Gmail lets this next one through
        Validator.IsValid("test..user@example.org").Should().BeFalse();
    }

    [Fact]
    public void Empty_Email_should_return_False()
    {
        Validator.IsValid(String.Empty).Should().BeFalse();
    }

    [Fact]
    public void Invalid_LocalPart_Valid_Domain_Email_should_return_False()
    {
        Validator.IsValid("test@.com").Should().BeFalse();
    }

    [Fact]
    public void Local_part_ending_with_a_dot_should_return_False()
    {
        Validator.IsValid("test.@example.org").Should().BeFalse();
    }

    [Fact]
    public void Local_part_starting_with_a_dot_should_return_False()
    {
        Validator.IsValid(".test@example..com").Should().BeFalse();
    }

    [Fact]
    public void Local_part_with_plus_addressing_should_return_True()
    {
        Validator.IsValid("john+doe@gmail.com").Should().BeTrue();
    }

    [Fact]
    public void Local_part_quoted_with_escaped_special_char_should_return_True()
    {
        Validator.IsValid("\"a\\@b\"@example.org").Should().BeTrue();
    }

    [Fact]
    public void Non_Email_should_return_False()
    {
        Validator.IsValid("invalid_email").Should().BeFalse();
    }

    [Fact]
    public void Null_Email_should_return_False()
    {
        Validator.IsValid((String)null!).Should().BeFalse();
    }

    [Fact]
    public void Quoted_special_characters_should_return_True()
    {
        Validator.IsValid("\"test!\"@example.org").Should().BeTrue();
    }

    [Fact]
    public void Unquoted_special_characters_should_return_False()
    {
        Validator.IsValid("te,st@example.org").Should().BeFalse();
    }

    [Fact]
    public void Valid_LocalPart_Invalid_Domain_Email_should_return_False()
    {
        Validator.IsValid("test@example!").Should().BeFalse();
    }

    [Fact]
    public void Valid_LocalPart_Valid_Domain_Email_should_return_True()
    {
        Validator.IsValid("john.doe@gmail.com").Should().BeTrue();
    }

    [Fact]
    public void WhiteSpace_Email_should_return_False()
    {
        Validator.IsValid("  ").Should().BeFalse();
    }
}
