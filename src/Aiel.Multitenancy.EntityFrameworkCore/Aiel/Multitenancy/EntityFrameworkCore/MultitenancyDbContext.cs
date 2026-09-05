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

using Aiel.Actions;
using Aiel.EntityFrameworkCore;
using Aiel.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Aiel.Multitenancy.EntityFrameworkCore;

/// <summary>
/// Extends the Entity Framework Core DbContext class to provide a base context for
/// Aiel applications. This class can be further extended to include common
/// functionality, configurations, or conventions that are specific to Aiel
/// applications, allowing for a consistent and reusable EntityFrameworkCore across
/// different projects.
/// </summary>
public class MultitenancyDbContext : AielDbContext
{
    private readonly ITenantResolver? _tenantResolver;
    private Task<TenantResolution>? _tenantResolutionTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class.
    /// </summary>
    protected MultitenancyDbContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options"></param>
    protected MultitenancyDbContext(DbContextOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class with the specified options and execution context.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="executionContext"></param>
    protected MultitenancyDbContext(DbContextOptions options, IExecutionContext executionContext)
        : base(options, executionContext)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class with the specified options and current tenant.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="currentTenant"></param>
    protected MultitenancyDbContext(DbContextOptions options, CurrentTenant currentTenant)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(currentTenant);
        SetTenantResolution(new TenantResolution.Resolved(currentTenant));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class with the specified options, current tenant, and execution context.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="currentTenant"></param>
    /// <param name="executionContext"></param>
    protected MultitenancyDbContext(DbContextOptions options, CurrentTenant currentTenant, IExecutionContext executionContext)
        : base(options, executionContext)
    {
        ArgumentNullException.ThrowIfNull(currentTenant);

        SetTenantResolution(new TenantResolution.Resolved(currentTenant));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class with the specified options and tenant resolver.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="tenantResolver"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected MultitenancyDbContext(DbContextOptions options, ITenantResolver tenantResolver)
        : base(options)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
        _tenantResolutionTask = LoadTenantResolutionAsync(CancellationToken.None);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultitenancyDbContext"/> class with the specified options, tenant resolver, and execution context.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="tenantResolver"></param>
    /// <param name="executionContext"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected MultitenancyDbContext(DbContextOptions options, ITenantResolver tenantResolver, IExecutionContext executionContext)
        : base(options, executionContext)
    {
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
        _tenantResolutionTask = LoadTenantResolutionAsync(CancellationToken.None);
    }

    /// <summary>
    /// Gets the current tenant resolution, which indicates whether a tenant has been resolved and provides access to the current tenant information if available.
    /// </summary>
    protected TenantResolution CurrentTenantResolution { get; private set; } = new TenantResolution.Missing();

    [UnconditionalSuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Referenced by ModelBuilderExtensions static construction")]
    private Boolean HasResolvedTenant
        => CurrentTenantResolution is TenantResolution.Resolved;

    [UnconditionalSuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Referenced by ModelBuilderExtensions static construction")]
    private Guid ResolvedTenantIdValue
        => CurrentTenantResolution is TenantResolution.Resolved resolved
            ? resolved.CurrentTenant.TenantId.Value
            : Guid.Empty;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyMultiTenantQueryFilters(this);
    }

    /// <inheritdoc/>
    public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantResolution = await EnsureTenantResolutionAsync(cancellationToken);

        StampTenantIds(tenantResolution);

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

    /// <summary>
    /// Sets the current tenant resolution.
    /// </summary>
    /// <param name="tenantResolution">The tenant resolution to set.</param>
    protected void SetTenantResolution(TenantResolution tenantResolution)
    {
        ArgumentNullException.ThrowIfNull(tenantResolution);

        CurrentTenantResolution = tenantResolution;
        _tenantResolutionTask = Task.FromResult(tenantResolution);
    }

    /// <summary>
    /// Ensures that the tenant resolution is available, either by returning the existing resolution or by resolving it using the tenant resolver if necessary.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>The tenant resolution.</returns>
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
                entry.Entity.TenantId = resolved.CurrentTenant.TenantId;
            }
        }
    }
}
