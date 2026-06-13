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

namespace Aiel.Authorization;

/// <summary>
/// Orchestrates validation and permission checks, producing a bound execution context on success.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
/// <remarks>
/// <para>The gate follows this sequence:</para>
/// <list type="number">
///   <item><description>Run <see cref="IActionValidator{TAction}"/> — fail fast on invalid input.</description></item>
///   <item><description>Run <see cref="IActionAuthorizationChecker{TAction}"/> — fail fast on denied permission.</description></item>
///   <item><description>Return a bound <see cref="IActionExecutionContext{TAction}"/> on success.</description></item>
/// </list>
/// <para>
/// If no <see cref="IActionValidator{TAction}"/> is registered, the gate proceeds to the permission check.
/// If no <see cref="IActionAuthorizationChecker{TAction}"/> is registered, the gate returns a
/// <see cref="MissingAuthorizationStoryError"/>.
/// </para>
/// </remarks>
public interface IActionGate<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Validates and authorizes <paramref name="action"/> for the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The ambient execution context identifying the actor and operation.</param>
    /// <param name="action">The action payload to validate and authorize.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> holding a bound <see cref="IActionExecutionContext{TAction}"/>
    /// when both validation and permission checks pass; a failed <see cref="Result{T}"/> otherwise.
    /// </returns>
    Task<Result<IActionExecutionContext<TAction>>> AuthorizeAsync(
        IExecutionContext context,
        TAction action,
        CancellationToken cancellationToken = default);
}
