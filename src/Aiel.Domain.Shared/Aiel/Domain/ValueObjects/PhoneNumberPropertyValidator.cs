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

namespace Aiel.Domain.ValueObjects;

/// <summary>
/// Defines a FluentValidation validator for <see cref="PhoneNumber"/> properties. Validation will fail if the value is not a valid NANP phone number.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PhoneNumberPropertyValidator<T>()
    : PropertyValidator<T, PhoneNumber>
{
    /// <inheritdoc/>
    public override String Name => "PhoneNumberValidator";

    /// <inheritdoc/>
    public override Boolean IsValid(ValidationContext<T> context, PhoneNumber value)
        => PhoneNumber.TryParse(value.Dashes, out _);
}

/// <summary>
/// Defines a FluentValidation validator for nullable <see cref="PhoneNumber"/> properties. Validation will fail if the value is not null and not a valid NANP phone number.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NullablePhoneNumberPropertyValidator<T>
    : PropertyValidator<T, PhoneNumber?>
{
    /// <inheritdoc/>
    public override String Name => "NullablePhoneNumberValidator";

    /// <inheritdoc/>
    protected override String GetDefaultMessageTemplate(String errorCode) => "Required";

    /// <inheritdoc/>
    public override Boolean IsValid(ValidationContext<T> context, PhoneNumber? value)
        // Null is not invalid
        => value is null || PhoneNumber.TryParse(value.Value.Dashes, out _);
}

/// <summary>
/// Defines a FluentValidation validator for <see cref="PhoneNumber"/> properties where a value is required.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NotEmptyPhoneNumberPropertyValidator<T>
    : PropertyValidator<T, PhoneNumber>
{
    /// <inheritdoc/>
    public override String Name => "NotEmptyPhoneNumberValidator";

    /// <inheritdoc/>
    protected override String GetDefaultMessageTemplate(String errorCode) => "Required";

    /// <inheritdoc/>
    public override Boolean IsValid(ValidationContext<T> context, PhoneNumber value)
        => value != PhoneNumber.Empty;
}

/// <summary>
/// Defines extension methods for FluentValidation rule builders to add <see cref="ValueObjects.PhoneNumber"/> validation.
/// </summary>
public static partial class PhoneNumberValidatorExtensions
{
    /// <summary>
    /// Defines a validator on the current rule builder for <see cref="ValueObjects.PhoneNumber"/> properties. Validation will fail if the value is not a valid NANP phone number.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<TRequest, PhoneNumber> PhoneNumber<TRequest>(this IRuleBuilder<TRequest, PhoneNumber> ruleBuilder)
        => ruleBuilder.SetValidator(new PhoneNumberPropertyValidator<TRequest>());

    /// <summary>
    /// Defines a validator on the current rule builder for nullable <see cref="ValueObjects.PhoneNumber"/> properties. Validation will fail if the value is not null and not a valid NANP phone number.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<TRequest, PhoneNumber?> PhoneNumber<TRequest>(this IRuleBuilder<TRequest, PhoneNumber?> ruleBuilder)
        => ruleBuilder.SetValidator(new NullablePhoneNumberPropertyValidator<TRequest>());

    /// <summary>
    /// Defines a validator on the current rule builder for <see cref="ValueObjects.PhoneNumber"/> properties. Validation will fail if the value is <see cref="ValueObjects.PhoneNumber.Empty"/>.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<TRequest, PhoneNumber> NotEmpty<TRequest>(this IRuleBuilder<TRequest, PhoneNumber> ruleBuilder)
        => ruleBuilder.SetValidator(new NotEmptyPhoneNumberPropertyValidator<TRequest>());
}
