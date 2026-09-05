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

using Aiel.Domain.Aggregates;
using Aiel.Domain.Events;
using Aiel.StrongIds;

namespace Aiel.Domain;

/// <summary>
/// Represents the base class for aggregate roots in the domain-driven design
/// context. An aggregate root is an entity that serves as the entry point
/// for a cluster of related entities and ensures the consistency of changes
/// within that cluster. This class provides functionality for managing
/// domain events associated with the aggregate root.
/// </summary>
/// <typeparam name="TKey">a strongly typed identifier for the aggregate root.</typeparam>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    where TKey : notnull, IStrongId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TKey}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id"></param>
    protected AggregateRoot(TKey id)
        : base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TKey}"/> class.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Raises a domain event and adds it to the list of domain events associated with the aggregate root.
    /// </summary>
    /// <param name="domainEvent"></param>
    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        OnRaiseEvent(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Called when a domain event is raised. This method can be overridden in derived classes to perform additional actions when a domain event is raised.
    /// </summary>
    /// <param name="domainEvent">The domain event that was raised.</param>
    protected virtual void OnRaiseEvent(IDomainEvent domainEvent)
    {
    }

    /// <summary>
    /// Clears all domain events from the aggregate root.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
