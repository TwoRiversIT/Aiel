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

/// <summary>
/// A factory that creates <see cref="IExecutionContext"/> instances for use in tests.
/// </summary>
/// <remarks>
/// Each call to <see cref="CreateRoot"/> produces a context with a new operation ID and correlation ID,
/// matching the behaviour of <see cref="DefaultExecutionContext.CreateRoot"/>.
/// </remarks>
public static class FakeExecutionContextFactory
{
    /// <summary>
    /// Creates a root <see cref="IExecutionContext"/> for the specified <paramref name="actor"/>.
    /// When <paramref name="actor"/> is <see langword="null"/>, a <see cref="FakeActor"/> is used.
    /// </summary>
    /// <param name="actor">
    /// The actor to associate with the execution context.
    /// Defaults to a new <see cref="FakeActor"/> when <see langword="null"/>.
    /// </param>
    public static IExecutionContext CreateRoot(IActor? actor = null)
        => DefaultExecutionContext.CreateRoot(actor ?? new FakeActor());
}
