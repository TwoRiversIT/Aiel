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

using Aiel.Results.IntegrationTests;
using Aiel.Results.Models;
using Aiel.Results.TestErrors;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

namespace Aiel.Results;

public class BasicTests(WebApplicationFactory<Program> factory)
        : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task Success_Result_CanBeDeserialized()
    {
        // Arrange
        var options = new JsonSerializerOptions().ConfigureForResults();
        var client = _factory.CreateClient();

        // Act
        var json = await client.GetStringAsync("/success", TestContext.Current.CancellationToken);

        var result = JsonSerializer.Deserialize<Result<IntrinsicTypes>>(json, options);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error.IsErrorType<NoError>().Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Failure_Result_CanBeDeserialized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetResultAsync<IntrinsicTypes>("/failure", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.IsErrorType<SimpleError>().Should().BeTrue();
    }

    [Fact]
    public async Task Success_Collection_Result_CanBeDeserialized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetResultAsync<IReadOnlyList<IntrinsicTypes>>("/collection/success", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.IsErrorType<NoError>().Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value[1].BoolValue.Should().BeTrue();
        result.Value[1].DateTimeValue.Should().NotBe(DateTime.MinValue);
        result.Value[1].DecimalValue.Should().Be(1.23m);
        result.Value[1].DoubleValue.Should().Be(4.56);
        result.Value[1].FloatValue.Should().Be(7.89f);
        result.Value[1].GuidValue.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.Value[1].IntValue.Should().Be(84);
        result.Value[1].StringValue.Should().Be("Hello, World!");
    }

    [Fact]
    public async Task Failure_Collection_Result_CanBeDeserialized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetResultAsync<IReadOnlyList<IntrinsicTypes>>("/collection/failure", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.IsErrorType<SimpleError>().Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ComplexError_Result_CanBeDeserialized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetResultAsync("/error", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        var error = result.Error as TransactionError;
        error.Should().NotBeNull();
        error.TransactionId.Should().Be("11111111-1111-1111-1111-111111111111");
        error.Reason.Should().Be(TransactionFailureReason.InsufficientFunds);
    }
}
