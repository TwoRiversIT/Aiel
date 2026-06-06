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

using Aiel.Execution;
using Aiel.Results;

namespace Aiel.Authorization;

/// <summary>
/// Performs resource-level authorization checks outside of the <see cref="IActionGate{TAction}"/> flow.
/// </summary>
/// <remarks>
/// Use this service when authorization must be checked mid-handler or against a specific resource
/// rather than at the gate boundary. It delegates evaluation to <see cref="IAuthorizationGrantEvaluator"/>.
/// </remarks>
public interface IResourceAuthorizationService
{
    /// <summary>
    /// Checks whether the actor in <paramref name="context"/> holds the required permission for the given scope.
    /// </summary>
    /// <param name="context">The ambient execution context identifying the actor.</param>
    /// <param name="permissionName">The permission name to check.</param>
    /// <param name="scopeType">The scope type to check against.</param>
    /// <param name="scopeKey">The specific scope key to check against.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>
    /// <see cref="Result.Success()"/> when the actor holds the permission;
    /// a failed <see cref="Result"/> carrying a <see cref="AuthorizationDeniedError"/> otherwise.
    /// </returns>
    Task<Result> AuthorizeAsync(
        IExecutionContext context,
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        CancellationToken cancellationToken = default);
}
