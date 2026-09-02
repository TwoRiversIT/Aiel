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
using System.Text.Json;

namespace Aiel.Actions.Queries;

public class QueryMultipleResultTests
{
    [Fact]
    public void QueryMultipleResult_Constructor_SetsProperties()
    {
        // Arrange
        var results = new List<Int32>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var totalCount = results.Count;
        var pageCount = 2;
        var pageSize = 6;
        var currentPage = 1;

        // Act
        var result = new QueryMultipleResult<Int32>(results, currentPage, pageSize, totalCount);

        // Assert
        result.TotalRecords.Should().Be(totalCount);
        result.TotalPages.Should().Be(pageCount);
        result.PageSize.Should().Be(pageSize);
        result.PageNumber.Should().Be(currentPage);
    }

    [Fact]
    public void QueryMultipleResult_Create_SetsProperties_Returns_Result()
    {
        // Arrange
        var results = new List<Int32>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var totalCount = results.Count;
        var pageCount = 2;
        var pageSize = 6;
        var currentPage = 1;

        // Act
        var result = QueryMultipleResult.Create(results, currentPage, pageSize, totalCount);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<QueryMultipleResult<Int32>>();
        result.IsSuccess.Should().BeTrue();
        result.Records.Should().NotBeNull();
        result.TotalRecords.Should().Be(totalCount);
        result.TotalRecords.Should().Be(totalCount);
        result.TotalPages.Should().Be(pageCount);
        result.PageSize.Should().Be(pageSize);
        result.PageNumber.Should().Be(currentPage);
    }

    [Fact]
    public void QueryMultipleResult_Can_Be_Serialized_And_Deserialized()
    {
        // Arrange
        var results = new List<Int32>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var totalCount = results.Count;
        var pageCount = 2;
        var pageSize = 6;
        var currentPage = 1;
        var queryMultipleResult = new QueryMultipleResult<Int32>(results, currentPage, pageSize, totalCount);

        // Act
        var serialized = JsonSerializer.Serialize(queryMultipleResult);
        var deserialized = JsonSerializer.Deserialize<QueryMultipleResult<Int32>>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.TotalRecords.Should().Be(totalCount);
        deserialized.TotalPages.Should().Be(pageCount);
        deserialized.PageSize.Should().Be(pageSize);
        deserialized.PageNumber.Should().Be(currentPage);
    }

    [Fact]
    public void QueryMultipleResultOfT_Can_Be_Assigned_Error()
    {
        // Act
        QueryMultipleResult<Int32> result = new ApiError("An error occurred while processing the query.");

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeOfType<ApiError>();
        result.IsSuccess.Should().BeFalse();
    }
}
