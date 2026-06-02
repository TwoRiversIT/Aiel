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
using Aiel.Execution;
using Aiel.Results;

namespace Aiel.Authorization;

/// <summary>
/// Validates an action's input before any permission check runs.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
/// <remarks>
/// Implement this interface per action type. The <see cref="IActionGate{TAction}"/> calls this
/// before <see cref="IActionAuthorizationChecker{TAction}"/>; a validation failure short-circuits the gate.
/// </remarks>
public interface IActionValidator<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Validates the action payload in the given execution context.
    /// </summary>
    /// <param name="context">The action execution context holding the payload and actor.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>
    /// <see cref="Result.Success()"/> when the action is valid;
    /// a failed <see cref="Result"/> carrying a <see cref="AuthorizationValidationError"/> otherwise.
    /// </returns>
    Task<Result> ValidateAsync(IActionExecutionContext<TAction> context, CancellationToken cancellationToken = default);
}
