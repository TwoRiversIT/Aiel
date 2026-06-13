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

using Aiel.Domain.Events;
using Aiel.EventSourcing;
using Aiel.StrongIds;

namespace Aiel.Domain;

public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    where TKey : notnull, IStrongId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot(TKey id)
        : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        OnRaiseEvent(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    protected virtual void OnRaiseEvent(IDomainEvent domainEvent)
    {
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class StateBasedAggregateRoot<TKey> : AggregateRoot<TKey>
    where TKey : notnull, IStrongId
{
    protected StateBasedAggregateRoot(TKey id)
        : base(id)
    {
    }

    protected StateBasedAggregateRoot()
    {
    }
}

public abstract class EventSourcedAggregateRoot<TKey> : AggregateRoot<TKey>, IRehydrateFromHistory
    where TKey : notnull, IStrongId
{
    protected EventSourcedAggregateRoot(TKey id)
        : base(id)
    {
    }

    protected EventSourcedAggregateRoot()
    {
    }

    protected abstract void Apply(IDomainEvent domainEvent);

    protected override void OnRaiseEvent(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        Version++;
    }

    void IRehydrateFromHistory.RehydrateFromHistory(IEnumerable<IDomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        foreach (var domainEvent in history)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            Apply(domainEvent);
            Version++;
        }
    }
}

