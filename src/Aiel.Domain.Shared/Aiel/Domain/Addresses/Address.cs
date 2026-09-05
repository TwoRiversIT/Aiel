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

using Aiel.Domain.Geography;
using System.ComponentModel.DataAnnotations;

namespace Aiel.Domain.Addresses;

/// <summary>
/// Represents a physical address, including addressee, street lines, city, province, postal code, and country.
/// </summary>
public record Address
{
    /// <summary>
    /// Gets a singleton instance of an empty address. This can be used to represent an uninitialized or default address.
    /// </summary>
    public static readonly Address Empty = new()
    {
        Addressee = String.Empty,
        Line1 = String.Empty,
        Line2 = String.Empty,
        City = String.Empty,
        Province = Region.Empty,
        PostalCode = PostCode.Empty,
        Country = Country.Empty
    };

    /// <summary>
    /// Gets or sets the name of the addressee for the address.
    /// </summary>
    public required String Addressee { get; init; } = String.Empty;

    /// <summary>
    /// Gets or sets the first line of the street address.
    /// </summary>
    [Display(Name = "Line 1")]
    public required String Line1 { get; init; } = String.Empty;

    /// <summary>
    /// Gets or sets the second line of the street address.
    /// </summary>
    [Display(Name = "Line 2")]
    public String Line2 { get; init; } = String.Empty;

    /// <summary>
    /// Gets or sets the city of the address.
    /// </summary>
    [Display(Name = "City")]
    public String City { get; init; } = String.Empty;

    /// <summary>
    /// Gets or sets the province or state of the address. This is represented by an IRegion interface, allowing for flexibility in region representation.
    /// </summary>
    [Display(Name = "Province")]
    public IRegion Province { get; init; } = Region.Empty;

    /// <summary>
    /// Gets or sets the postal code of the address. This is represented by an IPostCode interface, allowing for flexibility in postal code representation.
    /// </summary>
    [Display(Name = "Postal Code")]
    public required IPostCode PostalCode { get; init; }

    /// <summary>
    /// Gets or sets the country of the address. This is represented by a Country interface, allowing for flexibility in country representation.
    /// </summary>
    [Display(Name = "Country")]
    public Country Country { get; init; } = Country.Empty;
}
