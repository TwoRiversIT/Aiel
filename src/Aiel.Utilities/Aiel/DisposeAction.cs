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

namespace Aiel;

/// <summary>
/// This class can be used to provide an action when
/// Dispose method is called.
/// </summary>
/// <remarks>
/// Creates a new <see cref="DisposeAction"/> object.
/// </remarks>
/// <param name="action">Action to be executed when this object is disposed.</param>
public class DisposeAction([NotNull] Action action) : IDisposable
{
    private readonly Action _action = action ?? throw new ArgumentNullException(nameof(action));

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "DisposeAction is an Off-Label usage of IDisposable to execute an action without finalization.")]
    public void Dispose()
    {
        _action();
    }

    public static IDisposable Create(Action action)
    {
        return new DisposeAction(action);
    }
    public static IDisposable Create<T>(Action<T> action, T parameter)
    {
        return new DisposeAction<T>(action, parameter);
    }
}

/// <summary>
/// This class can be used to provide an action when
/// Dispose method is called.
/// <typeparam name="T">The type of the parameter of the action.</typeparam>
/// </summary>
/// <remarks>
/// Creates a new <see cref="DisposeAction"/> object.
/// </remarks>
/// <param name="action">Action to be executed when this object is disposed.</param>
/// <param name="parameter">The parameter of the action.</param>
public class DisposeAction<T>(Action<T> action, T parameter) : IDisposable
{
    private readonly Action<T> _action = action ?? throw new ArgumentNullException(nameof(action));

    private readonly T? _parameter = parameter;

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "DisposeAction<T> is an Off-Label usage of IDisposable to execute an action without finalization.")]
    public void Dispose()
    {
        if (_parameter != null)
        {
            _action(_parameter);
        }
    }
}
