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

/// <summary>
/// Demonstrates that custom Error types can be created and serialized
/// without modifying the base Error class.
/// </summary>
public abstract class CustomErrorSerializationTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{

    [Fact]
    public void CustomError_WithAdditionalProperties_ShouldRoundTrip()
    {
        var original = new TransactionError("Payment declined")
        {
            Reason = TransactionFailureReason.InsufficientFunds,
            TransactionId = "TXN12345"
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<TransactionError>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be("Payment declined");
        deserialized.Reason.Should().Be(TransactionFailureReason.InsufficientFunds);
        deserialized.TransactionId.Should().Be("TXN12345");
    }

    [Fact]
    public void Error_AsBaseType_DeserializesToConcreteType()
    {
        Error original = new TransactionError("Payment declined")
        {
            Reason = TransactionFailureReason.InsufficientFunds
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Results.JSO);

        deserialized.Should().BeOfType<TransactionError>();
    }

    [Fact]
    public void Result_WithError_RoundTrips()
    {
        var result = Result.Failure(new TransactionError("Boom"));

        var json = JsonSerializer.Serialize(result, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result>(json, Results.JSO);

        deserialized!.IsSuccess.Should().BeFalse();
        deserialized.Error.Should().BeOfType<TransactionError>();
    }

    [Fact]
    public void CustomError_ShouldSerializeAndDeserialize()
    {
        var original = new OrderNotFoundError("Customer ID 12345 not found")
        {
            OrderId = "12345"
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<OrderNotFoundError>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be("Customer ID 12345 not found");
        deserialized.ErrorCode.Should().Be(original.ErrorCode);
    }

    [Fact]
    public void CustomError_AsBaseType_ShouldDeserializeToCorrectType()
    {
        Error original = new OrderNotFoundError("Customer not found")
        {
            OrderId = "ABC123"
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<OrderNotFoundError>();
        deserialized!.Message.Should().Be("Customer not found");
    }

    [Fact]
    public void CustomError_InResultOfT_ShouldRoundTrip()
    {
        Result<String> original = new OrderNotFoundError("Customer ID 999 not found")
        {
            OrderId = "999"
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<String>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeFalse();
        deserialized.Error.Should().BeOfType<OrderNotFoundError>();
        deserialized.Error.Message.Should().Be("Customer ID 999 not found");
    }

    [Fact]
    public void CustomError_WithAdditionalProperties_ShouldSerialize()
    {
        var original = new TransactionError("Payment declined")
        {
            Reason = TransactionFailureReason.InsufficientFunds,
            TransactionId = "TXN12345"
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<TransactionError>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be("Payment declined");
        deserialized.Reason.Should().Be(TransactionFailureReason.InsufficientFunds);
        deserialized.TransactionId.Should().Be("TXN12345");
    }

    [Fact]
    public void MultipleCustomErrors_ShouldMaintainTypeIntegrity()
    {
        var errors = new Error[]
        {
            new OrderNotFoundError("Customer 1 not found")
            {
                OrderId = "C001"
            },
            new TransactionError("Payment failed")
            {
                Reason = TransactionFailureReason.CardExpired,
                TransactionId = "TXN67890"
            },
            new OrderNotFoundError("Customer 2 not found")
            {
                OrderId = "C002"
            }
        };

        foreach (var error in errors)
        {
            var json = JsonSerializer.Serialize(error, Results.JSO);
            var deserialized = JsonSerializer.Deserialize<Error>(json, Results.JSO);

            deserialized.Should().NotBeNull();
            deserialized.GetType().Should().Be(error.GetType(),
                $"error type {error.GetType().Name} should be preserved");
        }
    }

    [Fact]
    public void CustomError_ErrorCodeSingleton_ShouldBePreservedAfterDeserialization()
    {
        var original = new OrderNotFoundError("test")
        {
            OrderId = "TEST123"
        };

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<OrderNotFoundError>(json, Results.JSO);

        deserialized!.ErrorCode.Should().Be(original.ErrorCode,
            "ErrorCode value equality should be preserved through serialization");
        deserialized.OrderId.Should().Be("TEST123");
    }
}
