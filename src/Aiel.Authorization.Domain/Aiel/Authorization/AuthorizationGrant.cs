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

using Aiel.Domain;
using Aiel.Results;

namespace Aiel.Authorization;

/// <summary>
/// Represents a concrete authorization decision for a subject within a scope.
/// </summary>
public sealed class AuthorizationGrant : StateBasedAggregateRoot<AuthorizationGrantId>
{
    private AuthorizationGrant(
        AuthorizationGrantId grantId,
        PermissionStableId permissionStableId,
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision)
        : base(grantId)
    {
        PermissionStableId = permissionStableId;
        PermissionName = permissionName;
        ScopeType = scopeType;
        ScopeKey = scopeKey;
        SubjectType = subjectType;
        SubjectKey = subjectKey;
        Decision = decision;
    }

    private AuthorizationGrant()
    {
        PermissionStableId = default;
        PermissionName = default;
        ScopeType = default;
        ScopeKey = default;
        SubjectType = default;
        SubjectKey = default;
        Decision = AuthorizationGrantDecision.Granted;
    }

    /// <summary>
    /// Gets the stable permission identifier referenced by the grant.
    /// </summary>
    public PermissionStableId PermissionStableId { get; }

    /// <summary>
    /// Gets the current published permission name referenced by the grant.
    /// </summary>
    public PermissionName PermissionName { get; }

    /// <summary>
    /// Gets the scope type required by the grant.
    /// </summary>
    public AuthorizationScopeTypeName ScopeType { get; }

    /// <summary>
    /// Gets the specific scope key targeted by the grant.
    /// </summary>
    public AuthorizationScopeKey ScopeKey { get; }

    /// <summary>
    /// Gets the subject type targeted by the grant.
    /// </summary>
    public AuthorizationSubjectTypeName SubjectType { get; }

    /// <summary>
    /// Gets the specific subject key targeted by the grant.
    /// </summary>
    public AuthorizationSubjectKey SubjectKey { get; }

    /// <summary>
    /// Gets the persisted grant decision.
    /// </summary>
    public AuthorizationGrantDecision Decision { get; }

    /// <summary>
    /// Creates a permission grant from explicit permission identity values.
    /// </summary>
    /// <param name="grantId">The unique grant identifier.</param>
    /// <param name="permissionStableId">The durable permission identifier.</param>
    /// <param name="permissionName">The current published permission name.</param>
    /// <param name="scopeType">The required scope type.</param>
    /// <param name="scopeKey">The specific scope instance.</param>
    /// <param name="subjectType">The targeted subject type.</param>
    /// <param name="subjectKey">The targeted subject instance.</param>
    /// <param name="decision">The persisted grant decision.</param>
    /// <returns>A successful result containing the grant, or a typed failure.</returns>
    public static Result<AuthorizationGrant> Create(
        AuthorizationGrantId grantId,
        PermissionStableId permissionStableId,
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision)
    {
        if (grantId == default)
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantIdRequired));
        }

        if (permissionStableId == default)
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantStableIdRequired));
        }

        if (String.IsNullOrEmpty(permissionName.Value))
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantPermissionNameRequired));
        }

        if (String.IsNullOrEmpty(scopeType.Value))
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantScopeTypeRequired));
        }

        if (String.IsNullOrEmpty(scopeKey.Value))
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantScopeKeyRequired));
        }

        if (String.IsNullOrEmpty(subjectType.Value))
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantSubjectTypeRequired));
        }

        if (String.IsNullOrEmpty(subjectKey.Value))
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantSubjectKeyRequired));
        }

        if (!Enum.IsDefined(decision))
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.GrantDecisionRequired));
        }

        return Result<AuthorizationGrant>.Success(
            new AuthorizationGrant(
                grantId,
                permissionStableId,
                permissionName,
                scopeType,
                scopeKey,
                subjectType,
                subjectKey,
                decision));
    }

    /// <summary>
    /// Creates a permission grant from a catalog entry while enforcing catalog lifecycle rules.
    /// </summary>
    /// <param name="grantId">The unique grant identifier.</param>
    /// <param name="catalogEntry">The catalog entry being granted.</param>
    /// <param name="scopeKey">The specific scope instance.</param>
    /// <param name="subjectType">The targeted subject type.</param>
    /// <param name="subjectKey">The targeted subject instance.</param>
    /// <param name="decision">The persisted grant decision.</param>
    /// <returns>A successful result containing the grant, or a typed failure.</returns>
    public static Result<AuthorizationGrant> Create(
        AuthorizationGrantId grantId,
        PermissionCatalogEntry? catalogEntry,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision)
    {
        if (catalogEntry is null)
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.CatalogEntryRequired));
        }

        if (!catalogEntry.AcceptsNewGrants)
        {
            return Result<AuthorizationGrant>.Failure(
                new InvalidAuthorizationGrantError(AuthorizationDomainErrorMessages.RemovedCatalogEntriesCannotIssueGrants));
        }

        return Create(
            grantId,
            catalogEntry.Id,
            catalogEntry.PermissionName,
            catalogEntry.ScopeType,
            scopeKey,
            subjectType,
            subjectKey,
            decision);
    }

    /// <summary>
    /// Determines whether the grant applies to the supplied scope.
    /// </summary>
    /// <param name="scopeType">The scope type to compare.</param>
    /// <param name="scopeKey">The scope key to compare.</param>
    /// <returns><see langword="true"/> when the supplied scope matches this grant; otherwise, <see langword="false"/>.</returns>
    public Boolean MatchesScope(AuthorizationScopeTypeName scopeType, AuthorizationScopeKey scopeKey)
        => ScopeType == scopeType && ScopeKey == scopeKey;

    /// <summary>
    /// Determines whether the grant applies to the supplied subject.
    /// </summary>
    /// <param name="subjectType">The subject type to compare.</param>
    /// <param name="subjectKey">The subject key to compare.</param>
    /// <returns><see langword="true"/> when the supplied subject matches this grant; otherwise, <see langword="false"/>.</returns>
    public Boolean MatchesSubject(AuthorizationSubjectTypeName subjectType, AuthorizationSubjectKey subjectKey)
        => SubjectType == subjectType && SubjectKey == subjectKey;
}
