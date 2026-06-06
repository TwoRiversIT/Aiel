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

        Assert.True(roundTrip!.IsSuccess);
        Assert.Equal(Result.NoError, roundTrip.Error);
    }

    [Fact]
    public void Result_Failure_RoundTrips()
    {
        var original = Result.Failure(new SimpleError("Missing"));

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var roundTrip = JsonSerializer.Deserialize<Result>(json, Results.JSO);

        Assert.False(roundTrip!.IsSuccess);
        Assert.Equal(original.Error.Message, roundTrip.Error.Message);
        Assert.Equal(original.Error.ErrorCode.GetType(), roundTrip.Error.ErrorCode.GetType());
    }

    [Fact]
    public void ResultJsonStructure_Success_HasCorrectShape()
    {
        var result = Result.Success();
        var json = JsonSerializer.Serialize(result, Results.JSO);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("isSuccess", out var isSuccess));
        Assert.True(isSuccess.GetBoolean());

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.Equal(JsonValueKind.Object, error.ValueKind);

        Assert.True(error.TryGetProperty("$errorType", out var errorType));
        Assert.Contains("NoError", errorType.GetString());
    }

    [Fact]
    public void ResultJsonStructure_Failure_HasCorrectShape()
    {
        Result result = new SimpleError("Not found");
        var json = JsonSerializer.Serialize(result, Results.JSO);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("isSuccess", out var isSuccess));
        Assert.False(isSuccess.GetBoolean());

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.Equal(JsonValueKind.Object, error.ValueKind);

        Assert.True(error.TryGetProperty("$errorType", out var errorType));
        Assert.Contains("SimpleError", errorType.GetString());

        Assert.True(error.TryGetProperty("message", out var message));
        Assert.Equal("Not found", message.GetString());
    }

    [Fact]
    public void Results_JSO_UsesWebDefaults()
    {
        var policy = Results.JSO.PropertyNamingPolicy;

        // Web defaults use camelCase naming
        Assert.NotNull(policy);
        Assert.Equal("JsonCamelCaseNamingPolicy", policy.GetType().Name);
    }
}
