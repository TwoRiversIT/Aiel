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
using Aiel.Results;
using Microsoft.Extensions.Logging;

namespace Aiel.Mediator.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task HandleAsync_logs_before_and_after_successful_execution()
    {
        var logger = new ListLogger<LoggingBehavior<TestAction>>();
        var behavior = new LoggingBehavior<TestAction>(logger);

        var result = await behavior.HandleAsync(
            new TestAction(),
            () => ValueTask.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().HaveCount(2);
        logger.Entries[0].Level.Should().Be(LogLevel.Information);
        logger.Entries[0].Message.Should().Contain("Handling");
        logger.Entries[0].Message.Should().Contain(nameof(TestAction));
        logger.Entries[1].Level.Should().Be(LogLevel.Information);
        logger.Entries[1].Message.Should().Contain("Handled");
        logger.Entries[1].Message.Should().Contain(nameof(TestAction));
        logger.Entries[1].Message.Should().Contain("ms");
    }

    [Fact]
    public async Task HandleAsync_logs_error_and_rethrows_when_handler_throws()
    {
        var logger = new ListLogger<LoggingBehavior<TestAction>>();
        var behavior = new LoggingBehavior<TestAction>(logger);
        var exception = new InvalidOperationException("boom");

        Func<Task> act = async () => await behavior.HandleAsync(
            new TestAction(),
            () => ValueTask.FromException<Result>(exception),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("boom");

        logger.Entries.Should().HaveCount(2);
        logger.Entries[0].Level.Should().Be(LogLevel.Information);
        logger.Entries[1].Level.Should().Be(LogLevel.Error);
        logger.Entries[1].Exception.Should().BeSameAs(exception);
        logger.Entries[1].Message.Should().Contain(nameof(TestAction));
        logger.Entries[1].Message.Should().Contain("threw");
    }

    private sealed record TestAction : IAction;

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public Boolean IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, String> formatter)
            => Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }

    private sealed record LogEntry(LogLevel Level, String Message, Exception? Exception);
}
