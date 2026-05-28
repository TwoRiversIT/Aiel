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

public class DisposableBaseTests
{
    private class TestDisposable : DisposableBase
    {
        public Boolean DisposeCalled { get; private set; }
        public Boolean DisposeAsyncCalled { get; private set; }

        public Boolean IsObjectDisposed => IsDisposed;

        protected override void Dispose(Boolean disposing)
        {
            if (!IsDisposed && disposing)
            {
                DisposeCalled = true;
            }

            base.Dispose(disposing);
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            if (!IsDisposed)
            {
                DisposeAsyncCalled = true;
            }

            await Task.CompletedTask;
        }
    }

    [Fact]
    public void Dispose_CallsDispose()
    {
        var disposable = new TestDisposable();

        disposable.Dispose();

        Assert.True(disposable.DisposeCalled);
        Assert.True(disposable.IsObjectDisposed);
    }

    [Fact]
    public async Task DisposeAsync_CallsDisposeAsyncCore()
    {
        var disposable = new TestDisposable();

        await disposable.DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
        Assert.True(disposable.IsObjectDisposed);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var disposable = new TestDisposable();

        disposable.Dispose();
        disposable.Dispose();

        Assert.True(disposable.DisposeCalled);
        Assert.True(disposable.IsObjectDisposed);
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        var disposable = new TestDisposable();

        await disposable.DisposeAsync();
        await disposable.DisposeAsync();

        Assert.True(disposable.DisposeAsyncCalled);
        Assert.True(disposable.IsObjectDisposed);
    }

    [Fact]
    public void IsDisposed_IsFalseBeforeDispose()
    {
        var disposable = new TestDisposable();

        Assert.False(disposable.IsObjectDisposed);
    }

    [Fact]
    public void IsDisposed_IsTrueAfterDispose()
    {
        var disposable = new TestDisposable();

        disposable.Dispose();

        Assert.True(disposable.IsObjectDisposed);
    }

    [Fact]
    public async Task IsDisposed_IsTrueAfterDisposeAsync()
    {
        var disposable = new TestDisposable();

        await disposable.DisposeAsync();

        Assert.True(disposable.IsObjectDisposed);
    }
}
