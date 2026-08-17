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
/// Serialization tests for <see cref="Maybe{T}"/>.
/// </summary>
public class MaybeSerializationTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void Some_ShouldSerializeAsBareValue()
    {
        // Act
        var json = JsonSerializer.Serialize(Maybe<String>.Some("Hello, World!"), Results.JSO);

        // Assert
        json.Should().Be("\"Hello, World!\"", "the Maybe wrapper must not leak into the wire format");
    }

    [Fact]
    public void None_ShouldSerializeAsNull()
    {
        // Act
        var json = JsonSerializer.Serialize(Maybe<String>.None, Results.JSO);

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void Some_ShouldRoundTrip()
    {
        // Arrange
        var original = Maybe<TestRecord>.Some(new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com"));

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Maybe<TestRecord>>(json, Results.JSO);

        // Assert
        deserialized.HasValue.Should().BeTrue();
        deserialized.Value.Should().Be(original.Value);
    }

    [Fact]
    public void None_ShouldRoundTrip()
    {
        // Arrange
        var original = Maybe<TestRecord>.None;

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Maybe<TestRecord>>(json, Results.JSO);

        // Assert
        deserialized.IsNone.Should().BeTrue();
    }

    /// <summary>
    /// A present value that happens to equal <see langword="default"/> must survive the round trip
    /// as <c>Some</c>, not collapse into <c>None</c>.
    /// </summary>
    [Fact]
    public void Some_WithDefaultValue_ShouldRoundTripAsSome()
    {
        // Act
        var json = JsonSerializer.Serialize(Maybe<Int32>.Some(0), Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Maybe<Int32>>(json, Results.JSO);

        // Assert
        deserialized.HasValue.Should().BeTrue("Some(0) is a present value, not an absent one");
        deserialized.Value.Should().Be(0);
    }

    [Fact]
    public void ResultOfMaybe_Some_ShouldRoundTrip()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");
        var original = Result.Success(Maybe<TestRecord>.Some(record));

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<Maybe<TestRecord>>>(json, Results.JSO);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.HasValue.Should().BeTrue();
        deserialized.Value.Value.Should().Be(record);
    }

    [Fact]
    public void ResultOfMaybe_None_ShouldRoundTripAsSuccess()
    {
        // Arrange
        var original = Result.Success(Maybe<TestRecord>.None);

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<Maybe<TestRecord>>>(json, Results.JSO);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.IsSuccess.Should().BeTrue("absence is not a failure");
        deserialized.Value.IsNone.Should().BeTrue();
    }

    [Fact]
    public void ResultOfMaybe_Failure_ShouldRoundTripAsFailure()
    {
        // Arrange
        Result<Maybe<TestRecord>> original = new SimpleError("The store is unavailable");

        // Act
        var json = JsonSerializer.Serialize(original, Results.JSO);
        var deserialized = JsonSerializer.Deserialize<Result<Maybe<TestRecord>>>(json, Results.JSO);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.IsFailure.Should().BeTrue();
        deserialized.Error.IsErrorType<SimpleError>().Should().BeTrue();
        deserialized.TryGetValue(out _).Should().BeFalse();
    }
}
