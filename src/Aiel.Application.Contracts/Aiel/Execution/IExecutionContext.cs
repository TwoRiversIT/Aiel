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

namespace Aiel.Execution;

/// <summary>
/// Represents the ambient context for a single application execution.
/// </summary>
public interface IExecutionContext
{
    /// <summary>Gets the unique identifier for this specific operation.</summary>
    Guid OperationId { get; }

    /// <summary>Gets the actor responsible for the execution chain.</summary>
    IActor Actor { get; }

    /// <summary>Gets the identifier that groups all operations in the same logical request chain.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the parent operation identifier that caused this one, if any.</summary>
    Guid? CausationId { get; }

    /// <summary>Gets an optional identifier for the client instance that originated the request chain.</summary>
    Guid? ClientInstanceId { get; }

    /// <summary>Gets the mutable property bag for this execution.</summary>
    IDictionary<String, Object?> Properties { get; }
}
