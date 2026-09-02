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

public class SortFieldTests
{
    [Fact]
    public void SortField_ParameterizedConstructor_SetsProperties()
    {
        // Arrange
        var name = "Name";
        var direction = SortDirection.Descending;

        // Act
        var sortField = new SortField(name, direction);

        // Assert
        sortField.Name.Should().Be(name);
        sortField.Direction.Should().Be(direction);
    }

    [Fact]
    public void SortField_Can_Be_Serialized_And_Deserialized()
    {
        // Arrange
        var sortField = new SortField("Geordi", SortDirection.Descending);

        // Act
        var serialized = JsonSerializer.Serialize(sortField);
        var deserialized = JsonSerializer.Deserialize<SortField>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be("Geordi");
        deserialized.Direction.Should().Be(SortDirection.Descending);
    }

    [Fact]
    public void SortField_Constructor_ThrowsArgumentException_WhenNameIsNullOrWhitespace()
    {
        // Act
        Action act1 = () => _ = new SortField(null!);
        Action act2 = () => _ = new SortField("");
        Action act3 = () => _ = new SortField("   ");

        // Assert
        act1.Should().Throw<ArgumentException>().WithMessage("Sort field name must not be null, empty, or whitespace.*");
        act2.Should().Throw<ArgumentException>().WithMessage("Sort field name must not be null, empty, or whitespace.*");
        act3.Should().Throw<ArgumentException>().WithMessage("Sort field name must not be null, empty, or whitespace.*");
    }

    [Fact]
    public void SortField_Constructor_ThrowsArgumentOutOfRangeException_WhenDirectionIsInvalid()
    {
        // Act
        Action act = () => _ = new SortField("Name", (SortDirection)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Invalid sort direction. (Parameter 'Direction')");
    }

    [Fact]
    public void SortField_DefaultConstructor_SetsProperties()
    {
        // Act
        var sortField = new SortField();

        // Assert
        sortField.Name.Should().Be(SortField.InvalidName);
        sortField.Direction.Should().Be(0);
    }
}
