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

using Aiel.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Domain;

public sealed class DefaultDomainEventDispatcherTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<IDomainEventDispatcher, DefaultDomainEventDispatcher>();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    // -----------------------------------------------------------------------
    // Single event dispatch
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithRegisteredHandler_InvokesHandler()
    {
        var invoked = false;
        var provider = BuildProvider(s =>
            s.AddScoped<IDomainEventHandler<TestDomainEvent>>(
                _ => new TrackingHandler(() => invoked = true)));

        await provider
            .GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync(
                new TestDomainEvent(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task DispatchAsync_WithMultipleHandlers_InvokesAllInRegistrationOrder()
    {
        var callOrder = new List<String>();
        var provider = BuildProvider(s =>
        {
            s.AddScoped<IDomainEventHandler<TestDomainEvent>>(
                _ => new RecordingHandler("H1", callOrder));
            s.AddScoped<IDomainEventHandler<TestDomainEvent>>(
                _ => new RecordingHandler("H2", callOrder));
        });

        await provider
            .GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync(
                new TestDomainEvent(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        callOrder.Should().Equal("H1", "H2");
    }

    [Fact]
    public async Task DispatchAsync_WithNoHandlerRegistered_CompletesWithoutError()
    {
        var provider = BuildProvider();

        var act = () => provider
            .GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync(
                new TestDomainEvent(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerThrows_PropagatesException()
    {
        var provider = BuildProvider(s =>
            s.AddScoped<IDomainEventHandler<TestDomainEvent>, ThrowingHandler>());

        var act = () => provider
            .GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync(
                new TestDomainEvent(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // -----------------------------------------------------------------------
    // Enumerable overload
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithEnumerableOverload_DispatchesEachEventInOrder()
    {
        var callOrder = new List<String>();
        var provider = BuildProvider(s =>
            s.AddScoped<IDomainEventHandler<TestDomainEvent>>(
                _ => new RecordingHandler("H", callOrder)));

        var events = new IDomainEvent[]
        {
            new TestDomainEvent("E1"),
            new TestDomainEvent("E2"),
            new TestDomainEvent("E3"),
        };

        await provider
            .GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync(
                events,
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        callOrder.Should().Equal("H:E1", "H:E2", "H:E3");
    }

    // -----------------------------------------------------------------------
    // Test stubs
    // -----------------------------------------------------------------------

    private record TestDomainEvent(String Label = "") : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
        public String EventType => nameof(TestDomainEvent);
    }

    private sealed class TrackingHandler(Action onHandle)
        : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(
            TestDomainEvent domainEvent, IExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            onHandle();
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingHandler(String name, List<String> callOrder)
        : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(
            TestDomainEvent domainEvent, IExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            var label = String.IsNullOrEmpty(domainEvent.Label)
                ? name
                : $"{name}:{domainEvent.Label}";
            callOrder.Add(label);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(
            TestDomainEvent domainEvent, IExecutionContext context,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Handler failure");
    }
}
