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

namespace Aiel.Testing;

/// <summary>
/// Base class for specification tests (Given, When, Then), providing a
/// structured approach to testing a System Under Test (<typeparamref name="TSut"/>).
/// </summary>
/// <typeparam name="TFixture">The type of the specification fixture providing services and configuration.</typeparam>
/// <typeparam name="TSut">The type of the System Under Test (SUT) to be resolved from the service provider.</typeparam>
/// <param name="fixture">The specification fixture providing services and configuration.</param>
/// <param name="output">The test output helper for logging test output.</param>
/// <remarks>
/// <para>
/// This class is created once per test in the derived test class, and receives
/// the same fixture instance for all tests in the derived class.
/// </para>
/// <para>
/// The fixture provides shared setup, resources, or state that can be reused across multiple tests.
/// </para>
/// </remarks>
public abstract class SpecificationTestBase<TFixture, TSut>(TFixture fixture, ITestOutputHelper output)
    : SystemUnderTestBase<TFixture, TSut>(fixture, output)
    where TFixture : SpecificationTestFixture<TSut>
    where TSut : class
{
    internal override async ValueTask BeginInstanceTestsAsync(CancellationToken cancellationToken)
    {
        await base.BeginInstanceTestsAsync(cancellationToken);

        await GivenAsync(cancellationToken);
        await WhenAsync(cancellationToken);
        await ThenAsync(cancellationToken);
    }

    public virtual ValueTask GivenAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public abstract ValueTask WhenAsync(CancellationToken cancellationToken);

    public virtual ValueTask ThenAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
