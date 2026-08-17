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
    public void Failure_Result_May_Have_A_Value()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = Result.Failure(new SimpleError("Error Result with a Value"), guid);

        // Assert
        result.Value.Should().Be(guid);
    }
}
