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
/// EF Core persistence record for a permission catalog entry.
/// </summary>
/// <remarks>
/// This is an infrastructure type exposed only because EF Core requires it.
/// Consume catalog data through <see cref="PermissionMigrationRunner"/> instead.
/// </remarks>
public sealed class PermissionCatalogRecord
{
    /// <summary>
    /// Gets or sets the stable identifier for the permission catalog entry.
    /// </summary>
    public String StableId { get; set; } = String.Empty;
    /// <summary>
    /// Gets or sets the name of the permission associated with the catalog entry.
    /// </summary>
    public String PermissionName { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the type of scope associated with the permission catalog entry.
    /// </summary>
    public String ScopeType { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the lifecycle state of the permission catalog entry.
    /// </summary>
    public Int32 Lifecycle { get; set; }

    /// <summary>
    /// Gets or sets the list of authorization grants associated with the permission catalog entry.
    /// </summary>
    public List<AuthorizationGrantRecord> Grants { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of permission manifest snapshots associated with the permission catalog entry.
    /// </summary>
    public List<PermissionManifestSnapshotRecord> Snapshots { get; set; } = [];
}
