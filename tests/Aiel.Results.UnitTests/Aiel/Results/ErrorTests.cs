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
/// Unit tests for the <see cref="Error"/> class and its factory methods.
/// </summary>
public class ErrorTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void None_ShouldHaveDescription()
    {
        var error = Result.NoError;

        error.Message.Should().Be(NoError.DefaultMessage);
    }

    [Fact]
    public void None_ShouldHaveNoneErrorCode()
    {
        var error = Result.NoError;

        String codeName = error.ErrorCode;

        codeName.Should().Be("NoError");
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        var error = new SimpleError("User not found");

        error.Should().BeOfType<SimpleError>();
        error.Message.Should().Be("User not found");
        String codeName = error.ErrorCode;
        codeName.Should().Be("SimpleError");
    }

    [Fact]
    public void NotFoundError_ShouldUseSingletonErrorCode()
    {
        var error1 = new SimpleError("Description 1");
        var error2 = new SimpleError("Description 2");

        error1.ErrorCode.Should().BeSameAs(error2.ErrorCode);
    }

    [Fact]
    public void Errors_WithSameTypeAndDescription_ShouldNotBeEqual_DifferentInstances()
    {
        var error1 = new SimpleError("Invalid input");
        var error2 = new SimpleError("Invalid input");

        error1.Should().NotBe(error2, "different Error instances should not be equal (reference equality)");
    }

    [Fact]
    public void Errors_SameInstance_ShouldBeEqual()
    {
        var error = new SimpleError("Invalid input");
        var sameError = error;

        error.Should().Be(sameError, "same Error instance should be equal to itself");
    }

    [Fact]
    public void Errors_WithSameTypeButDifferentDescription_ShouldNotBeEqual()
    {
        var error1 = new SimpleError("Invalid input");
        var error2 = new SimpleError("Different description");

        error1.Should().NotBe(error2);
    }

    [Fact]
    public void Errors_WithDifferentTypes_ShouldNotBeEqual()
    {
        var error1 = new TransactionError("Description")
        {
            Reason = TransactionFailureReason.CardExpired,
            TransactionId = "XDV83401@FVAD"
        };
        var error2 = new SimpleError("Description");

        error1.Should().NotBe(error2);
    }

    [Fact]
    public void ErrorCodeSingletons_ShouldAllBeUnique()
    {
        var errors = new Error[]
        {
            Result.NoError,
            new SimpleError("test"),
            new TransactionError("test")
            {
                Reason = TransactionFailureReason.InsufficientFunds,
                TransactionId = "ABC12345@XYZ"
            }
        };

        var codes = errors.Select(e => e.ErrorCode).ToList();
        var distinctCodes = codes.Distinct().ToList();

        distinctCodes.Should().HaveCount(codes.Count, "all error code singletons should be unique references");
    }

    [Fact]
    public void Error_CodePropertyCanBeUsedAsString()
    {
        var error = new TransactionError("Test")
        {
            Reason = TransactionFailureReason.InsufficientFunds,
            TransactionId = "LKJ34567@UHBV"
        };

        String codeAsString = error.ErrorCode;

        codeAsString.Should().Be("TransactionError");
    }

    [Fact]
    public void Error_ToStringOnCode_ShouldReturnName()
    {
        var error = new TransactionError("Test")
        {
            Reason = TransactionFailureReason.InsufficientFunds,
            TransactionId = "ASD98765@PLMN"
        };

        var codeString = error.ErrorCode.ToString();

        codeString.Should().Be("TransactionError", "ToString should return the error type name");
    }

    [Fact]
    public void IsErrorType_WithMatchingType_ShouldReturnTrue()
    {
        var error = new SimpleError("Test");

        var result = error.IsErrorType<SimpleError>();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsErrorType_WithNonMatchingType_ShouldReturnFalse()
    {
        var error = new SimpleError("Test");

        var result = error.IsErrorType<TransactionError>();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsErrorType_WithBaseErrorType_ShouldReturnTrue()
    {
        var error = new SimpleError("Test");

        var result = error.IsErrorType<Error>();

        result.Should().BeTrue();
    }
}
