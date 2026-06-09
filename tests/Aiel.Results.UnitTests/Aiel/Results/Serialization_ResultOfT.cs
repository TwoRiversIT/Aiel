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
using System.Text.Json;

namespace Aiel.Results;

public class Serialization_ResultOfT(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{

    [Fact]
    public void ResultOfT_Success_WithInt_ShouldRoundTrip()
    {
        // Arrange
        var original = Result.Success(42);

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().Be(42);
    }

    [Fact]
    public void ResultOfT_Success_WithString_ShouldRoundTrip()
    {
        var original = Result.Success("Hello, World!");
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<String>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void ResultOfT_Success_WithNullString_ShouldRoundTrip()
    {
        var original = Result.Success<String?>(null);
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<String?>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().BeNull();
    }

    [Fact]
    public void ResultOfT_Success_WithRecord_ShouldRoundTrip()
    {
        var record = new TestRecord(123, "John Doe", "john@example.com");
        var original = Result.Success(record);
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<TestRecord>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().BeEquivalentTo(record);
    }

    [Fact]
    public void ResultOfT_Success_WithList_ShouldRoundTrip()
    {
        var items = new List<String> { "one", "two", "three" };
        var original = Result.Success(items);
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<List<String>>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void ResultOfT_Success_WithDictionary_ShouldRoundTrip()
    {
        var dict = new Dictionary<String, Int32> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        var original = Result.Success(dict);
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<Dictionary<String, Int32>>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().BeEquivalentTo(dict);
    }

    [Fact]
    public void ResultOfT_Success_WithComplexObject_ShouldRoundTrip()
    {
        var invoice = InvoiceDto.BogusInvoice;
        var original = Result.Success(invoice);
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<InvoiceDto>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Error.Should().BeOfType<NoError>();
        deserialized.Value.Should().BeEquivalentTo(invoice);
    }

    [Fact]
    public void ResultOfT_Failure_WithNotFoundError_ShouldRoundTrip()
    {
        Result<TestRecord> original = new SimpleError("Customer not found");
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<TestRecord>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeFalse();
        deserialized.Error.Should().BeOfType<SimpleError>();
        deserialized.Error.Message.Should().Be("Customer not found");
    }

    [Fact]
    public void ResultOfT_Failure_WithTransactionError_ShouldRoundTrip()
    {
        Result<Int32> original = new TransactionError("Could not connect to the payment gateway")
        {
            Reason = TransactionFailureReason.NetworkError,
            TransactionId = "ABC12345@DRMA"
        };
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeFalse();
        deserialized.Error.Should().BeOfType<TransactionError>();
        deserialized.Error.Message.Should().Be("Could not connect to the payment gateway");
        deserialized.Error.ErrorCode.Should().Be(new TransactionError("test")
        {
            Reason = TransactionFailureReason.NetworkError,
            TransactionId = "QWE23456@RTYU"
        }.ErrorCode);
    }

    [Fact]
    public void ResultOfT_Failure_AllErrorTypes_ShouldPreserveErrorCodeSingletons()
    {
        var errors = new Error[]
        {
            new SimpleError("test"),
            new TransactionError("test")
            {
                Reason = TransactionFailureReason.NetworkError,
                TransactionId = "test"
            }
        };

        foreach (var error in errors)
        {
            Result<Int32> result = error;
            var json = JsonSerializer.Serialize(result, Results.JSO);
            var deserialized = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

            deserialized.Should().NotBeNull();
            deserialized.IsSuccess.Should().BeFalse();
            deserialized.Error.GetType().Should().Be(error.GetType());
            deserialized.Error.ErrorCode.Should().Be(error.ErrorCode,
                $"{error.GetType().Name} should preserve ErrorCode value equality");
        }
    }
}
