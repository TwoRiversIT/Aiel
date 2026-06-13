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
using Aiel.Actions.Commands;
using Aiel.Actions.Queries;
using Aiel.Authorization;
using Aiel.Results;
using System.Collections.Concurrent;

namespace Aiel.Mediator;

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record MissingHandlerCommand : ICommand;

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record MissingHandlerQuery : IQuery<String>;

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record ScopedResolutionCommand : ICommand;

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed class ScopedResolutionCommandHandler : ICommandHandler<ScopedResolutionCommand>
{
    public ValueTask<Result> HandleAsync(ScopedResolutionCommand action, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Result.Success());
}

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record ScopedResolutionQuery : IQuery<String>;

public sealed class ScopedResolutionQueryHandler : IQueryHandler<ScopedResolutionQuery, String>
{
    public ValueTask<Result> HandleAsync(ScopedResolutionQuery action, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<Result>(Result.Success("scanned"));
}

public sealed record ScopedResolutionNotification : INotification;

public sealed class ScopedResolutionNotificationHandler : INotificationHandler<ScopedResolutionNotification>
{
    public ValueTask HandleAsync(ScopedResolutionNotification notification, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

public sealed record FaultingNotification : INotification;

public sealed class FaultingNotificationHandler : INotificationHandler<FaultingNotification>
{
    public ValueTask HandleAsync(FaultingNotification notification, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("handler fault");
}

public sealed class NonFaultingNotificationHandler(NotificationExecutionTracker tracker)
    : INotificationHandler<FaultingNotification>
{
    public ValueTask HandleAsync(FaultingNotification notification, CancellationToken cancellationToken = default)
        => tracker.RecordAsync(cancellationToken);
}

public sealed class NotABehavior<T> { }

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record OrderedCommand : ICommand;

public sealed class OrderedCommandHandler(ConcurrentQueue<String> callLog) : ICommandHandler<OrderedCommand>
{
    public ValueTask<Result> HandleAsync(OrderedCommand action, CancellationToken cancellationToken = default)
    {
        callLog.Enqueue("handler");
        return ValueTask.FromResult(Result.Success());
    }
}

public sealed class OuterRecordingBehavior<TAction>(ConcurrentQueue<String> callLog)
    : IPipelineBehavior<TAction>
    where TAction : IAction
{
    public async ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        callLog.Enqueue("outer:before");
        var result = await next();
        callLog.Enqueue("outer:after");
        return result;
    }
}

public sealed class InnerRecordingBehavior<TAction>(ConcurrentQueue<String> callLog)
    : IPipelineBehavior<TAction>
    where TAction : IAction
{
    public async ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        callLog.Enqueue("inner:before");
        var result = await next();
        callLog.Enqueue("inner:after");
        return result;
    }
}

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record SuccessfulQuery(String Value) : IQuery<String>;

public sealed class SuccessfulQueryHandler : IQueryHandler<SuccessfulQuery, String>
{
    public ValueTask<Result> HandleAsync(SuccessfulQuery action, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<Result>(Result.Success(action.Value));
}

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record ShortCircuitedQuery(String Value) : IQuery<String>;

public sealed class ShortCircuitedQueryHandler : IQueryHandler<ShortCircuitedQuery, String>
{
    public ValueTask<Result> HandleAsync(ShortCircuitedQuery action, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<Result>(Result.Success(action.Value));
}

public sealed class ShortCircuitFailureBehavior<TAction> : IPipelineBehavior<TAction>
    where TAction : IAction
{
    public ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Result.Failure(new TestError("Test error")));
}

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record ScopeTrackingCommand : ICommand;

public sealed class ScopeTrackingCommandHandler(
    DispatchScopeMarker scopeMarker,
    ConcurrentQueue<Guid> seenScopeIds) : ICommandHandler<ScopeTrackingCommand>
{
    public ValueTask<Result> HandleAsync(ScopeTrackingCommand action, CancellationToken cancellationToken = default)
    {
        seenScopeIds.Enqueue(scopeMarker.Id);
        return ValueTask.FromResult(Result.Success());
    }
}

public sealed class DispatchScopeMarker(Guid id)
{
    public Guid Id { get; } = id;
}

public sealed record UnhandledNotification : INotification;

public sealed record ParallelismProbeNotification : INotification;

public sealed class FirstParallelismProbeHandler(NotificationExecutionTracker tracker)
    : INotificationHandler<ParallelismProbeNotification>
{
    public ValueTask HandleAsync(ParallelismProbeNotification notification, CancellationToken cancellationToken = default)
        => tracker.RecordAsync(cancellationToken);
}

public sealed class SecondParallelismProbeHandler(NotificationExecutionTracker tracker)
    : INotificationHandler<ParallelismProbeNotification>
{
    public ValueTask HandleAsync(ParallelismProbeNotification notification, CancellationToken cancellationToken = default)
        => tracker.RecordAsync(cancellationToken);
}

public sealed record OrderedNotification : INotification;

public sealed class FirstOrderedNotificationHandler(ConcurrentQueue<String> callLog)
    : INotificationHandler<OrderedNotification>
{
    public ValueTask HandleAsync(OrderedNotification notification, CancellationToken cancellationToken = default)
    {
        callLog.Enqueue("first");
        return ValueTask.CompletedTask;
    }
}

public sealed class SecondOrderedNotificationHandler(ConcurrentQueue<String> callLog)
    : INotificationHandler<OrderedNotification>
{
    public ValueTask HandleAsync(OrderedNotification notification, CancellationToken cancellationToken = default)
    {
        callLog.Enqueue("second");
        return ValueTask.CompletedTask;
    }
}

[DoesNotRespectAuthority(Reason = "Test actions are permission-free")]
public sealed record DisposalTrackingCommand : ICommand;

public sealed class DisposalTrackingCommandHandler(
    DisposableScopeMarker scopeMarker,
    ScopeDisposalTracker tracker) : ICommandHandler<DisposalTrackingCommand>
{
    public ValueTask<Result> HandleAsync(DisposalTrackingCommand action, CancellationToken cancellationToken = default)
    {
        tracker.RecordUse(scopeMarker.Id);
        return ValueTask.FromResult(Result.Success());
    }
}

public sealed record DisposalTrackingNotification : INotification;

public sealed class DisposalTrackingNotificationHandler(
    DisposableScopeMarker scopeMarker,
    ScopeDisposalTracker tracker) : INotificationHandler<DisposalTrackingNotification>
{
    public ValueTask HandleAsync(DisposalTrackingNotification notification, CancellationToken cancellationToken = default)
    {
        tracker.RecordUse(scopeMarker.Id);
        return ValueTask.CompletedTask;
    }
}

public sealed class NotificationExecutionTracker
{
    private Int32 _activeHandlers;
    private Int32 _maxConcurrentHandlers;
    private Int32 _invocationCount;

    public Int32 InvocationCount => _invocationCount;
    public Int32 MaxConcurrentHandlers => _maxConcurrentHandlers;

    public async ValueTask RecordAsync(CancellationToken cancellationToken = default)
    {
        var active = Interlocked.Increment(ref _activeHandlers);
        Interlocked.Increment(ref _invocationCount);
        UpdateMaxConcurrency(active);

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        Interlocked.Decrement(ref _activeHandlers);
    }

    private void UpdateMaxConcurrency(Int32 active)
    {
        while (true)
        {
            var current = _maxConcurrentHandlers;
            if (active <= current)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _maxConcurrentHandlers, active, current) == current)
            {
                return;
            }
        }
    }
}

public sealed class ScopeDisposalTracker
{
    public ConcurrentQueue<Guid> UsedIds { get; } = [];
    public ConcurrentQueue<Guid> DisposedIds { get; } = [];

    public void RecordUse(Guid id) => UsedIds.Enqueue(id);

    public void RecordDispose(Guid id) => DisposedIds.Enqueue(id);
}

public sealed class DisposableScopeMarker(Guid id, ScopeDisposalTracker tracker)
    : IAsyncDisposable, IDisposable
{
    private Int32 _disposed;

    public Guid Id { get; } = id;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            tracker.RecordDispose(Id);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
