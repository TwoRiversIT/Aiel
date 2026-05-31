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
/// Represents a durable permission catalog entry and its lifecycle state.
/// </summary>
public sealed class PermissionCatalogEntry : StateBasedAggregateRoot<PermissionStableId>
{
    private PermissionCatalogEntry(
        PermissionStableId stableId,
        PermissionName permissionName,
        PermissionScopeTypeName scopeType,
        PermissionLifecycle lifecycle)
        : base(stableId)
    {
        PermissionName = permissionName;
        ScopeType = scopeType;
        Lifecycle = lifecycle;
    }

    private PermissionCatalogEntry()
    {
        PermissionName = default;
        ScopeType = default;
        Lifecycle = PermissionLifecycle.Active;
    }

    /// <summary>
    /// Gets the current published permission name.
    /// </summary>
    public PermissionName PermissionName { get; private set; }

    /// <summary>
    /// Gets the scope type required by this permission definition.
    /// </summary>
    public PermissionScopeTypeName ScopeType { get; private set; }

    /// <summary>
    /// Gets the published lifecycle state for this permission definition.
    /// </summary>
    public PermissionLifecycle Lifecycle { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the permission remains grantable.
    /// </summary>
    public Boolean AcceptsNewGrants => Lifecycle != PermissionLifecycle.Removed;

    /// <summary>
    /// Creates a permission catalog entry when the supplied identity is valid.
    /// </summary>
    /// <param name="stableId">The durable stable identifier for the permission.</param>
    /// <param name="permissionName">The current published permission name.</param>
    /// <param name="scopeType">The required scope type for grants.</param>
    /// <param name="lifecycle">The lifecycle state to assign.</param>
    /// <returns>A successful result containing the catalog entry, or a typed failure.</returns>
    public static Result<PermissionCatalogEntry> Create(
        PermissionStableId stableId,
        PermissionName permissionName,
        PermissionScopeTypeName scopeType,
        PermissionLifecycle lifecycle = PermissionLifecycle.Active)
    {
        if (stableId == default)
        {
            return Result<PermissionCatalogEntry>.Failure(
                new InvalidPermissionCatalogEntryError(PermissionDomainErrorMessages.CatalogStableIdRequired));
        }

        if (String.IsNullOrEmpty(permissionName.Value))
        {
            return Result<PermissionCatalogEntry>.Failure(
                new InvalidPermissionCatalogEntryError(PermissionDomainErrorMessages.CatalogPermissionNameRequired));
        }

        if (String.IsNullOrEmpty(scopeType.Value))
        {
            return Result<PermissionCatalogEntry>.Failure(
                new InvalidPermissionCatalogEntryError(PermissionDomainErrorMessages.CatalogScopeTypeRequired));
        }

        if (!Enum.IsDefined(lifecycle))
        {
            return Result<PermissionCatalogEntry>.Failure(
                new InvalidPermissionCatalogEntryError(PermissionDomainErrorMessages.CatalogLifecycleRequired));
        }

        return Result<PermissionCatalogEntry>.Success(
            new PermissionCatalogEntry(stableId, permissionName, scopeType, lifecycle));
    }

    /// <summary>
    /// Transitions the permission lifecycle without throwing for expected invalid moves.
    /// </summary>
    /// <param name="lifecycle">The requested lifecycle state.</param>
    /// <returns>A successful result when the transition is allowed, or a typed failure.</returns>
    public Result TransitionTo(PermissionLifecycle lifecycle)
    {
        if (!Enum.IsDefined(lifecycle))
        {
            return Result.Failure(
                new InvalidPermissionLifecycleTransitionError(PermissionDomainErrorMessages.CatalogLifecycleRequired));
        }

        if (lifecycle < Lifecycle)
        {
            return Result.Failure(
                new InvalidPermissionLifecycleTransitionError(PermissionDomainErrorMessages.LifecycleCanOnlyAdvanceForward));
        }

        if ((Int32)lifecycle - (Int32)Lifecycle > 1)
        {
            return Result.Failure(
                new InvalidPermissionLifecycleTransitionError(PermissionDomainErrorMessages.LifecycleCanOnlyAdvanceForward));
        }

        Lifecycle = lifecycle;
        return Result.Success();
    }
}
