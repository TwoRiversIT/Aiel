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
using System.Text.Json.Serialization;

namespace Aiel.Domain.Contacts.Infrastructure;

/// <summary>
/// Defines a JSON converter for <see cref="PhoneNumber"/> instances, allowing them to be serialized and deserialized as JSON strings.
/// </summary>
public sealed class PhoneNumberJsonConverter : JsonConverter<PhoneNumber>
{
    /// <summary>
    /// Reads and converts the JSON representation of a <see cref="PhoneNumber"/> object.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>The converted <see cref="PhoneNumber"/> object.</returns>
    public override PhoneNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var value = reader.GetString();
        if (String.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return PhoneNumber.Parse(value);
    }

    /// <summary>
    /// Writes a <see cref="PhoneNumber"/> object as a JSON string.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="PhoneNumber"/> value to write.</param>
    /// <param name="options">The serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, PhoneNumber value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.E164);
    }
}
