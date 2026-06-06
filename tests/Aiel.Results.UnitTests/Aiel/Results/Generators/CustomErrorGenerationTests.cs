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

namespace Aiel.Results.Generators;

/// <summary>
/// Tests to verify that custom errors defined in consuming assemblies are properly generated.
/// This also demonstrates the REGISTRATION CHALLENGE: how do these errors get registered
/// in ErrorRegistry which lives in a different assembly?
/// </summary>
public sealed class CustomErrorGenerationTests
{
    [Fact]
    public void OrderNotFoundError_Should_BeCreatable()
    {
        // Arrange & Act
        var error = new OrderNotFoundError("Order ORD-12345 was not found in the system")
        {
            OrderId = "ORD-12345"
        };

        // Assert
        Assert.Equal("ORD-12345", error.OrderId);
        Assert.Equal("Order ORD-12345 was not found in the system", error.Message);
        Assert.NotNull(error.ErrorCode);
    }

    [Fact]
    public void InventoryInsufficientError_Should_HandleMultipleProperties()
    {
        // Arrange & Act
        var error = new InventoryInsufficientError("Insufficient inventory for WIDGET-001")
        {
            ProductSku = "WIDGET-001",
            RequestedQuantity = 100,
            AvailableQuantity = 25
        };

        // Assert
        Assert.Equal("WIDGET-001", error.ProductSku);
        Assert.Equal(100, error.RequestedQuantity);
        Assert.Equal(25, error.AvailableQuantity);
        Assert.Equal("Insufficient inventory for WIDGET-001", error.Message);
    }

    [Fact]
    public void DatabaseConnectionError_Should_WorkWithNoAdditionalProperties()
    {
        // Arrange & Act
        var error = new DatabaseConnectionError("Failed to connect to database");

        // Assert
        Assert.Equal("Failed to connect to database", error.Message);
        Assert.NotNull(error.ErrorCode);
    }
}
