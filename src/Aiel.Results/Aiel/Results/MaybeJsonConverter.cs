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
/// Provides a custom JSON converter for <see cref="Maybe{T}"/> using System.Text.Json.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Maybe{T}.Some(T)"/> is written as the bare underlying value and <see cref="Maybe{T}.None"/> is
/// written as <see langword="null"/>. The wrapper does not appear in the JSON, so API contracts stay clean
/// for consumers that have no notion of <see cref="Maybe{T}"/>.
/// </para>
/// <para>
/// The absence of a wrapper on the wire does not weaken the guarantee: it is the type system, not the JSON,
/// that forces callers to handle the empty case.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the value that may be present.</typeparam>
public sealed class MaybeJsonConverter<T> : JsonConverter<Maybe<T>>
    where T : notnull
{
    /// <summary>
    /// Gets a value indicating that this converter handles <see langword="null"/> tokens itself,
    /// because <see langword="null"/> is the wire representation of <see cref="Maybe{T}.None"/>.
    /// </summary>
    public override Boolean HandleNull => true;

    /// <inheritdoc/>
    public override Maybe<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Maybe<T>.None;
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);

        return Maybe.FromNullable(value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Maybe<T> value, JsonSerializerOptions options)
    {
        if (value.TryGetValue(out var inner))
        {
            JsonSerializer.Serialize(writer, inner, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
