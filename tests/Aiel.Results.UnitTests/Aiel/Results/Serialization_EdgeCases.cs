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

public class Serialization_EdgeCases(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void ResultOfT_MultipleSerializations_ShouldProduceSameJson()
    {
        Result<String> result = new SimpleError("Not found");

        var json1 = JsonSerializer.Serialize(result, Results.JSO);
        var json2 = JsonSerializer.Serialize(result, Results.JSO);

        json1.Should().Be(json2, "serialization should be deterministic");
    }

    [Fact]
    public void ResultOfT_MultipleDeserializations_ShouldPreserveErrorCodeEquality()
    {
        Result<Int32> result = new SimpleError("Conflict");
        var json = JsonSerializer.Serialize(result, Results.JSO);

        var deserialized1 = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);
        var deserialized2 = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);
        deserialized1!.Error.ErrorCode.Should().Be(deserialized2!.Error.ErrorCode,
            "ErrorCode should maintain value equality across deserializations");
    }

    [Fact]
    public void ResultOfT_IsSuccess_ShouldMatchAfterRoundTrip()
    {
        var success = Result.Success(42);
        Result<Int32> failure = new SimpleError("Not found");

        var successJson = JsonSerializer.Serialize(success, Results.JSO);
        var failureJson = JsonSerializer.Serialize(failure, Results.JSO);

        var deserializedSuccess = JsonSerializer.Deserialize<Result<Int32>>(successJson, Results.JSO);
        var deserializedFailure = JsonSerializer.Deserialize<Result<Int32>>(failureJson, Results.JSO);

        deserializedSuccess.Should().NotBeNull();
        deserializedSuccess.IsSuccess.Should().BeTrue();
        deserializedFailure.Should().NotBeNull();
        deserializedFailure.IsSuccess.Should().BeFalse();
    }
}
