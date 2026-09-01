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

using Aiel.Testing.Errors;

namespace Aiel.Results;

public class ResultTests
{
    [Fact]
    public void Result_Should_BeFailure_When_CreatedWithFailure()
    {
        // Arrange
        var error = new SimpleError("Some error");
        var result = Result.Failure(error);

        // Act & Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Result_Should_BeSuccess_When_CreatedWithSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act & Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Result_Constructor_Should_ThrowArgumentException_When_CreatedWithInconsistentState()
    {
        // Act
        Action act = () => new Result(true, new SimpleError("Some error"));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("A Success Result must not have an error. (Parameter 'error')");
    }

    [Fact]
    public void Result_Constructor_Should_ThrowArgumentException_When_CreatedWithNullErrorForFailure()
    {
        // Act
        Action act = () => new Result(false, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("A Failure Result must have an error. (Parameter 'error')");
    }

    [Fact]
    public void ResultOfT_Success_Should_ThrowArgumentNullException_When_ValueIsNull()
    {
        // Act
        Action act = () => Result<TestRecord>.Success(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>(
            "the notnull constraint is warning-level only, so it must be enforced at runtime");
    }

    [Fact]
    public void ResultOfT_When_IsFailed_ValueProperty_Should_ThrowResultException()
    {
        // Arrange
        var error = new SimpleError("Not found");
        var result = Result<TestRecord>.Failure(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<ResultException>()
            .WithMessage($"*Cannot read Value when IsSuccess == false.*{typeof(SimpleError).Name}*")
            .Which.Error.Should().Be(error);
    }

    [Fact]
    public void ResultOfT_TryGetValue_Should_ReturnTrueAndValue_When_IsSuccess()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");
        var result = Result<TestRecord>.Success(record);

        // Act
        var got = result.TryGetValue(out var value);

        // Assert
        got.Should().BeTrue();
        value.Should().Be(record);
    }

    [Fact]
    public void ResultOfT_TryGetValue_Should_ReturnFalseAndDefault_When_IsFailed()
    {
        // Arrange
        var result = Result<TestRecord>.Failure(new SimpleError("Not found"));

        // Act
        var got = result.TryGetValue(out var value);

        // Assert
        got.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Result_TryGetValue_Should_ReturnFalse_When_ResultIsNotResultOfT()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var got = result.TryGetValue(out Int32 value);

        // Assert
        got.Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public void Result_TryGetValue_Should_ReturnTrue_When_ResultIsResultOfT_IsSuccess()
    {
        // Arrange
        Result result = Result.Success(42);

        // Act
        var got = result.TryGetValue(out Int32 value);

        // Assert
        got.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void Result_TryGetValue_Should_ReturnFalse_When_ResultIsResultOfT_IsFailed()
    {
        // Arrange
        Result result = Result<Int32>.Failure(new SimpleError("Not found"));

        // Act
        var got = result.TryGetValue(out Int32 value);

        // Assert
        got.Should().BeFalse();
        value.Should().Be(default);
    }

    /// <summary>
    /// A successful result whose value happens to equal <see langword="default"/> is still a
    /// present value. This is the regression that the previous HasDefaultValue() probe introduced.
    /// </summary>
    [Fact]
    public void ResultOfT_Success_Should_SetValue_When_ValueEqualsDefault()
    {
        // Act
        var result = Result<Int32>.Success(default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(default);
        result.TryGetValue(out var value).Should().BeTrue();
        value.Should().Be(default);
    }

    [Fact]
    public void Error_can_be_assigned_to_Result()
    {
        // Arrange
        var error = new SimpleError("Some error");

        // Act
        Result result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void Error_can_be_assigned_to_ResultOfT()
    {
        // Arrange
        var error = new SimpleError("Some error");

        // Act
        Result<Int32> singleResult = error;
        Result<IReadOnlyCollection<Int32>> collectionResult = error;

        // Assert
        singleResult.IsFailure.Should().BeTrue();
        singleResult.Error.Should().BeOfType<SimpleError>();
        collectionResult.IsFailure.Should().BeTrue();
        collectionResult.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void ResultOfT_HasValue_Returns_False_When_ResultIsFailure()
    {
        // Arrange
        var error = new SimpleError("Some error");

        // Act
        Result<String> result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ResultOfT_HasValue_Returns_True_When_ResultIsSuccess()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");

        // Act
        var result = Result<TestRecord>.Success(record);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void ResultOfT_Value_can_be_assigned()
    {
        // Act
        Result<Int32> singleResult = 42;
        Result<IReadOnlyCollection<Int32>> collectionResult = new[] { 42 };

        // Assert
        singleResult.IsSuccess.Should().BeTrue();
        singleResult.Value.Should().Be(42);
        collectionResult.IsSuccess.Should().BeTrue();
        collectionResult.Value.Should().BeEquivalentTo([42]);
    }
}
