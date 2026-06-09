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
/// Provides raw persistence operations for permission grants.
/// </summary>
/// <remarks>
/// This is an infrastructure-facing port. Business logic belongs in <see cref="IAuthorizationManager"/>,
/// not here. Implementations live in the Infrastructure layer.
/// </remarks>
public interface IAuthorizationGrantStore
{
    /// <summary>
    /// Persists a new permission grant and returns its assigned identifier.
    /// </summary>
    /// <param name="permissionName">The permission to grant.</param>
    /// <param name="scopeType">The scope type this grant applies to.</param>
    /// <param name="scopeKey">The specific scope key this grant is bound to.</param>
    /// <param name="subjectType">The subject type this grant targets.</param>
    /// <param name="subjectKey">The specific subject key this grant is bound to.</param>
    /// <param name="decision">Whether the grant allows or explicitly prohibits the permission.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A successful <see cref="Result{T}"/> with the new <see cref="AuthorizationGrantId"/> on success.</returns>
    Task<Result<AuthorizationGrantId>> CreateGrantAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the persisted grant identified by <paramref name="grantId"/>.
    /// </summary>
    /// <param name="grantId">The identifier of the grant to remove.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A successful <see cref="Result"/> on success.</returns>
    Task<Result> RevokeGrantAsync(
        AuthorizationGrantId grantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all grants held by the given subject.
    /// </summary>
    /// <param name="subjectType">The type of subject to query.</param>
    /// <param name="subjectKey">The specific subject key to query.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A non-null, possibly empty list of <see cref="AuthorizationGrantSummary"/> instances.</returns>
    Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default);
}
