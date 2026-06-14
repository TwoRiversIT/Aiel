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

namespace Aiel.Actions;

public sealed class ActionExecutionContextTests
{
    [Fact]
    public void CreateChild_PreservesActorCorrelationCausationPropertiesAndActionPayload()
    {
        var actor = new TestActor();
        var parent = new TestExecutionContext(
            operationId: Guid.NewGuid(),
            actor: actor,
            correlationId: Guid.NewGuid(),
            causationId: Guid.NewGuid(),
            clientInstanceId: Guid.NewGuid());
        var action = new TestAction("reschedule-appointment");

        parent.Properties["request-source"] = "api";
        parent.Properties["attempt"] = 3;

        var child = ActionExecutionContext<TestAction>.CreateChild(parent, action);

        child.Actor.Should().BeSameAs(parent.Actor);
        child.CorrelationId.Should().Be(parent.CorrelationId);
        child.CausationId.Should().Be(parent.OperationId);
        child.Action.Should().Be(action);
        child.Properties.Should().ContainKey("request-source");
        child.Properties["request-source"].Should().Be("api");
        child.Properties.Should().ContainKey("attempt");
        child.Properties["attempt"].Should().Be(3);
    }

    private sealed record TestAction(String Name) : IAction;

    private sealed class TestActor : IActor;

    private sealed class TestExecutionContext(
        Guid operationId,
        IActor actor,
        Guid correlationId,
        Guid? causationId,
        Guid? clientInstanceId) : IExecutionContext
    {
        public Guid OperationId { get; } = operationId;

        public IActor Actor { get; } = actor;

        public Guid CorrelationId { get; } = correlationId;

        public Guid? CausationId { get; } = causationId;

        public Guid? ClientInstanceId { get; } = clientInstanceId;

        public IDictionary<String, Object?> Properties { get; } = new Dictionary<String, Object?>();
    }
}
