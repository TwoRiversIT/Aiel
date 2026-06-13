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

using Aiel.Emailing.Abstractions.Aiel.Emailing;
using Microsoft.Extensions.Options;

namespace Aiel.Emailing;

public class ParsingEmailValidatorTests : EmailValidatorTestBase
{
    public ParsingEmailValidatorTests()
    {
        var options = Options.Create(new ParsingEmailValidatorOptions());
        Validator = new ParsingEmailValidator(options);
    }

    protected override IEmailValidator Validator { get; }

    [Theory]
    [InlineData("admin@mailserver1")]
    [InlineData("example@localhost")]
    [InlineData("user@com")]
    [InlineData("user@localserver")]
    public void Should_Validate_Email_With_Top_Level_Domains(String email)
    {
        var validator = new ParsingEmailValidator(Options.Create(new ParsingEmailValidatorOptions
        {
            AllowTopLevelDomains = true
        }));

        validator.IsValid(email).Should().BeTrue();
    }
}
