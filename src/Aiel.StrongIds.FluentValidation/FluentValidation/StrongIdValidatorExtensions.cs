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

using Aiel.StrongIds;
using FluentValidation.Validators;

namespace FluentValidation;

/// <summary>
/// Defines a FluentValidation validator for <see cref="IStrongId"/> properties. Validation will fail if the value is not a valid email.
/// </summary>
public class NotDefaultStrongIdPropertyValidator<T, TStrongId>()
    : PropertyValidator<T, TStrongId>, IPropertyValidator<T, TStrongId>
    where TStrongId : IStrongId
{
    /// <inheritdoc/>
    public override String Name => nameof(NotDefaultStrongIdPropertyValidator<,>);

    /// <inheritdoc/>
    public override Boolean IsValid(ValidationContext<T> context, TStrongId value) => value.HasValue;
}

/// <summary>
/// Defines extension methods for FluentValidation rule builders to add <see cref="NotDefault"/> validation.
/// </summary>
public static partial class StrongIdValidatorExtensions
{
    /// <summary>
    /// Defines a validator on the current rule builder for <see cref="IStrongId"/> properties. Validation will fail if <see cref="IStrongId.HasValue"/> is false.
    /// </summary>
    public static IRuleBuilderOptions<T, TStrongId> NotDefault<T, TStrongId>(this IRuleBuilder<T, TStrongId> ruleBuilder)
        where TStrongId : IStrongId
        => ruleBuilder.SetValidator(new NotDefaultStrongIdPropertyValidator<T, TStrongId>());
}
