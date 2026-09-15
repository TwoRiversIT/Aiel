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

using Aiel.Framework;

namespace Aiel.Testing;

public abstract class TestBase(ITestOutputHelper testOutputHelper) : DisposableBase, IAsyncLifetime
{
    /// <summary>
    /// Gets the cancellation token from the current test context.
    /// </summary>
    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// Gets the test output helper for logging test output.
    /// </summary>
    protected ITestOutputHelper TestOutput => testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    /// <summary>
    /// Do not override this method! You have been warned!
    /// </summary>
    /// <remarks>
    /// This method is called by xUnit once for each test because each test gets a new instance of the test class.
    /// This is how the test fixture can be signaled to proceed with the test execution. The test fixture is
    /// responsible for executing the Given, When, and Then steps of the test further down the inheritance
    /// chain.
    /// </remarks>
    public async ValueTask InitializeAsync()
    {
        await BeginInstanceTestsAsync(CancellationToken);
    }

    // Provides a hook for derived classes to notify the test fixture
    // that the test instance is ready to begin executing the test.
    internal abstract ValueTask BeginInstanceTestsAsync(CancellationToken cancellationToken);
}
