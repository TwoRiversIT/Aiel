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
/// Provides a custom JSON converter for serializing and deserializing instances of the <see cref="Result{T}"/> type using
/// System.Text.Json.
/// </summary>
/// <remarks>This converter enables correct handling of <see cref="Result{T}"/> objects when using System.Text.Json, ensuring
/// that both success and failure cases are represented accurately in JSON. Use this converter when you need to
/// serialize or deserialize <see cref="Result{T}"/> values in your application, such as for API responses or data
/// persistence.</remarks>
/// <typeparam name="T">The type of the value contained within the <see cref="Result{T}"/> instance.</typeparam>
public sealed class ResultOfTJsonConverter<T> : JsonConverter<Result<T>>
{
    /// <inheritdoc/>
    public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var isSuccess = root.GetProperty("isSuccess").GetBoolean();

        T? value = default;
        if (root.TryGetProperty("value", out var valueElement) && valueElement.ValueKind != JsonValueKind.Null)
        {
            value = JsonSerializer.Deserialize<T>(valueElement.GetRawText(), options);
        }

        Error? error = null;
        if (root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null)
        {
            error = JsonSerializer.Deserialize<Error>(errorElement.GetRawText(), options);
        }

        return isSuccess
            ? Result<T>.Success(value!)
            : Result<T>.Failure(error ?? throw new JsonException("Error property is required for failure result"));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);

        if (value.Error != null)
        {
            writer.WritePropertyName("error");
            JsonSerializer.Serialize(writer, value.Error, options);
        }

        if (value.IsSuccess && value.Value != null)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else
        {
            writer.WriteNull("value");
        }

        writer.WriteEndObject();
    }
}
