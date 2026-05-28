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
/// Unit tests for the <see cref="ResultExtensions"/> synchronous methods.
/// </summary>
public class ResultExtensionsSyncTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void GetValueOrDefault_WithSuccessResult_ShouldReturnValue()
    {
        var result = Result<Int32>.Success(42);

        var value = result.GetValueOrDefault(0);

        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_WithFailureResult_ShouldReturnDefaultValue()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));

        var value = result.GetValueOrDefault(99);

        value.Should().Be(99);
    }

    [Fact]
    public void GetValueOrDefault_WithSuccessResultNoParameter_ShouldReturnValue()
    {
        var result = Result<Int32>.Success(42);

        var value = result.GetValueOrDefault();

        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_WithFailureResultNoParameter_ShouldReturnTypeDefault()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));

        var value = result.GetValueOrDefault();

        value.Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefault_WithReferenceType_ShouldReturnNull()
    {
        var result = Result<String>.Failure(new SimpleError("Not found"));

        var value = result.GetValueOrDefault();

        value.Should().BeNull();
    }

    [Fact]
    public void GetValueOrDefault_WithReferenceTypeAndDefault_ShouldReturnDefault()
    {
        var result = Result<String>.Failure(new SimpleError("Not found"));

        var value = result.GetValueOrDefault("default value");

        value.Should().Be("default value");
    }

    [Fact]
    public void Map_WithSuccessResult_ShouldTransformValue()
    {
        var result = Result<Int32>.Success(5);

        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_WithFailureResult_ShouldReturnFailure()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));

        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void Bind_WithSuccessResult_ShouldChainOperation()
    {
        var result = Result<Int32>.Success(5);

        var bound = result.Bind(x =>
            x > 0 ? Result<String>.Success(x.ToString()) : new SimpleError("On vacation"));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_WithFailureInBinder_ShouldReturnBinderFailure()
    {
        var result = Result<Int32>.Success(-5);

        var bound = result.Bind(x =>
            x > 0 ? Result<String>.Success(x.ToString()) : new SimpleError("Ask nicely next time"));

        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void Bind_WithFailureResult_ShouldReturnFailure()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));

        var bound = result.Bind(x => Result<String>.Success(x.ToString()));

        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void Match_WithSuccessResult_ShouldCallOnSuccess()
    {
        var result = Result<Int32>.Success(5);

        var message = result.Match(
            x => $"Success: {x}",
            error => $"Error: {error.Message}");

        message.Should().Be("Success: 5");
    }

    [Fact]
    public void Match_WithFailureResult_ShouldCallOnFailure()
    {
        var result = Result<Int32>.Failure(new SimpleError("Item not found"));

        var message = result.Match(
            x => $"Success: {x}",
            error => $"Error: {error.Message}");

        message.Should().Be("Error: Item not found");
    }

    [Fact]
    public void Tap_WithSuccessResult_ShouldExecuteAction()
    {
        var result = Result<Int32>.Success(5);
        var sideEffect = 0;

        var tapped = result.Tap(x => sideEffect = x * 2);

        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(5);
        sideEffect.Should().Be(10);
    }

    [Fact]
    public void Tap_WithFailureResult_ShouldNotExecuteAction()
    {
        var result = Result<Int32>.Failure(new SimpleError("Not found"));
        var sideEffect = 0;

        var tapped = result.Tap(x => sideEffect = x * 2);

        tapped.IsSuccess.Should().BeFalse();
        sideEffect.Should().Be(0);
    }

    [Fact]
    public void ToResult_WithSuccessResult_ShouldCreateTypedResult()
    {
        var result = Result.Success();

        var typedResult = result.ToResult(42);

        typedResult.IsSuccess.Should().BeTrue();
        typedResult.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_WithFailureResult_ShouldReturnFailure()
    {
        var result = Result.Failure(new SimpleError("Not found"));

        var typedResult = result.ToResult(42);

        typedResult.IsSuccess.Should().BeFalse();
        typedResult.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void Chaining_ShouldWorkCorrectly()
    {
        var result = Result<Int32>.Success(5)
            .Map(x => x * 2)
            .Bind(x => x > 5 ? Result<String>.Success(x.ToString()) : new SimpleError("Missing"))
            .Tap(x => Console.WriteLine(x));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("10");
    }
}
