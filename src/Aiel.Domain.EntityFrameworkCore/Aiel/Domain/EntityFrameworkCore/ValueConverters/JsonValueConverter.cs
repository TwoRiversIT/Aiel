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

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Aiel.Domain.EntityFrameworkCore.ValueConverters;

/// <summary>
/// A generic JSON converter for Entity Framework Core that serializes and
/// deserializes objects of type <typeparamref name="T"/> to and from JSON
/// strings for database storage.
/// </summary>
/// <typeparam name="T">The type of the object to be serialized and deserialized.</typeparam>
public class JsonValueConverter<T> : ValueConverter<T, String>
    where T : class
{
    // Configure JSON serialization options once to reuse
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false  // Compact storage
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonValueConverter{T}"/> class.
    /// </summary>
    public JsonValueConverter() : base(
        // Serialize object to JSON string, handle null gracefully
        v => v != null
            ? JsonSerializer.Serialize(v, Options)
            : null!,
        // Deserialize JSON back to object, handle null/empty
        v => !String.IsNullOrEmpty(v)
            ? JsonSerializer.Deserialize<T>(v, Options)!
            : null!)
    {
    }
}
