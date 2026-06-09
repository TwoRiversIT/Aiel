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

namespace Aiel.Dapper;

/// <summary>
/// Specifies the database column name that a property should be mapped to when using Dapper.
/// </summary>
/// <remarks>
/// Apply this attribute to properties in your model classes to define custom mappings between
/// database column names and property names. This is useful when database column names differ
/// from your C# property naming conventions.
/// </remarks>
/// <example>
/// <code>
/// public class Customer
/// {
///     [ColumnMap("customer_id")]
///     public Int32 Id { get; set; }
///
///     [ColumnMap("full_name")]
///     public String Name { get; set; }
/// }
/// </code>
/// </example>
/// <param name="columnName">The name of the database column to map to this property.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnNameAttribute(String columnName) : Attribute
{
    /// <summary>
    /// Gets the database column name that this property maps to.
    /// </summary>
    public String Name { get; } = columnName;
}
