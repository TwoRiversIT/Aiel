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

namespace Aiel.Permissions;

/// <summary>
/// Resolves the permission scope from the action execution context.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
/// <remarks>
/// Each action type that participates in permission checks must have a corresponding resolver
/// that extracts the relevant <see cref="PermissionScopeResolution"/> from the action payload.
/// </remarks>
public interface IPermissionScopeResolver<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Resolves the scope type and key from <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The action execution context holding the payload and actor.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> holding a <see cref="PermissionScopeResolution"/>
    /// when the scope can be determined; a failed <see cref="Result{T}"/> otherwise.
    /// </returns>
    Task<Result<PermissionScopeResolution>> ResolveAsync(
        IActionExecutionContext<TAction> context,
        CancellationToken cancellationToken = default);
}
