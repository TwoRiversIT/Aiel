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

using Aiel.Actions;
using Aiel.Results;
using FluentValidation;

namespace Aiel.Mediator.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task HandleAsync_when_no_validators_are_registered_calls_next()
    {
        var behavior = new ValidationBehavior<TestAction>([]);
        var nextCalled = false;

        var result = await behavior.HandleAsync(
            new TestAction(String.Empty),
            () =>
            {
                nextCalled = true;
                return ValueTask.FromResult(Result.Success());
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_when_all_validators_pass_calls_next()
    {
        var validators = new IValidator<TestAction>[]
        {
            new PassingValidator(),
            new AlsoPassingValidator()
        };

        var behavior = new ValidationBehavior<TestAction>(validators);
        var nextCalled = false;

        var result = await behavior.HandleAsync(
            new TestAction("valid"),
            () =>
            {
                nextCalled = true;
                return ValueTask.FromResult(Result.Success());
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_when_validation_fails_returns_validation_error_and_skips_next()
    {
        var validators = new IValidator<TestAction>[]
        {
            new EmptyNameValidator(),
            new ReservedNameValidator()
        };

        var behavior = new ValidationBehavior<TestAction>(validators);
        var nextCalled = false;

        var result = await behavior.HandleAsync(
            new TestAction(String.Empty),
            () =>
            {
                nextCalled = true;
                return ValueTask.FromResult(Result.Success());
            },
            CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        var error = result.Error.Should().BeOfType<ValidationError>().Subject;
        error.Message.Should().Be("Validation failed.");
        error.Failures.Select(failure => failure.ErrorMessage)
            .Should().Contain("Name is required.");
    }

    [Fact]
    public async Task HandleAsync_aggregates_failures_from_multiple_validators()
    {
        var validators = new IValidator<TestAction>[]
        {
            new ReservedNameValidator(),
            new DuplicateReservedNameValidator()
        };

        var behavior = new ValidationBehavior<TestAction>(validators);

        var result = await behavior.HandleAsync(
            new TestAction("reserved"),
            () => ValueTask.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        var error = result.Error.Should().BeOfType<ValidationError>().Subject;
        error.Failures.Select(failure => failure.ErrorMessage).Should().Contain(
            "Name is reserved.",
            "Reserved names require an override.");
    }

    private sealed record TestAction(String Name) : IAction;

    private sealed class PassingValidator : AbstractValidator<TestAction>;

    private sealed class AlsoPassingValidator : AbstractValidator<TestAction>;

    private sealed class EmptyNameValidator : AbstractValidator<TestAction>
    {
        public EmptyNameValidator()
        {
            RuleFor(action => action.Name).NotEmpty().WithMessage("Name is required.");
        }
    }

    private sealed class ReservedNameValidator : AbstractValidator<TestAction>
    {
        public ReservedNameValidator()
        {
            RuleFor(action => action.Name)
                .NotEqual("reserved")
                .WithMessage("Name is reserved.");
        }
    }

    private sealed class DuplicateReservedNameValidator : AbstractValidator<TestAction>
    {
        public DuplicateReservedNameValidator()
        {
            RuleFor(action => action.Name)
                .Must(name => !String.Equals(name, "reserved", StringComparison.Ordinal))
                .WithMessage("Reserved names require an override.");
        }
    }
}
