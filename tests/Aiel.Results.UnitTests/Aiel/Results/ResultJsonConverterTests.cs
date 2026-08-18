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

public sealed class ResultJsonConverterTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void Result_Success_RoundTrips()
    {
        var original = Result.Success();

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var roundTrip = JsonSerializer.Deserialize<Result>(json, Results.JSO);

        roundTrip!.IsSuccess.Should().BeTrue();
        roundTrip.Error.Should().Be(Result.NoError);
    }

    [Fact]
    public void Result_Failure_RoundTrips()
    {
        var original = Result.Failure(new SimpleError("Missing"));

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var roundTrip = JsonSerializer.Deserialize<Result>(json, Results.JSO);

        roundTrip!.IsSuccess.Should().BeFalse();
        roundTrip.Error.ErrorDescription.Should().Be(original.Error.ErrorDescription);
        roundTrip.Error.ErrorCode.GetType().Should().Be(original.Error.ErrorCode.GetType());
    }

    [Fact]
    public void ResultJsonStructure_Success_HasCorrectShape()
    {
        var result = Result.Success();
        var json = JsonSerializer.Serialize(result, Results.JSO);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("isSuccess", out var isSuccess).Should().BeTrue();
        isSuccess.GetBoolean().Should().BeTrue();

        root.TryGetProperty("error", out var error).Should().BeTrue();
        error.ValueKind.Should().Be(JsonValueKind.Object);

        error.TryGetProperty("$errorType", out var errorType).Should().BeTrue();
        errorType.GetString().Should().Contain("NoError");
    }

    [Fact]
    public void ResultJsonStructure_Failure_HasCorrectShape()
    {
        Result result = new SimpleError("Not found");
        var json = JsonSerializer.Serialize(result, Results.JSO);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("isSuccess", out var isSuccess).Should().BeTrue();
        isSuccess.GetBoolean().Should().BeFalse();

        root.TryGetProperty("error", out var error).Should().BeTrue();
        error.ValueKind.Should().Be(JsonValueKind.Object);

        error.TryGetProperty("$errorType", out var errorType).Should().BeTrue();
        errorType.GetString().Should().Contain("SimpleError");

        error.TryGetProperty("errorDescription", out var errorDescription).Should().BeTrue();
        errorDescription.GetString().Should().Be("Not found");
    }

    [Fact]
    public void Results_JSO_UsesWebDefaults()
    {
        var policy = Results.JSO.PropertyNamingPolicy;

        // Web defaults use camelCase naming
        policy.Should().NotBeNull();
        policy.GetType().Name.Should().Be("JsonCamelCaseNamingPolicy");
    }
}
