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
/// Represents a single permission migration operation that can be executed by the migration runner.
/// </summary>
public interface IPermissionMigrationOperation;

/// <summary>
/// Adds a new permission to the catalog with <see cref="PermissionLifecycle.Active"/> lifecycle.
/// </summary>
/// <param name="StableId">The durable identifier for the permission being added.</param>
/// <param name="PermissionName">The initial published name for the permission.</param>
/// <param name="ScopeType">The scope type required when granting this permission.</param>
public sealed record AddPermissionOperation(
    PermissionStableId StableId,
    PermissionName PermissionName,
    AuthorizationScopeTypeName ScopeType) : IPermissionMigrationOperation;

/// <summary>
/// Renames an existing permission and updates all outstanding grants to reflect the new name.
/// A <see cref="PermissionManifestSnapshotRecord"/> capturing the previous name is also written.
/// </summary>
/// <param name="StableId">The durable identifier for the permission being renamed.</param>
/// <param name="PreviousName">The name the permission holds before the migration runs.</param>
/// <param name="NewName">The name the permission will hold after the migration runs.</param>
public sealed record RenamePermissionOperation(
    PermissionStableId StableId,
    PermissionName PreviousName,
    PermissionName NewName) : IPermissionMigrationOperation;

/// <summary>
/// Transitions an existing permission's lifecycle to <see cref="PermissionLifecycle.Deprecated"/>.
/// </summary>
/// <param name="StableId">The durable identifier for the permission being deprecated.</param>
public sealed record DeprecatePermissionOperation(PermissionStableId StableId) : IPermissionMigrationOperation;
