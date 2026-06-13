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

/// <summary>
/// Base class for all saga state bags. State is a pure data holder with no behavior
/// other than <see cref="MarkComplete"/>. Orchestration logic lives in the implementing
/// handler class, not here. Persistence is managed exclusively by <see cref="ISagaRepository{TSagaState}"/>.
/// </summary>
public abstract class SagaState
{
    /// <summary>Gets the unique identifier for this saga instance.</summary>
    public SagaId SagaId { get; internal set; }

    /// <summary>
    /// Gets whether this saga has completed. When <see langword="true"/> after handler
    /// execution, the runtime deletes the persisted state entry.
    /// </summary>
    public Boolean IsCompleted { get; private set; }

    /// <summary>
    /// Signals that the saga has completed. Call from within an
    /// <see cref="ISagaMessageHandler{TSagaState, TMessage}"/> implementation when all
    /// workflow steps are done.
    /// </summary>
    protected internal void MarkComplete() => IsCompleted = true;
}
