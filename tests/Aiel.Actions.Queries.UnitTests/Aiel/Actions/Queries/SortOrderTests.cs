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

public class SortOrderTests
{
    [Fact]
    public void SortOrder_Constructor_SetsProperties()
    {
        // Arrange
        var sortBy = new List<SortField> { new("Name", SortDirection.Ascending), new("Age", SortDirection.Descending) };

        // Act
        var sortOrder = new SortOrder(sortBy);

        // Assert
        sortOrder.Fields.Should().HaveCount(2);
        sortOrder.Fields[0].Name.Should().Be("Name");
        sortOrder.Fields[0].Direction.Should().Be(SortDirection.Ascending);
        sortOrder.Fields[1].Name.Should().Be("Age");
        sortOrder.Fields[1].Direction.Should().Be(SortDirection.Descending);
    }

    [Fact]
    public void SortOrder_Constructor_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _ = new SortOrder(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SortOrder_Can_Be_Serialized_And_Deserialized()
    {
        // Arrange
        var sortOrder = new SortOrder([new("Name", SortDirection.Ascending), new("Age", SortDirection.Descending)]);
     
        // Act
        var serialized = JsonSerializer.Serialize(sortOrder);
        var deserialized = JsonSerializer.Deserialize<SortOrder>(serialized);
        
        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Fields.Should().HaveCount(2);
        deserialized.Fields[0].Name.Should().Be("Name");
        deserialized.Fields[0].Direction.Should().Be(SortDirection.Ascending);
        deserialized.Fields[1].Name.Should().Be("Age");
        deserialized.Fields[1].Direction.Should().Be(SortDirection.Descending);
    }
}
