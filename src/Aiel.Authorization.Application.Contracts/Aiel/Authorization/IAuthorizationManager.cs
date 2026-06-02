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
/// Application service for creating, revoking, and querying permission grants.
/// </summary>
/// <remarks>
/// <para>
/// The manager coordinates with <see cref="IAuthorizationGrantStore"/> for persistence and
/// <see cref="IAuthorizationDefinitionRegistry"/> for validation without exposing domain entities
/// or raw store records to callers.
/// </para>
/// </remarks>
public interface IAuthorizationManager
{
    /// <summary>
    /// Creates a new permission grant and returns its identifier.
    /// </summary>
    /// <param name="request">The grant parameters.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> holding the new <see cref="AuthorizationGrantId"/> on success;
    /// a failed <see cref="Result{T}"/> when the request is invalid or the permission is not registered.
    /// </returns>
    Task<Result<AuthorizationGrantId>> GrantAsync(
        GrantPermissionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an existing permission grant.
    /// </summary>
    /// <param name="request">The revocation parameters.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A successful <see cref="Result"/> on success.</returns>
    Task<Result> RevokeAsync(
        RevokeAuthorizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all grants held by the given subject without exposing persistence records.
    /// </summary>
    /// <param name="subjectType">The type of subject to query.</param>
    /// <param name="subjectKey">The specific subject key to query.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A non-null list of <see cref="AuthorizationGrantSummary"/> instances; empty when none are found.</returns>
    Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default);
}
