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

namespace Aiel.Authorization.EntityFrameworkCore;

/// <summary>
/// A fluent builder that accumulates a sequence of permission migration operations.
/// </summary>
/// <remarks>
/// Pass the completed plan to <see cref="PermissionMigrationRunner.ApplyAsync"/> to execute it
/// against a <see cref="PermissionsDbContext"/>.
/// </remarks>
/// <example>
/// <code>
/// var plan = new PermissionMigrationPlan()
///     .Add(StableIdAlpha, PermissionNameRead, ScopeTypeAlpha)
///     .Rename(StableIdAlpha, PermissionNameRead, PermissionNameWrite)
///     .Deprecate(StableIdBeta);
/// await runner.ApplyAsync(plan, cancellationToken);
/// </code>
/// </example>
public sealed class PermissionMigrationPlan
{
    private readonly List<IPermissionMigrationOperation> _operations = [];

    /// <summary>
    /// Gets the ordered sequence of operations in this plan as a typed migration surface.
    /// </summary>
    public IReadOnlyList<IPermissionMigrationOperation> TypedOperations => _operations;

    /// <summary>
    /// Gets the ordered sequence of operations in this plan.
    /// </summary>
    public IReadOnlyList<Object> Operations => _operations;

    /// <summary>
    /// Appends an <see cref="AddPermissionOperation"/> that inserts a new catalog entry.
    /// </summary>
    /// <param name="stableId">The durable identifier for the new permission.</param>
    /// <param name="permissionName">The initial published name.</param>
    /// <param name="scopeType">The scope type required by this permission.</param>
    /// <returns>This plan instance, allowing further chaining.</returns>
    public PermissionMigrationPlan Add(
        PermissionStableId stableId,
        PermissionName permissionName,
        PermissionScopeTypeName scopeType)
    {
        _operations.Add(new AddPermissionOperation(stableId, permissionName, scopeType));
        return this;
    }

    /// <summary>
    /// Appends a <see cref="RenamePermissionOperation"/> that updates the catalog and all existing grants.
    /// </summary>
    /// <param name="stableId">The durable identifier of the permission to rename.</param>
    /// <param name="previousName">The name currently held by the permission.</param>
    /// <param name="newName">The name the permission will hold after migration.</param>
    /// <returns>This plan instance, allowing further chaining.</returns>
    public PermissionMigrationPlan Rename(
        PermissionStableId stableId,
        PermissionName previousName,
        PermissionName newName)
    {
        _operations.Add(new RenamePermissionOperation(stableId, previousName, newName));
        return this;
    }

    /// <summary>
    /// Appends a <see cref="DeprecatePermissionOperation"/> that advances the catalog lifecycle to
    /// <see cref="PermissionLifecycle.Deprecated"/>.
    /// </summary>
    /// <param name="stableId">The durable identifier of the permission to deprecate.</param>
    /// <returns>This plan instance, allowing further chaining.</returns>
    public PermissionMigrationPlan Deprecate(PermissionStableId stableId)
    {
        _operations.Add(new DeprecatePermissionOperation(stableId));
        return this;
    }
}
