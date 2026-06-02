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
using Aiel.Commands;
using Aiel.Execution;
using Aiel.Results;

namespace Aiel.Authorization;

public sealed class ActionGateCommandPipelineBehaviorTests
{
    // -----------------------------------------------------------------------
    // [DoesNotRespectAuthority] bypass
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenCommandDoesNotRespectAuthority_BypassesGateAndCallsNext()
    {
        var gate = new StubGate<ExemptCommand>(grantAccess: false);
        var behavior = new ActionGateCommandPipelineBehavior<ExemptCommand>(gate);
        var nextWasCalled = false;

        await behavior.HandleAsync(
            new ExemptCommand(),
            DefaultExecutionContext.CreateRoot(new TestActor()),
            ct =>
            {
                nextWasCalled = true;
                return Task.FromResult(Result.Success());
            },
            TestContext.Current.CancellationToken);

        nextWasCalled.Should().BeTrue();
        gate.WasInvoked.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Authorization failure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenAuthorizationFails_ReturnsFailureWithoutCallingNext()
    {
        var gate = new StubGate<TestCommand>(grantAccess: false);
        var behavior = new ActionGateCommandPipelineBehavior<TestCommand>(gate);
        var nextWasCalled = false;

        var result = await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(new TestActor()),
            ct =>
            {
                nextWasCalled = true;
                return Task.FromResult(Result.Success());
            },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        nextWasCalled.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Authorization success
    // -----------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenAuthorizationSucceeds_CallsNextAndReturnsItsResult()
    {
        var gate = new StubGate<TestCommand>(grantAccess: true);
        var behavior = new ActionGateCommandPipelineBehavior<TestCommand>(gate);

        var result = await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(new TestActor()),
            _ => Task.FromResult(Result.Success()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        gate.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorizationSucceeds_PropagatesHandlerFailure()
    {
        var gate = new StubGate<TestCommand>(grantAccess: true);
        var behavior = new ActionGateCommandPipelineBehavior<TestCommand>(gate);

        var result = await behavior.HandleAsync(
            new TestCommand(),
            DefaultExecutionContext.CreateRoot(new TestActor()),
            _ => Task.FromResult(Result.Failure(new TestError())),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // Test stubs
    // -----------------------------------------------------------------------

    private record TestCommand : ICommand;

    [DoesNotRespectAuthority(Reason = "Test command used only in unit test infrastructure.")]
    private record ExemptCommand : ICommand;

    private sealed class TestActor : IActor;

    private sealed class StubGate<TAction>(Boolean grantAccess) : IActionGate<TAction>
        where TAction : IAction
    {
        public Boolean WasInvoked { get; private set; }

        public Task<Result<IActionExecutionContext<TAction>>> AuthorizeAsync(
            IExecutionContext context,
            TAction action,
            CancellationToken cancellationToken = default)
        {
            WasInvoked = true;

            if (!grantAccess)
            {
                return Task.FromResult(
                    Result<IActionExecutionContext<TAction>>.Failure(
                        AuthorizationErrors.PermissionDenied(PermissionName.From("test.denied"))));
            }

            IActionExecutionContext<TAction> actionContext = new StubActionContext<TAction>(context, action);
            return Task.FromResult(Result<IActionExecutionContext<TAction>>.Success(actionContext));
        }
    }

    private sealed class StubActionContext<TAction>(IExecutionContext parent, TAction action)
        : IActionExecutionContext<TAction>
        where TAction : IAction
    {
        public TAction Action => action;
        public Guid OperationId => parent.OperationId;
        public IActor Actor => parent.Actor;
        public Guid CorrelationId => parent.CorrelationId;
        public Guid? CausationId => parent.CausationId;
        public Guid? ClientInstanceId => parent.ClientInstanceId;
        public IDictionary<String, Object?> Properties => parent.Properties;
    }

    private sealed class TestErrorCode : ErrorCode
    {
        protected override String Name => "TEST_ERROR";
    }

    private sealed class TestError() : Error(new TestErrorCode(), "Test error");
}
