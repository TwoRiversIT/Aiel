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

public class SerializationTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void ResultOfT_GetValueOrDefault_WorksAfterDeserialization()
    {
        // Arrange
        var original = Result.Success(99);

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.GetValueOrDefault().Should().Be(99);
    }

    [Fact]
    public void ResultOfT_Failure_GetValueOrDefault_WorksAfterDeserialization()
    {
        Result<Int32> original = new SimpleError("Failed");
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.GetValueOrDefault().Should().Be(0);
        deserialized.GetValueOrDefault(42).Should().Be(42);
    }

    [Fact]
    public void Error_IsErrorType_WorksAfterDeserialization()
    {
        var original = new SimpleError("test");
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<SimpleError>(json, Results.JSO);

        deserialized.Should().NotBeNull();
        deserialized.IsErrorType<SimpleError>().Should().BeTrue();
        deserialized.IsErrorType<TransactionError>().Should().BeFalse();
    }
}
