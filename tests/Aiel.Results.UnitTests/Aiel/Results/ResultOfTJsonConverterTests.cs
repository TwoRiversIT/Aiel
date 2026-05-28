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

public sealed class ResultOfTUnitTestBase(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void ResultOfT_Success_RoundTrips()
    {
        var converter = new ResultOfTJsonConverter<Int32>();
        var original = Result<Int32>.Success(42);

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var roundTrip = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

        Assert.True(roundTrip!.IsSuccess);
        Assert.Equal(42, roundTrip.Value);
    }

    [Fact]
    public void ResultOfT_Failure_RoundTrips()
    {
        var original = Result<Int32>.Failure(new SimpleError("Missing"));

        var json = JsonSerializer.Serialize(original, Results.JSO);
        var roundTrip = JsonSerializer.Deserialize<Result<Int32>>(json, Results.JSO);

        Assert.False(roundTrip!.IsSuccess);
        Assert.Equal(original.Error.Message, roundTrip.Error.Message);
        Assert.Equal(original.Error.ErrorCode.GetType(), roundTrip.Error.ErrorCode.GetType());
    }
}
