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
/// A test-only implementation of <see cref="IPermissionStore"/> that records all calls
/// and returns configurable results.
/// </summary>
/// <remarks>
/// Use this in unit tests to verify the permission store is called with the expected arguments,
/// or to simulate specific outcomes from the underlying store without touching real infrastructure.
/// All call lists are empty by default. Default results are successful with values from
/// <see cref="PermissionTestData"/>.
/// </remarks>
public sealed class FakePermissionStore : IPermissionStore
{
    private readonly List<PermissionGrantCreateCall> _createGrantCalls = [];
    private readonly List<PermissionSubjectQuery> _getGrantsCalls = [];
    private readonly List<PermissionGrantId> _revokeGrantCalls = [];

    /// <summary>Gets the recorded arguments for each call to <see cref="IPermissionStore.CreateGrantAsync"/>.</summary>
    public IReadOnlyList<PermissionGrantCreateCall> CreateGrantCalls => _createGrantCalls;

    /// <summary>Gets the recorded subject queries for each call to <see cref="IPermissionStore.GetGrantsForSubjectAsync"/>.</summary>
    public IReadOnlyList<PermissionSubjectQuery> GetGrantsCalls => _getGrantsCalls;

    /// <summary>Gets the grant identifiers passed to each call to <see cref="IPermissionStore.RevokeGrantAsync"/>.</summary>
    public IReadOnlyList<PermissionGrantId> RevokeGrantCalls => _revokeGrantCalls;

    /// <summary>
    /// Gets or initializes the result returned by <see cref="IPermissionStore.CreateGrantAsync"/>.
    /// Defaults to <see cref="Result.Success{T}"/> with <see cref="PermissionTestData.GrantIdAlpha"/>.
    /// </summary>
    public Result<PermissionGrantId> CreateGrantResult { get; init; } =
        Result.Success(PermissionTestData.GrantIdAlpha);

    /// <summary>
    /// Gets or initializes the result returned by <see cref="IPermissionStore.GetGrantsForSubjectAsync"/>.
    /// Defaults to <see cref="Result.Success{T}"/> with an empty grant list.
    /// </summary>
    public Result<IReadOnlyList<PermissionGrantSummary>> GetGrantsResult { get; init; } =
        Result.Success<IReadOnlyList<PermissionGrantSummary>>([]);

    /// <summary>
    /// Gets or initializes the result returned by <see cref="IPermissionStore.RevokeGrantAsync"/>.
    /// Defaults to <see cref="Result.Success()"/>.
    /// </summary>
    public Result RevokeGrantResult { get; init; } = Result.Success();

    /// <inheritdoc />
    public Task<Result<PermissionGrantId>> CreateGrantAsync(
        PermissionName permissionName,
        PermissionScopeTypeName scopeType,
        PermissionScopeKey scopeKey,
        PermissionSubjectTypeName subjectType,
        PermissionSubjectKey subjectKey,
        PermissionGrantDecision decision,
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
    public Task<Result<IReadOnlyList<PermissionGrantSummary>>> GetGrantsForSubjectAsync(
        PermissionSubjectTypeName subjectType,
        PermissionSubjectKey subjectKey,
        CancellationToken cancellationToken = default)
    {
        _getGrantsCalls.Add(new PermissionSubjectQuery(subjectType, subjectKey));
        return Task.FromResult(GetGrantsResult);
    }

    /// <inheritdoc />
    public Task<Result> RevokeGrantAsync(
        PermissionGrantId grantId,
        CancellationToken cancellationToken = default)
    {
        _revokeGrantCalls.Add(grantId);
        return Task.FromResult(RevokeGrantResult);
    }
}

/// <summary>Captures the arguments passed to a single <see cref="IPermissionStore.CreateGrantAsync"/> call.</summary>
/// <param name="PermissionName">The permission name passed to the call.</param>
/// <param name="ScopeType">The scope type passed to the call.</param>
/// <param name="ScopeKey">The scope key passed to the call.</param>
/// <param name="SubjectType">The subject type passed to the call.</param>
/// <param name="SubjectKey">The subject key passed to the call.</param>
/// <param name="Decision">The grant decision passed to the call.</param>
public sealed record PermissionGrantCreateCall(
    PermissionName PermissionName,
    PermissionScopeTypeName ScopeType,
    PermissionScopeKey ScopeKey,
    PermissionSubjectTypeName SubjectType,
    PermissionSubjectKey SubjectKey,
    PermissionGrantDecision Decision);

/// <summary>Captures the subject identity passed to a single <see cref="IPermissionStore.GetGrantsForSubjectAsync"/> call.</summary>
/// <param name="SubjectType">The subject type passed to the call.</param>
/// <param name="SubjectKey">The subject key passed to the call.</param>
public sealed record PermissionSubjectQuery(
    PermissionSubjectTypeName SubjectType,
    PermissionSubjectKey SubjectKey);
