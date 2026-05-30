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
using Aiel.Commands;
using Aiel.Domain;
using Aiel.Execution;
using Aiel.MultiTenancy;

namespace Aiel.EntityFrameworkCore;

/// <summary>
/// Extends the Entity Framework Core DbContext class to provide a base context for
/// Aiel applications. This class can be further extended to include common
/// functionality, configurations, or conventions that are specific to Aiel
/// applications, allowing for a consistent and reusable data access layer across
/// different projects.
/// </summary>
public class AielDbContext : DbContext, IUnitOfWork
{
    private readonly IExecutionContext? _executionContext;
    private readonly ITenantResolver? _tenantResolver;
    private Task<TenantResolution>? _tenantResolutionTask;

    protected AielDbContext()
    {
    }

    protected AielDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected AielDbContext(DbContextOptions options, IExecutionContext executionContext)
        : base(options)
    {
        _executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
    }

    protected AielDbContext(DbContextOptions options, TenantIdentity tenantIdentity)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(tenantIdentity);
        SetTenantResolution(new TenantResolution.Resolved(tenantIdentity));
    }

    protected AielDbContext(DbContextOptions options, TenantIdentity tenantIdentity, IExecutionContext executionContext)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(tenantIdentity);

        _executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        SetTenantResolution(new TenantResolution.Resolved(tenantIdentity));
    }

    protected AielDbContext(DbContextOptions options, ITenantResolver tenantResolver)
        : base(options)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
        _tenantResolutionTask = LoadTenantResolutionAsync(CancellationToken.None);
    }

    protected AielDbContext(DbContextOptions options, ITenantResolver tenantResolver, IExecutionContext executionContext)
        : base(options)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
        _executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        _tenantResolutionTask = LoadTenantResolutionAsync(CancellationToken.None);
    }

    protected TenantResolution CurrentTenantResolution { get; private set; } = new TenantResolution.Missing();

    private Boolean HasResolvedTenant
        => CurrentTenantResolution is TenantResolution.Resolved;

    private Guid ResolvedTenantIdValue
        => CurrentTenantResolution is TenantResolution.Resolved resolved
            ? resolved.TenantIdentity.TenantId.Value
            : Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyMultiTenantQueryFilters(this);
    }

    public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantResolution = await EnsureTenantResolutionAsync(cancellationToken);

        StampTenantIds(tenantResolution);
        StampAuditMetadata();

        var aggregates = GetTrackedAggregates().ToArray();
        var domainEvents = aggregates.SelectMany(static aggregate => aggregate.DomainEvents).ToArray();

        await PersistDomainEventsAsync(domainEvents, cancellationToken);

        var rowsAffected = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return rowsAffected;
    }

    protected void SetTenantResolution(TenantResolution tenantResolution)
    {
        ArgumentNullException.ThrowIfNull(tenantResolution);

        CurrentTenantResolution = tenantResolution;
        _tenantResolutionTask = Task.FromResult(tenantResolution);
    }

    protected virtual ValueTask PersistDomainEventsAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    protected virtual IEnumerable<IAggregateRoot> GetTrackedAggregates()
        => ChangeTracker
            .Entries()
            .Where(static entry => entry.Entity is IAggregateRoot)
            .Select(static entry => (IAggregateRoot)entry.Entity)
            .Distinct();

    protected virtual async ValueTask<TenantResolution> EnsureTenantResolutionAsync(CancellationToken cancellationToken)
    {
        if (_tenantResolutionTask is not null)
        {
            return await _tenantResolutionTask.WaitAsync(cancellationToken);
        }

        if (_tenantResolver is null)
        {
            return CurrentTenantResolution;
        }

        _tenantResolutionTask = LoadTenantResolutionAsync(cancellationToken);
        return await _tenantResolutionTask;
    }

    private async Task<TenantResolution> LoadTenantResolutionAsync(CancellationToken cancellationToken)
    {
        TenantResolution tenantResolution;

        try
        {
            tenantResolution = await _tenantResolver!.ResolveAsync(cancellationToken);
        }
        catch
        {
            tenantResolution = new TenantResolution.Error(TenantResolutionErrorReason.UnexpectedException);
        }

        SetTenantResolution(tenantResolution);
        return tenantResolution;
    }

    private void StampTenantIds(TenantResolution tenantResolution)
    {
        if (tenantResolution is not TenantResolution.Resolved resolved)
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries<IMultiTenant>())
        {
            if (entry.State != EntityState.Added)
            {
                continue;
            }

            if (entry.Entity.TenantId == default)
            {
                entry.Entity.TenantId = resolved.TenantIdentity.TenantId;
            }
        }
    }

    private void StampAuditMetadata()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var auditIdentity = (_executionContext?.Actor ?? SystemActor.Instance).AuditIdentity;

        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.CreatedAt = timestamp;
                    }

                    if (String.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                    {
                        entry.Entity.CreatedBy = auditIdentity;
                    }

                    entry.Entity.ModifiedAt = timestamp;
                    entry.Entity.ModifiedBy = auditIdentity;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = timestamp;
                    entry.Entity.ModifiedBy = auditIdentity;
                    break;
            }
        }
    }
}
