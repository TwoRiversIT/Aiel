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
using Aiel.Domain.Aggregates;
using Aiel.Domain.Auditing;
using Aiel.Domain.Events;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditMetadata();

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

    private void SetAuditMetadata()
    {
        var timestamp = _executionContext?.Timestamp ?? DateTimeOffset.UtcNow;
        var auditIdentity = (_executionContext?.Actor ?? SystemActor.Instance).AuditIdentity;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is ISetCreated addedCreate && addedCreate.CreatedAt == default)
                    {
                        addedCreate.AuditCreate(auditIdentity, timestamp);
                    }

                    if (entry.Entity is ISetUpdated addedUpdate && addedUpdate.UpdatedAt == default)
                    {
                        addedUpdate.AuditUpdate(auditIdentity, timestamp);
                    }

                    break;

                case EntityState.Modified:
                    if (entry.Entity is ISetUpdated modified)
                    {
                        modified.AuditUpdate(auditIdentity, timestamp);
                    }
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISetDeleted deleted)
                    {
                        deleted.AuditDelete(auditIdentity, timestamp);
                    }

                    break;
            }
        }
    }
}
