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

using Aiel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Actions.Queries;

public sealed class DefaultQueryDispatcherTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<IQueryDispatcher, DefaultQueryDispatcher>();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    // -----------------------------------------------------------------------
    // Happy-path dispatch
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WhenHandlerSucceeds_ReturnsSuccessWithValue()
    {
        var provider = BuildProvider(s =>
            s.AddScoped<IQueryHandler<TestQuery, String>, SucceedingHandler>());

        var dispatcher = provider.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.DispatchAsync<TestQuery, String>(
            new TestQuery(),
            DefaultExecutionContext.CreateRoot(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerFails_PropagatesFailure()
    {
        var provider = BuildProvider(s =>
            s.AddScoped<IQueryHandler<TestQuery, String>, FailingHandler>());

        var dispatcher = provider.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.DispatchAsync<TestQuery, String>(
            new TestQuery(),
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
            s.AddScoped<IQueryHandler<TestQuery, String>>(
                _ => new ContextCapturingHandler(ctx => capturedContext = ctx)));

        var actor = new TestActor();
        var parentContext = DefaultExecutionContext.CreateRoot(actor);
        var dispatcher = provider.GetRequiredService<IQueryDispatcher>();

        await dispatcher.DispatchAsync<TestQuery, String>(
            new TestQuery(),
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
    // Pipeline — ordering
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithMultipleBehaviors_ExecutesInRegistrationOrder()
    {
        var callOrder = new List<String>();

        var provider = BuildProvider(s =>
        {
            s.AddScoped<IQueryHandler<TestQuery, String>>(
                _ => new RecordingHandler(callOrder));
            s.AddTransient<IQueryPipelineBehavior<TestQuery, String>>(
                _ => new RecordingBehavior("B1", callOrder));
            s.AddTransient<IQueryPipelineBehavior<TestQuery, String>>(
                _ => new RecordingBehavior("B2", callOrder));
        });

        await provider
            .GetRequiredService<IQueryDispatcher>()
            .DispatchAsync<TestQuery, String>(
                new TestQuery(),
                DefaultExecutionContext.CreateRoot(),
                TestContext.Current.CancellationToken);

        callOrder.Should().Equal(
            "B1:before", "B2:before", "handler", "B2:after", "B1:after");
    }

    // -----------------------------------------------------------------------
    // Test stubs
    // -----------------------------------------------------------------------

    private record TestQuery : IQuery<String>;

    private sealed class TestActor : IActor;

    private sealed class SucceedingHandler : IQueryHandler<TestQuery, String>
    {
        public Task<Result<String>> HandleAsync(
            TestQuery query, IExecutionContext context, CancellationToken ct = default)
            => Task.FromResult(Result<String>.Success("ok"));
    }

    private sealed class FailingHandler : IQueryHandler<TestQuery, String>
    {
        public Task<Result<String>> HandleAsync(
            TestQuery query, IExecutionContext context, CancellationToken ct = default)
            => Task.FromResult(Result<String>.Failure(new TestError("I do not know what I want to eat.")));
    }

    private sealed class ContextCapturingHandler(Action<IExecutionContext> capture)
        : IQueryHandler<TestQuery, String>
    {
        public Task<Result<String>> HandleAsync(
            TestQuery query, IExecutionContext context, CancellationToken ct = default)
        {
            capture(context);
            return Task.FromResult(Result<String>.Success("ok"));
        }
    }

    private sealed class RecordingHandler(List<String> callOrder)
        : IQueryHandler<TestQuery, String>
    {
        public Task<Result<String>> HandleAsync(
            TestQuery query, IExecutionContext context, CancellationToken ct = default)
        {
            callOrder.Add("handler");
            return Task.FromResult(Result<String>.Success("ok"));
        }
    }

    private sealed class RecordingBehavior(String name, List<String> callOrder)
        : IQueryPipelineBehavior<TestQuery, String>
    {
        public async Task<Result<String>> HandleAsync(
            TestQuery query,
            IExecutionContext context,
            QueryPipelineHandlerDelegate<String> next,
            CancellationToken cancellationToken = default)
        {
            callOrder.Add($"{name}:before");
            var result = await next(cancellationToken);
            callOrder.Add($"{name}:after");
            return result;
        }
    }
}
