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
public class ErrorTests
{
    [Fact]
    public void NoError_ShouldHave_Description()
    {
        Result.NoError.Description.Should().Be(NoError.DefaultMessage);
    }

    [Fact]
    public void NoError_AssignedToString_ShouldBe_NoError()
    {
        String codeName = Result.NoError.Code;

        codeName.Should().Be("NoError");
    }

    [Fact]
    public void SimpleError_ShouldBe_Creatable()
    {
        var error = new SimpleError("User not found");

        error.Should().BeOfType<SimpleError>();
        error.Description.Should().Be("User not found");
        String codeName = error.Code;
        codeName.Should().Be("SimpleError");
    }

    [Fact]
    public void SimpleError_ShouldUse_SingletonErrorCode()
    {
        var error1 = new SimpleError("Description 1");
        var error2 = new SimpleError("Description 2");

        error1.Code.Should().BeSameAs(error2.Code);
    }

    [Fact]
    public void Errors_WithSameTypeAndDescription_ShouldNotBeEqual_DifferentInstances()
    {
        var error1 = new SimpleError("Invalid input");
        var error2 = new SimpleError("Invalid input");

        error1.Should().NotBe(error2, "different Error instances should not be equal (reference equality)");
    }

    [Fact]
    public void Errors_SameInstance_ShouldBe_Equal()
    {
        var error = new SimpleError("Invalid input");
        var sameError = error;

        error.Should().Be(sameError, "same Error instance should be equal to itself");
    }

    [Fact]
    public void Errors_WithSameTypeButDifferentDescription_ShouldNotBe_Equal()
    {
        var error1 = new SimpleError("Invalid input");
        var error2 = new SimpleError("Different description");

        error1.Should().NotBe(error2);
    }

    [Fact]
    public void Errors_WithDifferentTypes_ShouldNotBe_Equal()
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
    public void ErrorCodeSingletons_ShouldAllBe_Unique()
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

        var codes = errors.Select(e => e.Code).ToList();
        var distinctCodes = codes.Distinct().ToList();

        distinctCodes.Should().HaveCount(codes.Count, "all error code singletons should be unique references");
    }

    [Fact]
    public void ErrorCodeProperty_IsAssignableTo_String()
    {
        var error = new TransactionError("Test")
        {
            Reason = TransactionFailureReason.InsufficientFunds,
            TransactionId = "LKJ34567@UHBV"
        };

        String codeAsString = error.Code;

        codeAsString.Should().Be("TransactionError");
    }

    [Fact]
    public void ErrorCode_ToString_ShouldReturn_Name()
    {
        var error = new TransactionError("Test")
        {
            Reason = TransactionFailureReason.InsufficientFunds,
            TransactionId = "ASD98765@PLMN"
        };

        var codeString = error.Code.ToString();

        codeString.Should().Be(nameof(TransactionError), "ToString should return the error type name");
    }

    [Fact]
    public void IsErrorType_WithMatchingType_ShouldReturn_True()
    {
        var error = new SimpleError("Test");

        var result = error.IsErrorType<SimpleError>();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsErrorType_WithNonMatchingType_ShouldReturn_False()
    {
        var error = new SimpleError("Test");

        var result = error.IsErrorType<TransactionError>();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsErrorType_WithBaseErrorType_ShouldReturn_True()
    {
        var error = new SimpleError("Test");

        var result = error.IsErrorType<Error>();

        result.Should().BeTrue();
    }
}
