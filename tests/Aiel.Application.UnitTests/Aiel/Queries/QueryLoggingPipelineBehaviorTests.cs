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
using Aiel.Results;
using Microsoft.Extensions.Logging;

namespace Aiel.Queries;

public sealed class QueryLoggingPipelineBehaviorTests
{
    // -----------------------------------------------------------------------
    // Before-dispatch log
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_LogsInputTypeNameAndCorrelationIdBeforeDispatch()
    {
        var logger = new ListLogger<QueryLoggingPipelineBehavior<TestQuery, String>>();
        var behavior = new QueryLoggingPipelineBehavior<TestQuery, String>(logger);
        var context = DefaultExecutionContext.CreateRoot();

        var handlerRan = false;
        Task<Result<String>> nextAsync(CancellationToken ct = default)
        {
            logger.Entries.Should().NotBeEmpty("a log entry should be written before the handler runs");
            handlerRan = true;
            return Task.FromResult(Result<String>.Success("ok"));
        }

        await behavior.HandleAsync(
            new TestQuery(), context, nextAsync, TestContext.Current.CancellationToken);

        handlerRan.Should().BeTrue();
        var beforeEntry = logger.Entries.First();
        beforeEntry.Message.Should().Contain(nameof(TestQuery));
        beforeEntry.Message.Should().Contain(context.CorrelationId.ToString());
    }

    // -----------------------------------------------------------------------
    // After-dispatch log — success
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenHandlerSucceeds_LogsSuccessOutcome()
    {
        var logger = new ListLogger<QueryLoggingPipelineBehavior<TestQuery, String>>();
        var behavior = new QueryLoggingPipelineBehavior<TestQuery, String>(logger);

        await behavior.HandleAsync(
            new TestQuery(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result<String>.Success("ok")),
            TestContext.Current.CancellationToken);

        logger.Entries.Should().HaveCount(2);
        var afterEntry = logger.Entries[1];
        afterEntry.Level.Should().Be(LogLevel.Information);
        afterEntry.Message.Should().ContainAny("success", "succeeded");
    }

    // -----------------------------------------------------------------------
    // After-dispatch log — failure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenHandlerFails_LogsFailureAtWarningLevel()
    {
        var logger = new ListLogger<QueryLoggingPipelineBehavior<TestQuery, String>>();
        var behavior = new QueryLoggingPipelineBehavior<TestQuery, String>(logger);

        await behavior.HandleAsync(
            new TestQuery(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result<String>.Failure(new TestError())),
            TestContext.Current.CancellationToken);

        logger.Entries.Should().HaveCount(2);
        var afterEntry = logger.Entries[1];
        afterEntry.Level.Should().Be(LogLevel.Warning);
        afterEntry.Message.Should().Contain("fail");
    }

    // -----------------------------------------------------------------------
    // Result pass-through
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_PassesThroughSuccessResult()
    {
        var behavior = new QueryLoggingPipelineBehavior<TestQuery, String>(
            new ListLogger<QueryLoggingPipelineBehavior<TestQuery, String>>());

        var result = await behavior.HandleAsync(
            new TestQuery(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result<String>.Success("payload")),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("payload");
    }

    [Fact]
    public async Task HandleAsync_PassesThroughFailureResult()
    {
        var behavior = new QueryLoggingPipelineBehavior<TestQuery, String>(
            new ListLogger<QueryLoggingPipelineBehavior<TestQuery, String>>());

        var result = await behavior.HandleAsync(
            new TestQuery(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result<String>.Failure(new TestError())),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Test stubs
    // -----------------------------------------------------------------------

    private record TestQuery : IQuery<String>;

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, String Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public Boolean IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, String> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }

    private sealed class TestErrorCode : ErrorCode
    {
        protected override String Name => "TEST_ERROR";
    }

    private sealed class TestError() : Error(new TestErrorCode(), "Test error");
}
