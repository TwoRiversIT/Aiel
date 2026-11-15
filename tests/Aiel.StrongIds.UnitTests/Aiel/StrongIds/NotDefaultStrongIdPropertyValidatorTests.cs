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

namespace Aiel.StrongIds;

public class NotDefaultStrongIdPropertyValidatorTests
{
    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIs_DefaultGuid()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, GuidAllowDefaultFalseId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, default).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNot_EmptyGuid()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, GuidAllowDefaultFalseId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, new GuidAllowDefaultFalseId(Guid.NewGuid())).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIs_EmptyGuid()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, GuidAllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, default).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIs_DefaultInt32()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, Int32AllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, default).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNot_DefaultInt32()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, Int32AllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, new Int32AllowDefaultTrueId(1)).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIs_EmptyString()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, StringAllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, StringAllowDefaultTrueId.Empty).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNot_EmptyString()
    {
        var validator = new NotDefaultStrongIdPropertyValidator<Object, StringAllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, new StringAllowDefaultTrueId("test")).Should().BeTrue();
    }

    [Fact]
    public void NotDefault_AddsValidationFailure_WhenValueIs_DefaultGuid()
    {
        var validator = new GuidStrongIdRequestValidator();

        var result = validator.Validate(new GuidStrongIdRequest(default));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(GuidStrongIdRequest.StrongId));
    }

    [Fact]
    public void NotDefault_IsValid_ReturnsTrue_WhenValueIsNot_DefaultGuid()
    {
        var validator = new GuidStrongIdRequestValidator();

        var result = validator.Validate(new GuidStrongIdRequest(new GuidAllowDefaultTrueId(Guid.NewGuid())));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void NotDefault_IsValid_ReturnsFalse_WhenValueIs_NullString()
    {
        var validator = new StringStrongIdRequestValidator();

        var result = validator.Validate(new StringStrongIdRequest(new StringAllowDefaultTrueId(null!)));

        result.IsValid.Should().BeFalse("null strings are converted to String.Empty by the StrongId implementation");
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(StringStrongIdRequest.StrongId));
    }

    [Fact]
    public void NotDefault_IsValid_ReturnsTrue_WhenValueIsNot_EmptyString()
    {
        var validator = new StringStrongIdRequestValidator();

        var result = validator.Validate(new StringStrongIdRequest(new StringAllowDefaultTrueId("test")));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void NotNull_IsValid_ReturnsFalse_and_AddsValidationFailure_WhenValueIs_Null()
    {
        var validator = new NullableStrongIdRequestValidator();

        var result = validator.Validate(new NullableStrongIdRequest(null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(NullableStrongIdRequest.StrongId));
    }

    private sealed record GuidStrongIdRequest(GuidAllowDefaultTrueId StrongId);

    private sealed class GuidStrongIdRequestValidator : AbstractValidator<GuidStrongIdRequest>
    {
        public GuidStrongIdRequestValidator() => RuleFor(x => x.StrongId).NotDefault();

    }

    private sealed record StringStrongIdRequest(StringAllowDefaultTrueId StrongId);

    private sealed class StringStrongIdRequestValidator : AbstractValidator<StringStrongIdRequest>
    {
        public StringStrongIdRequestValidator() => RuleFor(x => x.StrongId).NotDefault();
    }

    private sealed record NullableStrongIdRequest(StringAllowDefaultTrueId? StrongId);

    private sealed class NullableStrongIdRequestValidator : AbstractValidator<NullableStrongIdRequest>
    {
        public NullableStrongIdRequestValidator() => RuleFor(x => x.StrongId).NotNull();
    }
}
