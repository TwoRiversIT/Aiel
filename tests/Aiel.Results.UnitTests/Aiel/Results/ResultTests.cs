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
    public void Result_Should_ThrowArgumentException_When_CreatedWithInconsistentState()
    {
        // Arrange
        var error = new SimpleError("Some error");

        // Act
        Action act = () => new Result(true, error);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("A Success Result must not have an error. (Parameter 'error')");
    }

    [Fact]
    public void Result_Should_ThrowArgumentException_When_CreatedWithNullErrorForFailure()
    {
        // Act
        Action act = () => new Result(false, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("A Failure Result must have an error. (Parameter 'error')");
    }

    [Fact]
    public void Success_Should_ThrowArgumentNullException_When_ValueIsNull()
    {
        // Act
        Action act = () => Result<TestRecord>.Success(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>(
            "the notnull constraint is warning-level only, so it must be enforced at runtime");
    }

    [Fact]
    public void Value_Should_ThrowResultException_When_Failure()
    {
        // Arrange
        var error = new SimpleError("Not found");
        var result = Result<TestRecord>.Failure(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<ResultException>()
            .WithMessage("*Check IsSuccess before reading Value*")
            .Which.Error.Should().Be(error);
    }

    [Fact]
    public void TryGetValue_Should_ReturnTrueAndValue_When_Success()
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
    public void TryGetValue_Should_ReturnFalse_When_Failure()
    {
        // Arrange
        var result = Result<TestRecord>.Failure(new SimpleError("Not found"));

        // Act
        var got = result.TryGetValue(out var value);

        // Assert
        got.Should().BeFalse();
        value.Should().BeNull();
    }

    /// <summary>
    /// A successful result whose value happens to equal <see langword="default"/> is still a
    /// present value. This is the regression that the previous HasDefaultValue() probe introduced.
    /// </summary>
    [Fact]
    public void Success_Should_CarryValue_When_ValueEqualsDefault()
    {
        // Act
        var result = Result<Int32>.Success(0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        result.TryGetValue(out var value).Should().BeTrue();
        value.Should().Be(0);
    }
}
