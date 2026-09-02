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

using System.Text.Json;

namespace Aiel.Actions.Queries;

public class PageTests
{
    [Fact]
    public void Page_Create_SetsProperties()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 10;
        var totalRecords = 50;

        // Act
        var pageBased = Page.Create(pageNumber, pageSize, totalRecords);

        // Assert
        pageBased.Number.Should().Be(pageNumber);
        pageBased.Size.Should().Be(pageSize);
        pageBased.Total.Should().Be(totalRecords);
    }

    [Fact]
    public void Page_SkipTake_SetsProperties()
    {
        // Arrange
        var skip = 30;
        var take = 50;

        // Act
        var skipTake = Page.SkipTake(skip, take);

        // Assert
        skipTake.Offset.Should().Be(skip);
        skipTake.Size.Should().Be(take);
    }

    [Fact]
    public void Page_Create_SetsPageNumberToOne_WhenPageNumberIsLessThanOne()
    {
        // Act
        var pageInfo = Page.Create(0, 0, 0);

        // Assert
        pageInfo.Number.Should().Be(1);
    }

    [Fact]
    public void Page_Create_SetsPageSizeToOne_WhenPageSizeIsLessThanOne()
    {
        // Act
        var pageInfo = Page.Create(0, 0, 0);

        // Assert
        pageInfo.Size.Should().Be(1);
    }

    [Fact]
    public void Page_Number_Setter_SetsPageNumberToOne_WhenValueIsLessThanOne()
    {
        // Arrange
        var pageInfo = Page.Create(10, 10, 100);

        // Act
        pageInfo = pageInfo with { Number = 0 };

        // Assert
        pageInfo.Number.Should().Be(1);
    }

    [Fact]
    public void Page_Size_Setter_SetsPageSizeToOne_WhenValueIsLessThanOne()
    {
        // Arrange
        var pageInfo = Page.Create(10, 10, 100);

        // Act
        pageInfo = pageInfo with { Size = 0 };

        // Assert
        pageInfo.Size.Should().Be(1);
    }

    [Fact]
    public void Page_Create_Calculates_Offset_Correctly()
    {
        // Arrange
        // Remember, Paging is 1-based, so Page 3 with a size of 10 means the offset is 20 (0-based).
        var pageInfo = Page.Create(3, 10, 1);

        // Act
        var offset = pageInfo.Offset;

        // Assert
        offset.Should().Be(20);
    }

    [Fact]
    public void Page_SkipTake_Calculates_Offset_Correctly()
    {
        // Arrange
        var pageInfo = Page.SkipTake(3, 10, 1);

        // Act
        var offset = pageInfo.Offset;

        // Assert
        offset.Should().Be(3);
    }

    [Fact]
    public void Page_Offset_Can_Be_Set()
    {
        // Arrange
        var pageInfo = Page.Create(3, 10, 100);

        // Act
        pageInfo = pageInfo with { Offset = 15 };

        // Assert
        pageInfo.Offset.Should().Be(15);
    }

    [Fact]
    public void Page_Offset_Can_Be_Reset()
    {
        // Arrange & Sanity Check
        var pageInfo = Page.All;
        pageInfo.Offset.Should().Be(0);
        pageInfo.Size.Should().Be(Int32.MaxValue);

        // Act
        pageInfo = pageInfo with { Offset = 15 };

        // Assert
        pageInfo.Offset.Should().Be(15);
    }

    [Fact]
    public void Page_Calculates_Pages_Correctly()
    {
        // Act
        var pageInfo = Page.Create(10, 10, 45);

        // Assert
        pageInfo.Pages.Should().Be(5);
    }

    [Fact]
    public void Page_Create_CanBeSerialized()
    {
        // Arrange
        var pageInfo = Page.Create(2, 10, 50);

        // Act
        var serialized = JsonSerializer.Serialize(pageInfo);
        var deserialized = JsonSerializer.Deserialize<Page>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Number.Should().Be(pageInfo.Number);
        deserialized.Size.Should().Be(pageInfo.Size);
        deserialized.Offset.Should().Be(pageInfo.Offset);
        deserialized.Total.Should().Be(pageInfo.Total);
    }

    [Fact]
    public void Page_SkipTake_CanBeSerialized()
    {
        // Arrange
        var pageInfo = Page.SkipTake(2, 10, 50);

        // Act
        var serialized = JsonSerializer.Serialize(pageInfo);
        var deserialized = JsonSerializer.Deserialize<Page>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Number.Should().Be(pageInfo.Number);
        deserialized.Size.Should().Be(pageInfo.Size);
        deserialized.Offset.Should().Be(pageInfo.Offset);
        deserialized.Total.Should().Be(pageInfo.Total);
    }

    [Fact]
    public void Page_WhenTotalIsZero_Pages_ReturnsZero()
    {
        // Act
        var pageInfo = Page.Create(1, 10, 0);

        // Assert
        pageInfo.Pages.Should().Be(0);
    }

    [Fact]
    public void Page_Create_WithInsaneValues_Returns_SaneValue()
    {
        // Act
        var pageInfo = Page.Create(-99, -99, -1000);

        // Assert
        pageInfo.Pages.Should().Be(0);
        pageInfo.Total.Should().Be(0);
        pageInfo.Size.Should().Be(1);
        pageInfo.Number.Should().Be(1);
    }

    [Fact]
    public void Page_SkipTake_WithInsaneValues_Returns_SaneValue()
    {
        // Act
        var pageInfo = Page.SkipTake(-99, -99, -1000);

        // Assert
        pageInfo.Pages.Should().Be(0);
        pageInfo.Total.Should().Be(0);
        pageInfo.Size.Should().Be(1);
        pageInfo.Number.Should().Be(1);
    }
}
