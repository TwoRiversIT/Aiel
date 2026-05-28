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

using Aiel.Results.TestErrors;

namespace Aiel.Results;

/// <summary>
/// Unit tests for the <see cref="ResultExtensions"/> async methods.
/// </summary>
public class ResultExtensionsAsyncTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public async Task MapAsync_WithSuccessResult_ShouldTransformValue()
    {
        var result = Result<Int32>.Success(5);

        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_WithFailureResult_ShouldReturnFailure()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));

        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public async Task MapAsync_WithTask_ShouldTransformValue()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));

        var mapped = await resultTask.MapAsync(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_WithTaskAndAsyncMapper_ShouldTransformValue()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));

        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindAsync_WithSuccessResult_ShouldChainOperation()
    {
        var result = Result<Int32>.Success(5);

        var bound = await result.BindAsync((Func<Int32, Task<Result<String>>>)(async x =>
        {
            await Task.Delay(1);
            return x > 0 ? Result<String>.Success(x.ToString()) : new SimpleError("Kidnapped");
        }));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_WithFailureResult_ShouldReturnFailure()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));

        var bound = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<String>.Success(x.ToString());
        });

        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public async Task BindAsync_WithTask_ShouldChainOperation()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));

        var bound = await resultTask.BindAsync(x =>
            x > 0 ? Result<String>.Success(x.ToString()) : new SimpleError("Gone Fishing"));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_WithTaskAndAsyncBinder_ShouldChainOperation()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));

        var bound = await resultTask.BindAsync((Func<Int32, Task<Result<String>>>)(async x =>
        {
            await Task.Delay(1);
            return x > 0 ? Result<String>.Success(x.ToString()) : new SimpleError("Smoke Break");
        }));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public async Task MatchAsync_WithSuccessResult_ShouldCallOnSuccess()
    {
        var result = Result<Int32>.Success(5);

        var message = await result.MatchAsync(
            async x =>
            {
                await Task.Delay(1);
                return $"Success: {x}";
            },
            async error =>
            {
                await Task.Delay(1);
                return $"Error: {error.Message}";
            });

        message.Should().Be("Success: 5");
    }

    [Fact]
    public async Task MatchAsync_WithFailureResult_ShouldCallOnFailure()
    {
        var result = Result<Int32>.Failure(new SimpleError("Item not found"));

        var message = await result.MatchAsync(
            async x =>
            {
                await Task.Delay(1);
                return $"Success: {x}";
            },
            async error =>
            {
                await Task.Delay(1);
                return $"Error: {error.Message}";
            });

        message.Should().Be("Error: Item not found");
    }

    [Fact]
    public async Task MatchAsync_WithTask_ShouldHandleBothCases()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));

        var message = await resultTask.MatchAsync(
            x => $"Success: {x}",
            error => $"Error: {error.Message}");

        message.Should().Be("Success: 5");
    }

    [Fact]
    public async Task MatchAsync_WithTaskAndAsyncHandlers_ShouldHandleBothCases()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));

        var message = await resultTask.MatchAsync(
            async x =>
            {
                await Task.Delay(1);
                return $"Success: {x}";
            },
            async error =>
            {
                await Task.Delay(1);
                return $"Error: {error.Message}";
            });

        message.Should().Be("Success: 5");
    }

    [Fact]
    public async Task TapAsync_WithSuccessResult_ShouldExecuteAction()
    {
        var result = Result<Int32>.Success(5);
        var sideEffect = 0;

        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            sideEffect = x * 2;
        });

        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(5);
        sideEffect.Should().Be(10);
    }

    [Fact]
    public async Task TapAsync_WithFailureResult_ShouldNotExecuteAction()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));
        var sideEffect = 0;

        var tapped = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            sideEffect = x * 2;
        });

        tapped.IsSuccess.Should().BeFalse();
        sideEffect.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_WithTask_ShouldExecuteAction()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));
        var sideEffect = 0;

        var tapped = await resultTask.TapAsync(x => sideEffect = x * 2);

        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(5);
        sideEffect.Should().Be(10);
    }

    [Fact]
    public async Task TapAsync_WithTaskAndAsyncAction_ShouldExecuteAction()
    {
        var resultTask = Task.FromResult(Result<Int32>.Success(5));
        var sideEffect = 0;

        var tapped = await resultTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            sideEffect = x * 2;
        });

        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(5);
        sideEffect.Should().Be(10);
    }

    [Fact]
    public async Task AsyncChaining_ShouldWorkCorrectly()
    {
        var result = await Task.FromResult(Result<Int32>.Success(5))
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            })
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return x > 5 ? Result<String>.Success(x.ToString()) : new SimpleError("Sick - Have Doctor's Note");
            })
            .TapAsync(async x =>
            {
                await Task.Delay(1);
                Console.WriteLine(x);
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("10");
    }
}
