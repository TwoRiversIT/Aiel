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

namespace Aiel.Authorization.Testing;

/// <summary>
/// A test-only implementation of <see cref="IAuthorizationGrantStore"/> that records all calls
/// and returns configurable results.
/// </summary>
/// <remarks>
/// Use this in unit tests to verify the permission store is called with the expected arguments,
/// or to simulate specific outcomes from the underlying store without touching real infrastructure.
/// All call lists are empty by default. Default results are successful with values from
/// <see cref="AuthorizationTestData"/>.
/// </remarks>
public sealed class FakeAuthorizationGrantStore : IAuthorizationGrantStore
{
    private readonly List<PermissionGrantCreateCall> _createGrantCalls = [];
    private readonly List<PermissionSubjectQuery> _getGrantsCalls = [];
    private readonly List<AuthorizationGrantId> _revokeGrantCalls = [];

    /// <summary>Gets the recorded arguments for each call to <see cref="IAuthorizationGrantStore.CreateGrantAsync"/>.</summary>
    public IReadOnlyList<PermissionGrantCreateCall> CreateGrantCalls => _createGrantCalls;

    /// <summary>Gets the recorded subject queries for each call to <see cref="IAuthorizationGrantStore.GetGrantsForSubjectAsync"/>.</summary>
    public IReadOnlyList<PermissionSubjectQuery> GetGrantsCalls => _getGrantsCalls;

    /// <summary>Gets the grant identifiers passed to each call to <see cref="IAuthorizationGrantStore.RevokeGrantAsync"/>.</summary>
    public IReadOnlyList<AuthorizationGrantId> RevokeGrantCalls => _revokeGrantCalls;

    /// <summary>
    /// Gets or initializes the result returned by <see cref="IAuthorizationGrantStore.CreateGrantAsync"/>.
    /// Defaults to <see cref="Result.Success{T}"/> with <see cref="AuthorizationTestData.GrantIdAlpha"/>.
    /// </summary>
    public Result<AuthorizationGrantId> CreateGrantResult { get; init; } =
        Result.Success(AuthorizationTestData.GrantIdAlpha);

    /// <summary>
    /// Gets or initializes the result returned by <see cref="IAuthorizationGrantStore.GetGrantsForSubjectAsync"/>.
    /// Defaults to <see cref="Result.Success{T}"/> with an empty grant list.
    /// </summary>
    public Result<IReadOnlyList<AuthorizationGrantSummary>> GetGrantsResult { get; init; } =
        Result.Success<IReadOnlyList<AuthorizationGrantSummary>>([]);

    /// <summary>
    /// Gets or initializes the result returned by <see cref="IAuthorizationGrantStore.RevokeGrantAsync"/>.
    /// Defaults to <see cref="Result.Success()"/>.
    /// </summary>
    public Result RevokeGrantResult { get; init; } = Result.Success();

    /// <inheritdoc />
    public Task<Result<AuthorizationGrantId>> CreateGrantAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision,
        CancellationToken cancellationToken = default)
    {
        _createGrantCalls.Add(new PermissionGrantCreateCall(
            permissionName,
            scopeType,
            scopeKey,
            subjectType,
            subjectKey,
            decision));

        return Task.FromResult(CreateGrantResult);
    }

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default)
    {
        _getGrantsCalls.Add(new PermissionSubjectQuery(subjectType, subjectKey));
        return Task.FromResult(GetGrantsResult);
    }

    /// <inheritdoc />
    public Task<Result> RevokeGrantAsync(
        AuthorizationGrantId grantId,
        CancellationToken cancellationToken = default)
    {
        _revokeGrantCalls.Add(grantId);
        return Task.FromResult(RevokeGrantResult);
    }
}

/// <summary>Captures the arguments passed to a single <see cref="IAuthorizationGrantStore.CreateGrantAsync"/> call.</summary>
/// <param name="PermissionName">The permission name passed to the call.</param>
/// <param name="ScopeType">The scope type passed to the call.</param>
/// <param name="ScopeKey">The scope key passed to the call.</param>
/// <param name="SubjectType">The subject type passed to the call.</param>
/// <param name="SubjectKey">The subject key passed to the call.</param>
/// <param name="Decision">The grant decision passed to the call.</param>
public sealed record PermissionGrantCreateCall(
    PermissionName PermissionName,
    AuthorizationScopeTypeName ScopeType,
    AuthorizationScopeKey ScopeKey,
    AuthorizationSubjectTypeName SubjectType,
    AuthorizationSubjectKey SubjectKey,
    AuthorizationGrantDecision Decision);

/// <summary>Captures the subject identity passed to a single <see cref="IAuthorizationGrantStore.GetGrantsForSubjectAsync"/> call.</summary>
/// <param name="SubjectType">The subject type passed to the call.</param>
/// <param name="SubjectKey">The subject key passed to the call.</param>
public sealed record PermissionSubjectQuery(
    AuthorizationSubjectTypeName SubjectType,
    AuthorizationSubjectKey SubjectKey);
