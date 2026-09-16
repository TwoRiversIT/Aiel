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

using Aiel.Domain.ValueObjects.Addresses;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Aiel.Domain.EntityFrameworkCore.ValueConverters;

/// <summary>
/// A value converter for the <see cref="Address"/> value object, enabling its conversion to and from a string representation for storage in a database.
/// </summary>
public class AddressValueConverter : ValueConverter<Address, String>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddressValueConverter"/> class.
    /// </summary>
    public AddressValueConverter() : base(
        address => ToString(address),
        serialized => FromString(serialized))
    {
    }

    private static Address FromString(String serialized)
        => JsonSerializer.Deserialize<Address>(serialized)
            ?? throw new InvalidOperationException("Failed to deserialize Address from JSON.");

    private static String ToString(Address address) => JsonSerializer.Serialize(address);
}

