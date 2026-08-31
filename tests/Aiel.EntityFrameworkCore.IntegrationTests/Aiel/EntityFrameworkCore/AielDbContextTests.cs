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

public class AielDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_persists_domain_events_before_clearing_them()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var dbContext = CreateDomainEventDbContext(Guid.NewGuid().ToString("N"));
        var aggregate = new TestAggregate();
        aggregate.RecordChange();

        dbContext.TrackAggregate(aggregate);

        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.PersistedDomainEvents.Should().ContainSingle();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_stamps_audit_fields_for_new_entities()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databaseName = Guid.NewGuid().ToString("N");
        var timestamp = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var context = DefaultExecutionContext.CreateRoot(new AuditTestActor("counsellor:alpha"), timestamp: timestamp);

        await using var dbContext = CreateAuditedDbContext(databaseName, context);

        var entity = new AuditedNote { Id = Guid.NewGuid(), Name = "alpha" };
        dbContext.Notes.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        entity.CreatedAt.Should().Be(timestamp);
        entity.UpdatedAt.Should().Be(timestamp);
        entity.CreatedBy.Should().Be("counsellor:alpha");
        entity.UpdatedBy.Should().Be("counsellor:alpha");
    }

    [Fact]
    public async Task SaveChangesAsync_updates_modified_audit_fields_for_existing_entities()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databaseName = Guid.NewGuid().ToString("N");
        var createdAt = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var createContext = DefaultExecutionContext.CreateRoot(new AuditTestActor("counsellor:alpha"), timestamp: createdAt);
        var updatedContext = DefaultExecutionContext.CreateRoot(new AuditTestActor("counsellor:beta"), timestamp: updatedAt);

        Guid entityId;

        await using (var seedContext = CreateAuditedDbContext(databaseName, createContext))
        {
            var seeded = new AuditedNote { Id = Guid.NewGuid(), Name = "before" };
            seedContext.Notes.Add(seeded);
            await seedContext.SaveChangesAsync(cancellationToken);

            entityId = seeded.Id;
        }

        await using var dbContext = CreateAuditedDbContext(databaseName, updatedContext);
        var entity = await dbContext.Notes.SingleAsync(static note => note.Name == "before", cancellationToken);
        entity.Name = "after";

        await dbContext.SaveChangesAsync(cancellationToken);

        entity.Id.Should().Be(entityId);
        entity.CreatedAt.Should().Be(createdAt);
        entity.CreatedBy.Should().Be("counsellor:alpha");
        entity.UpdatedAt.Should().BeOnOrAfter(updatedAt);
        entity.UpdatedBy.Should().Be("counsellor:beta");
    }

    private static DomainEventDbContext CreateDomainEventDbContext(String databaseName)
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new DomainEventDbContext(options);
    }

    private static AuditedDbContext CreateAuditedDbContext(String databaseName, IExecutionContext executionContext)
    {
        var options = new DbContextOptionsBuilder<AuditedDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AuditedDbContext(options, executionContext);
    }

    private sealed class DomainEventDbContext(DbContextOptions<DomainEventDbContext> options)
        : AielDbContext(options)
    {
        private readonly List<IAggregateRoot> _trackedAggregates = [];

        public List<IDomainEvent> PersistedDomainEvents { get; } = [];

        public void TrackAggregate(IAggregateRoot aggregateRoot)
        {
            _trackedAggregates.Add(aggregateRoot);
        }

        protected override IEnumerable<IAggregateRoot> GetTrackedAggregates() => _trackedAggregates;

        protected override ValueTask PersistDomainEventsAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
        {
            PersistedDomainEvents.AddRange(domainEvents);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AuditedDbContext(DbContextOptions<AuditedDbContext> options, IExecutionContext executionContext)
        : AielDbContext(options, executionContext)
    {
        public DbSet<AuditedNote> Notes => Set<AuditedNote>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditedNote>().HasKey(static note => note.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class AuditedNote : ISetAudited
    {
        public Guid Id { get; set; }
        public String Name { get; set; } = String.Empty;

        public DateTimeOffset CreatedAt { get; private set; }
        public String CreatedBy { get; private set; } = String.Empty;

        public DateTimeOffset UpdatedAt { get; private set; }
        public String UpdatedBy { get; private set; } = String.Empty;

        DateTimeOffset ICreated.CreatedAt => CreatedAt;
        String ICreated.CreatedBy => CreatedBy;
        DateTimeOffset IUpdated.UpdatedAt => UpdatedAt;
        String IUpdated.UpdatedBy => UpdatedBy;

        void ISetCreated.SetCreated(String createdBy, DateTimeOffset createdAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);
            ArgumentOutOfRangeException.ThrowIfLessThan(createdAt, DateTimeOffset.UnixEpoch);
            CreatedAt = createdAt;
            CreatedBy = createdBy;
        }

        void ISetUpdated.SetUpdated(String updatedBy, DateTimeOffset updatedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);
            ArgumentOutOfRangeException.ThrowIfLessThan(updatedAt, DateTimeOffset.UnixEpoch);
            UpdatedAt = updatedAt;
            UpdatedBy = updatedBy;
        }
    }

    private sealed record AuditTestActor(String AuditIdentity) : IActor;

    private sealed class TestAggregate : IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

        public void RecordChange()
        {
            _domainEvents.Add(new TestDomainEvent());
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    private sealed record TestDomainEvent(Guid EventId, DateTimeOffset OccurredOn, String EventType) : IDomainEvent
    {
        public TestDomainEvent()
            : this(Guid.NewGuid(), DateTimeOffset.UtcNow, nameof(TestDomainEvent))
        {
        }
    }
}
