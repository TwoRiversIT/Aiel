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

namespace Aiel.Authorization.Testing;

public sealed class FakeExecutionContextFactoryTests
{
    [Fact]
    public void CreateRoot_ReturnsNonNullContext()
    {
        var context = FakeExecutionContextFactory.CreateRoot();
        context.Should().NotBeNull();
    }

    [Fact]
    public void CreateRoot_ContextActorIsNonNull()
    {
        var context = FakeExecutionContextFactory.CreateRoot();
        context.Actor.Should().NotBeNull();
    }

    [Fact]
    public void CreateRoot_OperationIdIsNonEmpty()
    {
        var context = FakeExecutionContextFactory.CreateRoot();
        context.OperationId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateRoot_CorrelationIdIsNonEmpty()
    {
        var context = FakeExecutionContextFactory.CreateRoot();
        context.CorrelationId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateRoot_WithCustomActor_UsesProvidedActor()
    {
        var actor = new FakeActor();
        var context = FakeExecutionContextFactory.CreateRoot(actor);
        context.Actor.Should().BeSameAs(actor);
    }

    [Fact]
    public void CreateRoot_WithNoActor_UsesFakeActor()
    {
        var context = FakeExecutionContextFactory.CreateRoot();
        context.Actor.Should().BeOfType<FakeActor>();
    }

    [Fact]
    public void CreateRoot_EachCallProducesDistinctOperationId()
    {
        var contextA = FakeExecutionContextFactory.CreateRoot();
        var contextB = FakeExecutionContextFactory.CreateRoot();
        contextA.OperationId.Should().NotBe(contextB.OperationId);
    }
}
