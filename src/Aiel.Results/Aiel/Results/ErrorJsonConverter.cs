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

namespace Aiel.Results;

/// <summary>
/// JSON converter for <see cref="Error"/> types.
/// </summary>
public sealed class ErrorJsonConverter : JsonConverter<Error>
{
    internal const String Discriminator = "$errorType";

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(Discriminator, value.GetType().FullName);

        // Create fresh options with Web defaults to avoid infinite recursion with factory
        var cleanOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        foreach (var prop in JsonDocument
            .Parse(JsonSerializer.Serialize(value, value.GetType(), cleanOptions))
            .RootElement.EnumerateObject())
        {
            prop.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    // Create fresh options with Web defaults and camelCase naming to avoid issues with parameter binding
    // This avoids the ErrorJsonConverterFactory and ensures proper deserialization
    readonly JsonSerializerOptions _cleanOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var typeName = root.GetProperty(Discriminator).GetString()
            ?? throw new JsonException("Missing error type discriminator.");

        var errorType = ErrorRegistry.GetErrorType(typeName);

        var deserializedError = (Error?)JsonSerializer.Deserialize(
            root.GetRawText(),
            errorType,
            _cleanOptions
        );

        return deserializedError ?? throw new JsonException($"Failed to deserialize error of type {errorType.FullName}.");
    }
}
