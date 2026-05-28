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

namespace Aiel.Results.TestErrors;

public sealed partial class SimpleError : Error;

/// <summary>
/// Example of a dynamically generated custom error with no additional properties.
/// </summary>
public sealed partial class DatabaseConnectionError : Error
{
    // No additional properties - just uses the base description
}

/// <summary>
/// Example of a dynamically generated custom error with multiple properties.
/// </summary>
public sealed partial class InventoryInsufficientError : Error
{
    /// <summary>
    /// The product SKU that has insufficient inventory.
    /// </summary>
    public String ProductSku { get; init; }

    /// <summary>
    /// The requested quantity.
    /// </summary>
    public Int32 RequestedQuantity { get; init; }

    /// <summary>
    /// The available quantity in inventory.
    /// </summary>
    public Int32 AvailableQuantity { get; init; }
}

/// <summary>
/// Example of a dynamically generated domain-specific error in a consuming application.
/// This demonstrates how the generator handles errors defined outside the core library.
/// </summary>
public sealed partial class OrderNotFoundError : Error
{
    /// <summary>
    /// The unique identifier of the order that was not found.
    /// </summary>
    public String OrderId { get; init; }
}

/// <summary>
/// Manually created custom error type for testing purposes.
/// </summary>
public sealed partial class TransactionError : Error
{
    public TransactionFailureReason Reason { get; init; }
    public String TransactionId { get; init; }
}

public enum TransactionFailureReason
{
    InsufficientFunds,
    CardExpired,
    InvalidCardNumber,
    NetworkError,
    UnknownError
}
