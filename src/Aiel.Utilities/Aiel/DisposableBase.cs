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
/// Provides a base implementation for test fixtures with standard disposal patterns.
/// </summary>
public abstract class DisposableBase : IAsyncDisposable, IDisposable
{
    private Boolean _isDisposed;

    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    protected Boolean IsDisposed => _isDisposed;

    /// <summary>
    /// Releases all resources used by the test fixture.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases all resources used by the test fixture.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        // Dispose unmanaged resources only; managed already handled by DisposeAsyncCore
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the test fixture and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    /// <remarks>
    /// Derived classes should override this method to dispose of their specific managed resources synchronously.
    /// </remarks>
    protected virtual void Dispose(Boolean disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Dispose managed resources in derived classes
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// Asynchronously releases managed resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// Derived classes should override this method to asynchronously dispose of their specific managed resources.
    /// </remarks>
    protected virtual ValueTask DisposeAsyncCore()
    {
        // Dispose managed resources asynchronously in derived classes
        return ValueTask.CompletedTask;
    }
}
