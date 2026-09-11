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

using Aiel.StrongIds.Validators;
using Aiel.Testing.StrongIds;
using FluentValidation;

namespace Aiel.StrongIds;

public class StrongIdValidationTests
{
    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsDefault_Guid()
    {
        var validator = new StrongIdPropertyValidator<Object, GuidAllowDefaultFalseId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, default).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNotDefault_Guid()
    {
        var validator = new StrongIdPropertyValidator<Object, GuidAllowDefaultFalseId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, new GuidAllowDefaultFalseId(Guid.NewGuid())).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsDefault_Int32()
    {
        var validator = new StrongIdPropertyValidator<Object, Int32AllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, default).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNotDefault_Int32()
    {
        var validator = new StrongIdPropertyValidator<Object, Int32AllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, new Int32AllowDefaultTrueId(1)).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsDefault_String()
    {
        var validator = new StrongIdPropertyValidator<Object, StringAllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, StringAllowDefaultTrueId.Empty).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNotDefault_String()
    {
        var validator = new StrongIdPropertyValidator<Object, StringAllowDefaultTrueId>();
        var context = new ValidationContext<Object>(new Object());

        validator.IsValid(context, new StringAllowDefaultTrueId("test")).Should().BeTrue();
    }

    [Fact]
    public void NotDefault_AddsValidationFailure_WhenValueIsDefault()
    {
        var validator = new StrongIdRequestValidator();

        var result = validator.Validate(new StrongIdRequest(default));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(StrongIdRequest.StrongId));
    }

    [Fact]
    public void NotDefault_AllowsNonDefaultValue()
    {
        var validator = new StrongIdRequestValidator();

        var result = validator.Validate(new StrongIdRequest(new GuidAllowDefaultFalseId(Guid.NewGuid())));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private sealed class StrongIdRequestValidator : AbstractValidator<StrongIdRequest>
    {
        public StrongIdRequestValidator() => RuleFor(x => x.StrongId).NotDefault();
    }

    private sealed record StrongIdRequest(GuidAllowDefaultFalseId StrongId);
}
