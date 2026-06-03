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

namespace Aiel.MessageBus.Sagas;

public sealed class SagaStateTests
{
    // Subclass exposes MarkComplete for testing; MarkComplete is protected internal.
    private sealed class TestSagaState : SagaState
    {
        public void Complete() => MarkComplete();
    }

    [Fact]
    public void IsCompleted_StartsFalse()
    {
        var state = new TestSagaState();

        state.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void MarkComplete_SetsIsCompletedToTrue()
    {
        var state = new TestSagaState();

        state.Complete();

        state.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void MarkComplete_IsIdempotent()
    {
        var state = new TestSagaState();

        state.Complete();
        state.Complete();

        state.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void SagaId_DefaultsToEmptyGuid()
    {
        var state = new TestSagaState();

        state.SagaId.Value.Should().Be(Guid.Empty);
    }
}
