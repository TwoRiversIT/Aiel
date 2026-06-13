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

namespace Aiel.Actions.Commands;

public sealed class UnitOfWorkCommandPipelineBehaviorTests
{
    // -----------------------------------------------------------------------
    // SaveChanges on success
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenHandlerSucceeds_CallsSaveChanges()
    {
        var uow = new SpyUnitOfWork();
        var behavior = new UnitOfWorkCommandPipelineBehavior<TestCommand>(uow);

        await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result.Success()),
            TestContext.Current.CancellationToken);

        uow.SaveChangesCalled.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // No SaveChanges on failure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenHandlerFails_DoesNotCallSaveChanges()
    {
        var uow = new SpyUnitOfWork();
        var behavior = new UnitOfWorkCommandPipelineBehavior<TestCommand>(uow);

        await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result.Failure(new TestError("I do not know what I want to eat."))),
            TestContext.Current.CancellationToken);

        uow.SaveChangesCalled.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Result pass-through
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_PassesThroughSuccessResult()
    {
        var behavior = new UnitOfWorkCommandPipelineBehavior<TestCommand>(new SpyUnitOfWork());

        var result = await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result.Success()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_PassesThroughFailureResult()
    {
        var behavior = new UnitOfWorkCommandPipelineBehavior<TestCommand>(new SpyUnitOfWork());

        var result = await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(),
            _ => Task.FromResult(Result.Failure(new TestError("I do not know what I want to eat."))),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Test stubs
    // -----------------------------------------------------------------------

    private record TestCommand : ICommand;

    private sealed class SpyUnitOfWork : IUnitOfWork
    {
        public Boolean SaveChangesCalled { get; private set; }

        public Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.FromResult(0);
        }
    }
}
