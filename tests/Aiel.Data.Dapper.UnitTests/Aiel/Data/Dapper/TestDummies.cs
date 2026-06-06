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

using Aiel.Dapper;

namespace Aiel.Data.Dapper;

/// <summary>
/// Test entity for customers.
/// </summary>
[HasColumnMaps]
public class Customer
{
    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    [ColumnName("customer_id")]
    public Int32 Id { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    [ColumnName("full_name")]
    public String Name { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the customer email.
    /// </summary>
    [ColumnName("email_address")]
    public String Email { get; set; } = String.Empty;
}

/// <summary>
/// Test entity for orders.
/// </summary>
[HasColumnMaps]
public class Order
{
    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    [ColumnName("order_id")]
    public Int32 Id { get; set; }

    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    [ColumnName("customer_id")]
    public Int32 CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the order total.
    /// </summary>
    [ColumnName("order_total")]
    public Decimal Total { get; set; }
}
