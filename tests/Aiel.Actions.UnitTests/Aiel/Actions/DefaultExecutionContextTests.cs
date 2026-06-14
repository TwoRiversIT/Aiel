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

public sealed class DefaultExecutionContextTests
{
    [Fact]
    public void CreateRoot_WithoutExplicitActor_UsesSystemActor()
    {
        var context = DefaultExecutionContext.CreateRoot();

        context.Actor.Should().BeSameAs(SystemActor.Instance);
    }

    [Fact]
    public void CreateChild_PreservesActorAndIdentifiers()
    {
        var actor = new TestActor();
        var parent = DefaultExecutionContext.CreateRoot(actor);

        var child = DefaultExecutionContext.CreateChild(parent);

        child.Actor.Should().BeSameAs(actor);
        child.CorrelationId.Should().Be(parent.CorrelationId);
        child.CausationId.Should().Be(parent.OperationId);
        child.OperationId.Should().NotBe(parent.OperationId);
    }

    private sealed class TestActor : IActor;
}
