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
using FluentValidation;
using FluentValidation.Validators;

namespace Aiel.Emailing;

public class EmailPropertyValidator<T>()
    : PropertyValidator<T, Email>
{
    public override String Name => "EmailValidator";

    public override Boolean IsValid(ValidationContext<T> context, Email value)
        => EmailValidator.IsValid(value);
}

public class NullableEmailPropertyValidator<T>
    : PropertyValidator<T, Email?>
{
    public override String Name => "NullableEmailValidator";

    protected override String GetDefaultMessageTemplate(String errorCode) => "Required";

    public override Boolean IsValid(ValidationContext<T> context, Email? value)
        // Null is not invalid
        => value is null || EmailValidator.IsValid(value);
}

public static partial class EmailValidatorExtensions
{
    /// <summary>
    /// Defines a validator on the current rule builder for <see cref="Email?"/> properties. Validation will fail if the value is not null and not a valid Member Number.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<TRequest, Email?> Email<TRequest>(this IRuleBuilder<TRequest, Email?> ruleBuilder)
        => ruleBuilder.SetValidator(new NullableEmailPropertyValidator<TRequest>());
}
