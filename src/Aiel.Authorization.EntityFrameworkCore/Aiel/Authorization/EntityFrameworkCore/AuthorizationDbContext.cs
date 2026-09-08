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

using Microsoft.EntityFrameworkCore;

namespace Aiel.Authorization.EntityFrameworkCore;

/// <summary>
/// EF Core database context for permission grants, the catalog, and migration snapshots.
/// </summary>
/// <remarks>
/// This context is registered in DI and consumed only through <see cref="IAuthorizationGrantStore"/> and
/// <see cref="PermissionMigrationRunner"/>. Avoid taking a direct dependency on this type from
/// outside the infrastructure assembly.
/// </remarks>
public sealed class AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Gets the permission catalog records.
    /// </summary>
    public DbSet<PermissionCatalogRecord> Catalog => Set<PermissionCatalogRecord>();

    /// <summary>
    /// Gets the authorization grant records.
    /// </summary>
    public DbSet<AuthorizationGrantRecord> Grants => Set<AuthorizationGrantRecord>();

    /// <summary>
    /// Gets the permission manifest snapshot records.
    /// </summary>
    public DbSet<PermissionManifestSnapshotRecord> Snapshots => Set<PermissionManifestSnapshotRecord>();

    /// <summary>
    /// Configures the EF Core model for the authorization database context, including table names, keys, properties, and relationships.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PermissionCatalogRecord>(catalog =>
        {
            catalog.ToTable("PermissionCatalog");
            catalog.HasKey(r => r.StableId);
            catalog.Property(r => r.StableId).HasColumnName("StableId").HasMaxLength(256);
            catalog.Property(r => r.PermissionName).HasColumnName("PermissionName").HasMaxLength(256).IsRequired();
            catalog.Property(r => r.ScopeType).HasColumnName("ScopeType").HasMaxLength(128).IsRequired();
            catalog.Property(r => r.Lifecycle).HasColumnName("Lifecycle").IsRequired();

            catalog.HasMany(r => r.Grants)
                .WithOne(g => g.Catalog)
                .HasForeignKey(g => g.StableId)
                .HasPrincipalKey(r => r.StableId);

            catalog.HasMany(r => r.Snapshots)
                .WithOne(s => s.Catalog)
                .HasForeignKey(s => s.StableId)
                .HasPrincipalKey(r => r.StableId);
        });

        modelBuilder.Entity<AuthorizationGrantRecord>(grant =>
        {
            grant.ToTable("AuthorizationGrants");
            grant.HasKey(r => r.Id);
            grant.Property(r => r.Id).HasColumnName("Id");
            grant.Property(r => r.StableId).HasColumnName("StableId").HasMaxLength(256).IsRequired();
            grant.Property(r => r.PermissionName).HasColumnName("PermissionName").HasMaxLength(256).IsRequired();
            grant.Property(r => r.ScopeType).HasColumnName("ScopeType").HasMaxLength(128).IsRequired();
            grant.Property(r => r.ScopeKey).HasColumnName("ScopeKey").HasMaxLength(512).IsRequired();
            grant.Property(r => r.SubjectType).HasColumnName("SubjectType").HasMaxLength(128).IsRequired();
            grant.Property(r => r.SubjectKey).HasColumnName("SubjectKey").HasMaxLength(512).IsRequired();
            grant.Property(r => r.Decision).HasColumnName("Decision").IsRequired();
            grant.Property(r => r.GrantedAt).HasColumnName("GrantedAt").IsRequired();

            grant.HasIndex(r => new { r.SubjectType, r.SubjectKey });
            grant.HasIndex(r => new { r.PermissionName, r.ScopeType, r.ScopeKey, r.SubjectType, r.SubjectKey });
        });

        modelBuilder.Entity<PermissionManifestSnapshotRecord>(snapshot =>
        {
            snapshot.ToTable("PermissionManifestSnapshots");
            snapshot.HasKey(r => r.Id);
            snapshot.Property(r => r.Id).HasColumnName("Id");
            snapshot.Property(r => r.StableId).HasColumnName("StableId").HasMaxLength(256).IsRequired();
            snapshot.Property(r => r.PreviousPermissionName).HasColumnName("PreviousPermissionName").HasMaxLength(256).IsRequired();
            snapshot.Property(r => r.NewPermissionName).HasColumnName("NewPermissionName").HasMaxLength(256).IsRequired();
            snapshot.Property(r => r.MigratedAt).HasColumnName("MigratedAt").IsRequired();
        });
    }
}
