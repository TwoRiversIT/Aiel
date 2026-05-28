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
using FluentValidation.Results;
using Aiel.Actions;
using Aiel.Results;

namespace Aiel.Mediator.Behaviors;

/// <summary>
/// Runs FluentValidation validators for the current action before invoking the next pipeline step.
/// </summary>
/// <typeparam name="TAction">The action type flowing through the pipeline.</typeparam>
public sealed class ValidationBehavior<TAction>(IEnumerable<IValidator<TAction>> validators)
    : IPipelineBehavior<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Validates the current action and short-circuits with a <see cref="ValidationError"/> when validation fails.
    /// </summary>
    /// <param name="request">The dispatched action being validated.</param>
    /// <param name="next">The next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">The token that cancels validation or dispatch.</param>
    /// <returns>
    /// The next pipeline result when validation succeeds, or a failure result containing a <see cref="ValidationError"/>
    /// when any validator reports failures.
    /// </returns>
    public async ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TAction>(request);
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count > 0)
        {
            return ValidationError.FromFailures(failures);
        }

        return await next();
    }
}
