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
/// This context is registered in DI and consumed only through <see cref="IPermissionStore"/> and
/// <see cref="PermissionMigrationRunner"/>. Avoid taking a direct dependency on this type from
/// outside the infrastructure assembly.
/// </remarks>
public sealed class PermissionsDbContext(DbContextOptions<PermissionsDbContext> options)
    : DbContext(options)
{
    public DbSet<PermissionCatalogRecord> Catalog => Set<PermissionCatalogRecord>();
    public DbSet<PermissionGrantRecord> Grants => Set<PermissionGrantRecord>();
    public DbSet<PermissionManifestSnapshotRecord> Snapshots => Set<PermissionManifestSnapshotRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PermissionCatalogRecord>(catalog =>
        {
            catalog.ToTable("permission_catalog");
            catalog.HasKey(r => r.StableId);
            catalog.Property(r => r.StableId).HasColumnName("stable_id").HasMaxLength(256);
            catalog.Property(r => r.PermissionName).HasColumnName("permission_name").HasMaxLength(256).IsRequired();
            catalog.Property(r => r.ScopeType).HasColumnName("scope_type").HasMaxLength(128).IsRequired();
            catalog.Property(r => r.Lifecycle).HasColumnName("lifecycle").IsRequired();

            catalog.HasMany(r => r.Grants)
                .WithOne(g => g.Catalog)
                .HasForeignKey(g => g.StableId)
                .HasPrincipalKey(r => r.StableId);

            catalog.HasMany(r => r.Snapshots)
                .WithOne(s => s.Catalog)
                .HasForeignKey(s => s.StableId)
                .HasPrincipalKey(r => r.StableId);
        });

        modelBuilder.Entity<PermissionGrantRecord>(grant =>
        {
            grant.ToTable("permission_grants");
            grant.HasKey(r => r.Id);
            grant.Property(r => r.Id).HasColumnName("id");
            grant.Property(r => r.StableId).HasColumnName("stable_id").HasMaxLength(256).IsRequired();
            grant.Property(r => r.PermissionName).HasColumnName("permission_name").HasMaxLength(256).IsRequired();
            grant.Property(r => r.ScopeType).HasColumnName("scope_type").HasMaxLength(128).IsRequired();
            grant.Property(r => r.ScopeKey).HasColumnName("scope_key").HasMaxLength(512).IsRequired();
            grant.Property(r => r.SubjectType).HasColumnName("subject_type").HasMaxLength(128).IsRequired();
            grant.Property(r => r.SubjectKey).HasColumnName("subject_key").HasMaxLength(512).IsRequired();
            grant.Property(r => r.Decision).HasColumnName("decision").IsRequired();
            grant.Property(r => r.GrantedAt).HasColumnName("granted_at").IsRequired();

            grant.HasIndex(r => new { r.SubjectType, r.SubjectKey });
            grant.HasIndex(r => new { r.PermissionName, r.ScopeType, r.ScopeKey, r.SubjectType, r.SubjectKey });
        });

        modelBuilder.Entity<PermissionManifestSnapshotRecord>(snapshot =>
        {
            snapshot.ToTable("permission_manifest_snapshots");
            snapshot.HasKey(r => r.Id);
            snapshot.Property(r => r.Id).HasColumnName("id");
            snapshot.Property(r => r.StableId).HasColumnName("stable_id").HasMaxLength(256).IsRequired();
            snapshot.Property(r => r.PreviousPermissionName).HasColumnName("previous_permission_name").HasMaxLength(256).IsRequired();
            snapshot.Property(r => r.NewPermissionName).HasColumnName("new_permission_name").HasMaxLength(256).IsRequired();
            snapshot.Property(r => r.MigratedAt).HasColumnName("migrated_at").IsRequired();
        });
    }
}
