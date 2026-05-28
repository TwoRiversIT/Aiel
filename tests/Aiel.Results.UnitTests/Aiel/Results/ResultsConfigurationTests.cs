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
/// Unit tests for the <see cref="Results"/> static configuration class.
/// </summary>
public class ResultsConfigurationTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void ConfigureForResults_WithValidOptions_ShouldAddAllThreeConverters()
    {
        var options = new JsonSerializerOptions();

        options.ConfigureForResults();

        options.Converters.Should().HaveCount(3);
        options.Converters.Should().Contain(c => c.GetType() == typeof(ErrorJsonConverterFactory));
        options.Converters.Should().Contain(c => c.GetType() == typeof(ResultJsonConverter));
        options.Converters.Should().Contain(c => c.GetType() == typeof(ResultOfTJsonConverterFactory));
    }

    [Fact]
    public void ConfigureForResults_WithNullOptions_ShouldThrowArgumentNullException()
    {
        JsonSerializerOptions? options = null;

        var action = () => options!.ConfigureForResults();

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureForResults_WhenCalledTwice_ShouldNotAddDuplicateConverters()
    {
        var options = new JsonSerializerOptions();

        options.ConfigureForResults();
        var countAfterFirstCall = options.Converters.Count;

        options.ConfigureForResults();
        var countAfterSecondCall = options.Converters.Count;

        countAfterFirstCall.Should().Be(3);
        countAfterSecondCall.Should().Be(3, "duplicate converters should not be added");
    }

    [Fact]
    public void ConfigureForResults_WhenCalledTwice_ShouldNotCreateDuplicateErrorJsonConverterFactory()
    {
        var options = new JsonSerializerOptions();

        options.ConfigureForResults();
        options.ConfigureForResults();

        var errorFactoryCount = options.Converters.Count(c => c.GetType() == typeof(ErrorJsonConverterFactory));
        errorFactoryCount.Should().Be(1);
    }

    [Fact]
    public void ConfigureForResults_WhenCalledTwice_ShouldNotCreateDuplicateResultJsonConverter()
    {
        var options = new JsonSerializerOptions();

        options.ConfigureForResults();
        options.ConfigureForResults();

        var resultConverterCount = options.Converters.Count(c => c.GetType() == typeof(ResultJsonConverter));
        resultConverterCount.Should().Be(1);
    }

    [Fact]
    public void ConfigureForResults_WhenCalledTwice_ShouldNotCreateDuplicateResultOfTJsonConverterFactory()
    {
        var options = new JsonSerializerOptions();

        options.ConfigureForResults();
        options.ConfigureForResults();

        var resultOfTFactoryCount = options.Converters.Count(c => c.GetType() == typeof(ResultOfTJsonConverterFactory));
        resultOfTFactoryCount.Should().Be(1);
    }

    [Fact]
    public void ConfigureForResults_WithOptionsContainingOtherConverters_ShouldAddResultConverters()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        options.ConfigureForResults();

        options.Converters.Should().HaveCountGreaterThanOrEqualTo(4);
        options.Converters.Should().Contain(c => c.GetType() == typeof(System.Text.Json.Serialization.JsonStringEnumConverter));
        options.Converters.Should().Contain(c => c.GetType() == typeof(ErrorJsonConverterFactory));
        options.Converters.Should().Contain(c => c.GetType() == typeof(ResultJsonConverter));
        options.Converters.Should().Contain(c => c.GetType() == typeof(ResultOfTJsonConverterFactory));
    }

    [Fact]
    public void ConfigureForResults_ShouldEnableSerializationOfResultsWithErrors()
    {
        var options = new JsonSerializerOptions();
        options.ConfigureForResults();

        Result<Int32> result = new SimpleError("Test error");
        var json = JsonSerializer.Serialize(result, options);

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain(ErrorJsonConverter.Discriminator);
    }

    [Fact]
    public void ConfigureForResults_ShouldEnableDeserializationOfResultsWithErrors()
    {
        var options = new JsonSerializerOptions();
        options.ConfigureForResults();

        Result<Int32> result = new SimpleError("Test error");
        var json = JsonSerializer.Serialize(result, options);

        var deserialized = JsonSerializer.Deserialize<Result<Int32>>(json, options);

        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeFalse();
        deserialized.Error.Should().BeOfType<SimpleError>();
    }

    [Fact]
    public void JSO_AfterConfigureForResults_ShouldBePreConfigured()
    {
        // The Results.JSO is lazily initialized and should contain all converters
        var jso = Results.JSO;

        jso.Should().NotBeNull();
        jso.Converters.Should().Contain(c => c.GetType() == typeof(ErrorJsonConverterFactory));
        jso.Converters.Should().Contain(c => c.GetType() == typeof(ResultJsonConverter));
        jso.Converters.Should().Contain(c => c.GetType() == typeof(ResultOfTJsonConverterFactory));
    }

    [Fact]
    public void JSO_MultipleAccessesShouldReturnSameInstance()
    {
        var jso1 = Results.JSO;
        var jso2 = Results.JSO;

        jso1.Should().BeSameAs(jso2);
    }

    [Fact]
    public void FullWorkflow_ConfigureAndSerialize_Success_ShouldRoundTrip()
    {
        // Create a fresh options instance for this test
        var options = new JsonSerializerOptions().ConfigureForResults();

        var original = Result.Success("Hello, World!");
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Result<String>>(json, options);

        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void FullWorkflow_ConfigureAndSerialize_Failure_ShouldRoundTrip()
    {
        // Create a fresh options instance for this test
        var options = new JsonSerializerOptions();
        options.ConfigureForResults();

        Result<String> original = new SimpleError("Operation failed");
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Result<String>>(json, options);

        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeFalse();
        deserialized.Error.Should().BeOfType<SimpleError>();
    }
}
