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
using Aiel.Testing.Dummies;

namespace Aiel.Domain.Aggregates;

public class EventSourcedAggregateRootTests
{
    [Fact]
    public void Create_Should_Raise_Created_Event()
    {
        var ticket = TypicalESRoot.Create(TypicalId.From(Guid.NewGuid()));

        ticket.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TicketCreatedEvent>();
    }

    private class TypicalESRoot : EventSourcedAggregateRoot<TypicalId>
    {
        protected override void Apply(IDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case TicketCreatedEvent e:
                    Apply(e);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"No Apply() handler for '{domainEvent.GetType().Name}'.");
            }
        }

        private void Apply(TicketCreatedEvent e)
        {
            Id = e.Id;
        }

        public static TypicalESRoot Create(TypicalId id)
        {
            var ticket = new TypicalESRoot();

            ticket.AddEvent(new TicketCreatedEvent(id));

            return ticket;
        }
    }

    internal record TicketCreatedEvent(TypicalId Id) : IDomainEvent
    {
        public EventId EventId { get; }
        public DateTimeOffset OccurredOn { get; }
        public String EventType => "TicketCreatedEvent";
    }
}
