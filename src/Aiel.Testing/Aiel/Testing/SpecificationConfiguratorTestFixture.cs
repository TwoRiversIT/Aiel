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

public class SpecificationConfiguratorTestFixture<TConfigurator, TSut> : SystemUnderTestConfiguratorTestFixture<TConfigurator, TSut>
    where TConfigurator : class, IConfigurator, new()
    where TSut : class
{
    private Func<ValueTask>? _givenAsync;
    private Func<ValueTask>? _whenAsync;
    private Func<ValueTask>? _thenAsync;

    internal override async ValueTask InitializeFixtureAsync(InitializationContext context, CancellationToken cancellationToken)
    {
        if (_givenAsync is null || _whenAsync is null || _thenAsync is null)
        {
            throw new InvalidOperationException("GivenAsync, WhenAsync, and ThenAsync functions have not been provided.");
        }

        await _givenAsync();
        await _whenAsync();
        await _thenAsync();
    }

    public void ProvideTest(Func<ValueTask> givenAsync, Func<ValueTask> whenAsync, Func<ValueTask> thenAsync)
    {
        _givenAsync = givenAsync ?? throw new ArgumentNullException(nameof(givenAsync));
        _whenAsync = whenAsync ?? throw new ArgumentNullException(nameof(whenAsync));
        _thenAsync = thenAsync ?? throw new ArgumentNullException(nameof(thenAsync));
    }
}
