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

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Aiel.Dapper.UnitTests")]

namespace Aiel.DataAccess.Dapper.Internals;

/// <summary>
/// This class exists to test MapColumnsFromExecutingAssembly() functionality, ensuring that
/// column mappings are correctly applied when mapping from the executing assembly.
/// </summary>
[HasColumnMaps]
internal class ProductEx
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    [ColumnName("id")]
    public Int32 Id { get; set; }

    /// <summary>
    /// Gets or sets the product name, mapped from 'product_name' column.
    /// </summary>
    [ColumnName("product_name")]
    public String Name { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the product price, mapped from 'unit_price' column.
    /// </summary>
    [ColumnName("unit_price")]
    public Decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the quantity in stock, mapped from 'quantity_in_stock' column.
    /// </summary>
    [ColumnName("quantity_in_stock")]
    public Int32 Stock { get; set; }
}
