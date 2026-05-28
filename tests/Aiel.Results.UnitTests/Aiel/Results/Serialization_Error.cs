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

public class Serialization_Error(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{

    [Fact]
    public void Error_TransactionError_ShouldRoundTrip()
    {
        var original = new TransactionError("test")
        {
            Reason = TransactionFailureReason.NetworkError,
            TransactionId = "FUB02789@BAR"
        };
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<TransactionError>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be("test");
        deserialized.ErrorCode.Should().Be(new TransactionError("test")
        {
            Reason = TransactionFailureReason.NetworkError,
            TransactionId = "test"
        }.ErrorCode);
    }

    [Fact]
    public void Error_NotFoundError_ShouldRoundTrip()
    {
        var original = new SimpleError("Resource ID 123 not found");
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<SimpleError>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be("Resource ID 123 not found");
        deserialized.ErrorCode.Should().Be(new SimpleError("test").ErrorCode);
    }

    [Fact]
    public void Error_WithSpecialCharacters_ShouldRoundTrip()
    {
        var original = new SimpleError("Path: <>&\"'\\\n\t");
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<SimpleError>(json, Results.JSO);
        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be("Path: <>&\"'\\\n\t");
    }

    [Fact]
    public void Error_AsBaseType_ShouldDeserializeToCorrectType()
    {
        Error original = new SimpleError("Item not found");
        var json = JsonSerializer.Serialize(original, Results.JSO);

        var deserialized = JsonSerializer.Deserialize<Error>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SimpleError>();
        deserialized!.Message.Should().Be("Item not found");
    }
}
