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

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Collections.Concurrent;

namespace Aiel.Mediator;

public class DispatcherTests
{
    private static ServiceProvider BuildProvider(
        Action<IServiceCollection>? configureServices = null,
        params Type[] behaviors)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);

        var builder = services.AddDispatcher(typeof(DispatcherBuilderTests).Assembly);
        foreach (var behavior in behaviors)
        {
            builder.WithBehavior(behavior);
        }

        builder.Build();
        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public async Task ExecuteAsync_when_action_is_null_throws()
    {
        using var provider = BuildProvider();
        var sender = provider.GetRequiredService<ISender>();

        Func<Task> act = async () => await sender.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("action");
    }

    [Fact]
    public async Task QueryAsync_when_action_is_null_throws()
    {
        using var provider = BuildProvider();
        var sender = provider.GetRequiredService<ISender>();

        Func<Task> act = async () => await sender.QueryAsync<String>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("action");
    }

    [Fact]
    public async Task PublishAsync_when_notification_is_null_throws()
    {
        using var provider = BuildProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        Func<Task> act = async () => await publisher.PublishAsync<UnhandledNotification>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("notification");
    }

    [Fact]
    public async Task ExecuteAsync_when_handler_is_missing_throws()
    {
        using var provider = BuildProvider();
        var sender = provider.GetRequiredService<ISender>();

        Func<Task> act = async () => await sender.ExecuteAsync(new MissingHandlerCommand());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered for action type*MissingHandlerCommand*");
    }

    [Fact]
    public async Task QueryAsync_when_handler_is_missing_throws()
    {
        using var provider = BuildProvider();
        var sender = provider.GetRequiredService<ISender>();

        Func<Task> act = async () => await sender.QueryAsync(new MissingHandlerQuery());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered for action type*MissingHandlerQuery*");
    }

    [Fact]
    public async Task ExecuteAsync_runs_behaviors_in_registration_order()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var callLog = new ConcurrentQueue<String>();
        using var provider = BuildProvider(
            services => services.AddSingleton(callLog),
            typeof(OuterRecordingBehavior<>),
            typeof(InnerRecordingBehavior<>));
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.ExecuteAsync(new OrderedCommand(), cancellationToken);

        result.IsSuccess.Should().BeTrue();
        callLog.Should().ContainInOrder(
            "outer:before",
            "inner:before",
            "handler",
            "inner:after",
            "outer:after");
    }

    [Fact]
    public async Task QueryAsync_returns_typed_success_value_from_handler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var provider = BuildProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.QueryAsync(new SuccessfulQuery("payload"), cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("payload");
    }

    [Fact]
    public async Task QueryAsync_promotes_plain_failure_result_from_behavior_to_typed_result()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var provider = BuildProvider(
            behaviors: [typeof(ShortCircuitFailureBehavior<>)]);
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.QueryAsync(new ShortCircuitedQuery("ignored"), cancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<TestError>();
    }

    [Fact]
    [SuppressMessage("AielUsage", "AIEL00005:Multiple mediator dispatch calls in a single method", Justification = "<Pending>")]
    public async Task ExecuteAsync_creates_a_new_scope_for_each_dispatch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var seenScopeIds = new ConcurrentQueue<Guid>();
        using var provider = BuildProvider(services =>
        {
            services.AddScoped(_ => new DispatchScopeMarker(Guid.NewGuid()));
            services.AddSingleton(seenScopeIds);
        });
        var sender = provider.GetRequiredService<ISender>();

        await sender.ExecuteAsync(new ScopeTrackingCommand(), cancellationToken);
        await sender.ExecuteAsync(new ScopeTrackingCommand(), cancellationToken);

        seenScopeIds.Should().HaveCount(2);
        seenScopeIds.Distinct().Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_disposes_scoped_services_when_dispatch_completes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tracker = new ScopeDisposalTracker();
        using var provider = BuildProvider(services =>
        {
            services.AddSingleton(tracker);
            services.AddScoped(_ => new DisposableScopeMarker(Guid.NewGuid(), tracker));
        });
        var sender = provider.GetRequiredService<ISender>();

        await sender.ExecuteAsync(new DisposalTrackingCommand(), cancellationToken);

        tracker.UsedIds.Should().ContainSingle();
        tracker.DisposedIds.Should().ContainSingle();
        tracker.DisposedIds.Should().Equal(tracker.UsedIds);
    }

    [Fact]
    public async Task PublishAsync_when_no_handler_is_registered_completes_without_error()
    {
        using var provider = BuildProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        Func<Task> act = async () => await publisher.PublishAsync(new UnhandledNotification());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_invokes_all_handlers_without_overlapping_execution()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tracker = new NotificationExecutionTracker();
        using var provider = BuildProvider(services => services.AddSingleton(tracker));
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new ParallelismProbeNotification(), cancellationToken);

        tracker.InvocationCount.Should().Be(2);
        tracker.MaxConcurrentHandlers.Should().Be(1);
    }

    [Fact]
    public async Task PublishAsync_invokes_handlers_in_registration_order()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var callLog = new ConcurrentQueue<String>();
        using var provider = BuildProvider(services =>
        {
            services.AddSingleton(callLog);
            services.AddScoped<INotificationHandler<OrderedNotification>, FirstOrderedNotificationHandler>();
            services.AddScoped<INotificationHandler<OrderedNotification>, SecondOrderedNotificationHandler>();
        });
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new OrderedNotification(), cancellationToken);

        callLog.Should().ContainInOrder("first", "second");
    }

    [Fact]
    public async Task PublishAsync_disposes_scoped_services_when_publication_completes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tracker = new ScopeDisposalTracker();
        using var provider = BuildProvider(services =>
        {
            services.AddSingleton(tracker);
            services.AddScoped(_ => new DisposableScopeMarker(Guid.NewGuid(), tracker));
        });
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new DisposalTrackingNotification(), cancellationToken);

        tracker.UsedIds.Should().ContainSingle();
        tracker.DisposedIds.Should().ContainSingle();
        tracker.DisposedIds.Should().Equal(tracker.UsedIds);
    }

    [Fact]
    public async Task PublishAsync_invokes_all_handlers_even_when_one_throws()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tracker = new NotificationExecutionTracker();
        using var provider = BuildProvider(services => services.AddSingleton(tracker));
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new FaultingNotification(), cancellationToken);

        // Both handlers run; the non-throwing one records its invocation.
        tracker.InvocationCount.Should().Be(1, "the non-faulting handler must still run");
    }

    [Fact]
    public async Task PublishAsync_logs_error_when_handler_throws()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var fakeLogger = new FakeLogger<NotificationHandlerWrapper<FaultingNotification>>();
        var tracker = new NotificationExecutionTracker();
        using var provider = BuildProvider(services => services
            .AddSingleton(tracker)
            .AddSingleton<ILogger<NotificationHandlerWrapper<FaultingNotification>>>(fakeLogger));
        var publisher = provider.GetRequiredService<IPublisher>();

        await publisher.PublishAsync(new FaultingNotification(), cancellationToken);

        fakeLogger.Collector.Count.Should().Be(1);
        fakeLogger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        fakeLogger.Collector.LatestRecord.Message.Should().Be("[HandlerException] The FaultingNotificationHandler threw an exception. See the inner exception for details.");
    }
}
