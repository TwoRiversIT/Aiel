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

namespace Aiel.Actions.Queries;

public class PageInfoTests
{
    [Fact]
    public void PageInfo_Constructor_SetsProperties()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 10;
        var totalRecords = 50;

        // Act
        var pageInfo = new PageInfo(pageNumber, pageSize, totalRecords);

        // Assert
        pageInfo.Number.Should().Be(pageNumber);
        pageInfo.Size.Should().Be(pageSize);
        pageInfo.Total.Should().Be(totalRecords);
    }

    [Fact]
    public void PageInfo_Constructor_ThrowsArgumentOutOfRangeException_WhenPageNumberIsLessThan1()
    {
        // Act
        Action act = () => _ = new PageInfo(0, 10, 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Paging is 1 based. The pageNumber parameter must be greater than or equal to 1.*");
    }

    [Fact]
    public void PageInfo_Constructor_ThrowsArgumentOutOfRangeException_WhenPageSizeIsLessThan1()
    {
        // Act
        Action act = () => _ = new PageInfo(1, 0, 1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The pageSize parameter must be greater than or equal to 1.*");
    }

    [Fact]
    public void PageInfo_Number_Setter_ThrowsArgumentOutOfRangeException_WhenValueIsLessThan1()
    {
        // Arrange
        var pageInfo = new PageInfo(1, 10);
        // Act
        Action act = () => pageInfo.Number = 0;
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Paging is 1 based. The pageNumber parameter must be greater than or equal to 1.*");
    }

    [Fact]
    public void PageInfo_Size_Setter_ThrowsArgumentOutOfRangeException_WhenValueIsLessThan1()
    {
        // Arrange
        var pageInfo = new PageInfo(1, 10);
        // Act
        Action act = () => pageInfo.Size = 0;
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The pageSize parameter must be greater than or equal to 1.*");
    }

    [Fact]
    public void PageInfo_Offset_CalculatesCorrectly()
    {
        // Arrange
        var pageInfo = new PageInfo(3, 10);

        // Act
        var offset = pageInfo.Offset;

        // Assert
        offset.Should().Be(20);
    }

    [Fact]
    public void PageInfo_Calculates_Pages_Correctly()
    {
        // Act
        var pageInfo = new PageInfo(1, 10) { Total = 45 };

        // Assert
        pageInfo.Pages.Should().Be(5);
    }

    [Fact]
    public void PageInfo_WhenTotalIsZero_Pages_ReturnsNegativeOne()
    {
        // Arrange
        var pageInfo = new PageInfo(1, 10) { Total = 0 };
        
        // Act
        var pages = pageInfo.Pages;
     
        // Assert
        pages.Should().Be(-1);
    }
}
