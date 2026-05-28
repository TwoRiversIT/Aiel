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

using Microsoft.Extensions.DependencyInjection;
using Aiel.Execution;
using Aiel.Results;

namespace Aiel.Commands;

public sealed class DefaultCommandDispatcherTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<ICommandDispatcher, DefaultCommandDispatcher>();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    // -----------------------------------------------------------------------
    // Happy-path dispatch
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WhenHandlerSucceeds_ReturnsSuccess()
    {
        var provider = BuildProvider(s =>
            s.AddScoped<ICommandHandler<TestCommand>, SucceedingHandler>());

        var dispatcher = provider.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.DispatchAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerFails_PropagatesFailure()
    {
        var provider = BuildProvider(s =>
            s.AddScoped<ICommandHandler<TestCommand>, FailingHandler>());

        var dispatcher = provider.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.DispatchAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Child execution context
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_CreatesChildContext_CausationIdEqualsParentOperationId()
    {
        IExecutionContext? capturedContext = null;
        var provider = BuildProvider(s =>
            s.AddScoped<ICommandHandler<TestCommand>>(
                _ => new ContextCapturingHandler(ctx => capturedContext = ctx)));

        var actor = new TestActor();
        var parentContext = DefaultExecutionContext.CreateRoot(actor);
        var dispatcher = provider.GetRequiredService<ICommandDispatcher>();

        await dispatcher.DispatchAsync(
            new TestCommand(),
            parentContext,
            TestContext.Current.CancellationToken);

        capturedContext.Should().NotBeNull();
        capturedContext!.Actor.Should().BeSameAs(parentContext.Actor,
            "the child must preserve the parent actor");
        capturedContext!.CausationId.Should().Be(parentContext.OperationId,
            "the child's CausationId must equal the parent's OperationId");
        capturedContext.CorrelationId.Should().Be(parentContext.CorrelationId,
            "the correlation chain must be preserved across child contexts");
        capturedContext.OperationId.Should().NotBe(parentContext.OperationId,
            "each dispatch must receive its own unique OperationId");
    }

    // -----------------------------------------------------------------------
    // Pipeline — zero behaviors
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithNoBehaviors_InvokesHandlerDirectly()
    {
        var handler = new InvocationTrackingHandler();
        var provider = BuildProvider(s =>
            s.AddScoped<ICommandHandler<TestCommand>>(_ => handler));

        await provider
            .GetRequiredService<ICommandDispatcher>()
            .DispatchAsync(
                new TestCommand(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        handler.WasInvoked.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // Pipeline — ordering
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithMultipleBehaviors_ExecutesInRegistrationOrder()
    {
        var callOrder = new List<String>();

        var provider = BuildProvider(s =>
        {
            s.AddScoped<ICommandHandler<TestCommand>>(
                _ => new RecordingHandler(callOrder));
            s.AddTransient<ICommandPipelineBehavior<TestCommand>>(
                _ => new RecordingBehavior("B1", callOrder));
            s.AddTransient<ICommandPipelineBehavior<TestCommand>>(
                _ => new RecordingBehavior("B2", callOrder));
        });

        await provider
            .GetRequiredService<ICommandDispatcher>()
            .DispatchAsync(
                new TestCommand(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        callOrder.Should().Equal(
            "B1:before", "B2:before", "handler", "B2:after", "B1:after");
    }

    // -----------------------------------------------------------------------
    // Test stubs
    // -----------------------------------------------------------------------

    private record TestCommand : ICommand;

    private sealed class TestActor : IActor;

    private sealed class SucceedingHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> HandleAsync(
            TestCommand command, IExecutionContext context, CancellationToken ct = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FailingHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> HandleAsync(
            TestCommand command, IExecutionContext context, CancellationToken ct = default)
            => Task.FromResult(Result.Failure(new TestError()));
    }

    private sealed class ContextCapturingHandler(Action<IExecutionContext> capture)
        : ICommandHandler<TestCommand>
    {
        public Task<Result> HandleAsync(
            TestCommand command, IExecutionContext context, CancellationToken ct = default)
        {
            capture(context);
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class InvocationTrackingHandler : ICommandHandler<TestCommand>
    {
        public Boolean WasInvoked { get; private set; }

        public Task<Result> HandleAsync(
            TestCommand command, IExecutionContext context, CancellationToken ct = default)
        {
            WasInvoked = true;
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class RecordingHandler(List<String> callOrder)
        : ICommandHandler<TestCommand>
    {
        public Task<Result> HandleAsync(
            TestCommand command, IExecutionContext context, CancellationToken ct = default)
        {
            callOrder.Add("handler");
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class RecordingBehavior(String name, List<String> callOrder)
        : ICommandPipelineBehavior<TestCommand>
    {
        public async Task<Result> HandleAsync(
            TestCommand command,
            IExecutionContext context,
            CommandPipelineHandlerDelegate next,
            CancellationToken cancellationToken = default)
        {
            callOrder.Add($"{name}:before");
            var result = await next(cancellationToken);
            callOrder.Add($"{name}:after");
            return result;
        }
    }

    private sealed class TestErrorCode : ErrorCode
    {
        protected override String Name => "TEST_ERROR";
    }

    private sealed class TestError() : Error(new TestErrorCode(), "Test error");
}
