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

using Aiel.Results;

namespace Aiel.Authorization;

/// <summary>
/// Evaluates whether a permission is granted for a given subject and scope combination.
/// </summary>
/// <remarks>
/// Implementations query the persisted grant store and apply any override rules (prohibit beats grant).
/// This is a read-side concern; mutations go through <see cref="IAuthorizationManager"/>.
/// </remarks>
public interface IAuthorizationGrantEvaluator
{
    /// <summary>
    /// Evaluates the effective decision for the given permission, scope, and subject.
    /// </summary>
    /// <param name="permissionName">The permission to evaluate.</param>
    /// <param name="scopeType">The type of scope to evaluate against.</param>
    /// <param name="scopeKey">The specific scope key to evaluate against.</param>
    /// <param name="subjectType">The type of subject being evaluated.</param>
    /// <param name="subjectKey">The specific subject key being evaluated.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>
    /// The effective <see cref="AuthorizationGrantDecision"/> when a matching grant exists;
    /// <see langword="null"/> when no grant covers the combination.
    /// </returns>
    Task<Result<AuthorizationGrantDecision?>> EvaluateAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default);
}
