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

namespace Aiel.Authorization;

/// <summary>
/// Specification-level tests for <see cref="DefaultActionGate{TAction}"/> behavior.
/// </summary>
public sealed class ActionGateSpecificationTests
{
    private static readonly PermissionName DocumentsRead = PermissionName.From("documents.read");

    [Fact]
    public async Task AuthorizeAsync_WhenValidationFails_ReturnsValidationErrorAndSkipsPermissionCheck()
    {
        var callOrder = new List<String>();
        var gate = CreateGate(
            validator: new RecordingValidator(callOrder, Result.Failure(AuthorizationErrors.ValidationFailed(DocumentsRead, "required"))),
            checker: new RecordingChecker(callOrder, Result.Success()));

        var result = await gate.AuthorizeAsync(CreateContext(), new TestAction(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<AuthorizationValidationError>();
        callOrder.Should().Equal("validate");
    }

    [Fact]
    public async Task AuthorizeAsync_WhenNoCheckerRegistered_ReturnsMissingAuthorizationStoryError()
    {
        var gate = CreateGate(validator: new RecordingValidator([], Result.Success()));

        var result = await gate.AuthorizeAsync(CreateContext(), new TestAction(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<MissingAuthorizationStoryError>();
    }

    [Fact]
    public async Task AuthorizeAsync_WhenPermissionDenied_ReturnsPermissionDeniedError()
    {
        var gate = CreateGate(
            validator: new RecordingValidator([], Result.Success()),
            checker: new RecordingChecker([], Result.Failure(AuthorizationErrors.PermissionDenied(DocumentsRead))));

        var result = await gate.AuthorizeAsync(CreateContext(), new TestAction(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<AuthorizationDeniedError>();
    }

    [Fact]
    public async Task AuthorizeAsync_WhenBothPass_ReturnsBoundExecutionContextInOrder()
    {
        var callOrder = new List<String>();
        var gate = CreateGate(
            validator: new RecordingValidator(callOrder, Result.Success()),
            checker: new RecordingChecker(callOrder, Result.Success()));
        var action = new TestAction();

        var result = await gate.AuthorizeAsync(CreateContext(), action, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Action.Should().BeSameAs(action);
        callOrder.Should().Equal("validate", "check");
    }

    private static DefaultExecutionContext CreateContext()
        => DefaultExecutionContext.CreateRoot(new TestActor());

    private static DefaultActionGate<TestAction> CreateGate(
        IActionValidator<TestAction>? validator = null,
        IActionAuthorizationChecker<TestAction>? checker = null)
    {
        var serviceProvider = new TestServiceProvider();
        serviceProvider.Add(validator);
        serviceProvider.Add(checker);
        return new DefaultActionGate<TestAction>(serviceProvider);
    }

    private sealed class RecordingValidator(List<String> callOrder, Result result) : IActionValidator<TestAction>
    {
        public Task<Result> ValidateAsync(
            IActionExecutionContext<TestAction> context,
            CancellationToken cancellationToken = default)
        {
            callOrder.Add("validate");
            return Task.FromResult(result);
        }
    }

    private sealed class RecordingChecker(List<String> callOrder, Result result) : IActionAuthorizationChecker<TestAction>
    {
        public Task<Result> CheckPermissionAsync(
            IActionExecutionContext<TestAction> context,
            CancellationToken cancellationToken = default)
        {
            callOrder.Add("check");
            return Task.FromResult(result);
        }
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Object> _services = [];

        public void Add<TService>(TService? service)
            where TService : class
        {
            if (service is not null)
            {
                _services[typeof(TService)] = service;
            }
        }

        public Object? GetService(Type serviceType)
            => _services.TryGetValue(serviceType, out var service) ? service : null;
    }

    private sealed class TestAction : IAction;

    private sealed class TestActor : IActor;
}
