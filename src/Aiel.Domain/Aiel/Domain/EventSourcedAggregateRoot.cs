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

/// <summary>
/// Represents an aggregate root that is event-sourced, meaning its state is derived from a sequence of domain events.
/// </summary>
/// <typeparam name="TKey">The type of the strongly-typed identifier.</typeparam>
public abstract class EventSourcedAggregateRoot<TKey> : AggregateRoot<TKey>, IRehydrateFromHistory
    where TKey : notnull, IStrongId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcedAggregateRoot{TKey}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the aggregate root.</param>
    protected EventSourcedAggregateRoot(TKey id)
        : base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcedAggregateRoot{TKey}"/> class.
    /// </summary>
    protected EventSourcedAggregateRoot()
    {
    }

    /// <summary>
    /// Applies the specified domain event to the aggregate root, updating its state accordingly.
    /// </summary>
    /// <param name="domainEvent"></param>
    protected abstract void Apply(IDomainEvent domainEvent);

    /// <summary>
    /// Raises the specified domain event, applying it to the aggregate root and incrementing the version.
    /// </summary>
    /// <param name="domainEvent"></param>
    protected override void OnRaiseEvent(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        Version++;
    }

    /// <inheritdoc/>
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
