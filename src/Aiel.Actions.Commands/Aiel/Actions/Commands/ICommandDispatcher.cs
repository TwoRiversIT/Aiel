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

using Aiel.Results;

namespace Aiel.Actions.Commands;

/// <summary>
/// Defines a contract for dispatching command objects to their corresponding handlers asynchronously.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for locating and invoking the appropriate handler
/// for a given command. The dispatcher coordinates command execution and may handle cross-cutting concerns such as
/// logging, validation, or transaction management. This interface supports both commands that do not return a result
/// and commands that return a result value.
/// </remarks>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a command to its corresponding handler asynchronously and returns a result indicating the success or failure of the operation.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to dispatch.</typeparam>
    /// <param name="command">The command instance to dispatch.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result"/> indicating the success or failure of the operation.</returns>
    Task<Result> DispatchAsync<TCommand>(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand;
}

