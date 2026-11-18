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

using Aiel.Results;
using Aiel.Testing.Dummies;
using System.Text.Json;

namespace Aiel.Actions.Queries;

public class MultipleResultTests
{
    [Fact]
    public void MultipleResult_Constructor_SetsProperties()
    {
        // Arrange
        var page = 2; // Note: Paging is 1 based, not 0.
        var size = 6;
        var dataset = Enumerable.Range(1, 100).ToList();
        var matching = dataset.Where(d => d % 2 == 0).ToList();
        var values = matching.Skip((page - 1) * size).Take(size).ToList();
        var a = new String(' ', 40);

        // Act
        var result = new QueryResult<Int32>(values, page, size, matching.Count, dataset.Count);

        // Assert
        result.Values.Should().HaveCount(size);
        result.Count.Should().Be(values.Count);
        result.Matching.Should().Be(matching.Count);
        result.Total.Should().Be(dataset.Count);
        result.Pages.Should().Be((matching.Count + size - 1) / size);
        result.Page.Should().Be(page);
        result.Size.Should().Be(size);
    }

    [Fact]
    public void MultipleResult_Create_SetsProperties_Returns_Result()
    {
        // Arrange
        var page = 2; // Note: Paging is 1 based, not 0.
        var size = 6;
        var dataset = Enumerable.Range(1, 100).ToList();
        var matching = dataset.Where(d => d % 2 == 0).ToList();
        var values = matching.Skip((page - 1) * size).Take(size).ToList();
        var a = new String(' ', 40);

        // Act
        var result = QueryResult.Success(values, page, size, matching.Count, dataset.Count);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<QueryResult<Int32>>();
        result.IsSuccess.Should().BeTrue();
        result.Values.Should().NotBeNull();
        result.Total.Should().Be(dataset.Count);
        result.Total.Should().Be(dataset.Count);
        result.Pages.Should().Be((matching.Count + size - 1) / size);
        result.Size.Should().Be(size);
        result.Page.Should().Be(page);
    }

    [Fact]
    public void MultipleResult_Can_Be_Serialized_And_Deserialized()
    {
        // Arrange
        var results = new List<Int32>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var matching = results.Count;
        var pageCount = 2;
        var pageSize = 6;
        var currentPage = 1;
        var multipleResult = new QueryResult<Int32>(results, currentPage, pageSize, matching);

        // Act
        var serialized = JsonSerializer.Serialize(multipleResult);
        var deserialized = JsonSerializer.Deserialize<QueryResult<Int32>>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Matching.Should().Be(matching);
        deserialized.Pages.Should().Be(pageCount);
        deserialized.Size.Should().Be(pageSize);
        deserialized.Page.Should().Be(currentPage);
    }

    [Fact]
    public void MultipleResultOfT_Can_Be_Assigned_Error()
    {
        // Act
        QueryResult<Int32> result = new ApiError("An error occurred while processing the query.");

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeOfType<ApiError>();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void MultipleResultOfT_TryGetRecords_Should_ReturnFalseAndDefault_When_IsFailed()
    {
        // Arrange
        var result = QueryResult.Failure<TypicalRecord>(new SimpleError("Not found"));

        // Act
        var got = result.TryGetValues(out IReadOnlyList<TypicalRecord> records);

        // Assert
        got.Should().BeFalse();
        records.Should().BeEmpty();
    }

    [Fact]
    public void MultipleResultOfT_TryGetRecords_Should_ReturnFalse_When_ResultIsNotResultOfT()
    {
        // Arrange
        var result = QueryResult.Success([1, 2, 3, 4]);

        // Act
        var got = result.TryGetValues<TypicalRecord>(out var value);

        // Assert
        got.Should().BeFalse();
        value.Should().BeEmpty();
    }

    [Fact]
    public void MultipleResultOfT_TryGetRecords_Should_ReturnTrue_When_ResultIsResultOfT_IsSuccess()
    {
        // Arrange
        var john = TypicalRecord.Create("John");
        var result = QueryResult.Success([john]);

        // Act
        var got = result.TryGetValues<TypicalRecord>(out var records);

        // Assert
        got.Should().BeTrue();
        records.Should().Contain(john);
    }
}
