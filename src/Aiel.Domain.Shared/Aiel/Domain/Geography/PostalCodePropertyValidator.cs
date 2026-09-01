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

using FluentValidation;
using FluentValidation.Validators;

namespace Aiel.Domain.Geography;

public class PostalCodePropertyValidator<T>()
    : PropertyValidator<T, PostalCode>
{
    public override String Name => "PostalCodeValidator";

    public override Boolean IsValid(ValidationContext<T> context, PostalCode value)
        => value.IsValidPostalCode();
}

public class NullablePostalCodePropertyValidator<T>
    : PropertyValidator<T, PostalCode?>
{
    public override String Name => "NullablePostalCodeValidator";

    protected override String GetDefaultMessageTemplate(String errorCode) => "Required";

    public override Boolean IsValid(ValidationContext<T> context, PostalCode? value)
        // Null is not invalid
        => value is null || value.IsValidPostalCode();
}

public static partial class PostalCodeValidatorExtensions
{
    /// <summary>
    /// Defines a validator on the current rule builder for <see cref="PostalCode?"/> properties. Validation will fail if the value is not null and not a valid Member Number.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<TRequest, PostalCode?> PostalCode<TRequest>(this IRuleBuilder<TRequest, PostalCode?> ruleBuilder)
        => ruleBuilder.SetValidator(new NullablePostalCodePropertyValidator<TRequest>());
}

public class CanadianPostalCodePropertyValidator<T> : PropertyValidator<T, String>
{
    public override String Name => "Canadian Postal Code Validator";

    protected override String GetDefaultMessageTemplate(String errorCode)
        => "Please enter a valid Canadian Postal Code in the format of 'H0H 0H0'.";

    public override Boolean IsValid(ValidationContext<T> context, String value)
        // Null is not invalid
        => value?.IsValidPostalCode() != false;
}

public static class CanadianPostalCodePropertyValidatorExtensions
{
    /// <summary>
    /// Defines a validator on the current rule builder for <see cref="String"/> properties. Validation will fail if the string is not a valid Canadian Postal Code.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<TRequest, String> CanadianPostalCode<TRequest>(this IRuleBuilder<TRequest, String> ruleBuilder)
        => ruleBuilder.SetValidator(new CanadianPostalCodePropertyValidator<TRequest>());
}
