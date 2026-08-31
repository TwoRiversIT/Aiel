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

public record Address
{
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

    public required String Addressee { get; init; } = String.Empty;

    [Display(Name = "Line 1")]
    public required String Line1 { get; init; } = String.Empty;

    [Display(Name = "Line 2")]
    public String Line2 { get; init; } = String.Empty;

    [Display(Name = "City")]
    public String City { get; init; } = String.Empty;

    [Display(Name = "Province")]
    public IRegion Province { get; init; } = Region.Empty;

    [Display(Name = "Postal Code")]
    public required IPostCode PostalCode { get; init; }

    [Display(Name = "County")]
    public Country Country { get; init; } = Country.Empty;
}
