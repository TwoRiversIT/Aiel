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

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aiel.StrongIds;

public static class StrongIdsSystemTextJson
{
    public static JsonSerializerOptions ConfigureForStrongIds(this JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializerOptions);

        if (jsonSerializerOptions.IsReadOnly)
        {
            throw new InvalidOperationException("The provided JsonSerializerOptions instance is read-only and cannot be configured.");
        }

        if (!jsonSerializerOptions.Converters.Any(static converter => converter.GetType() == typeof(StrongIdJsonConverterFactory)))
        {
            jsonSerializerOptions.Converters.Add(new StrongIdJsonConverterFactory());
        }

        return jsonSerializerOptions;
    }
}

public sealed class StrongIdJsonConverterFactory : JsonConverterFactory
{
    public override Boolean CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return TryGetStrongIdType(typeToConvert, out _, out _);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        if (!TryGetStrongIdType(typeToConvert, out var strongIdType, out var valueType))
        {
            throw new InvalidOperationException($"Type '{typeToConvert.FullName}' is not a supported Strong ID.");
        }

        if (Nullable.GetUnderlyingType(typeToConvert) is not null)
        {
            return (JsonConverter)Activator.CreateInstance(typeof(NullableStrongIdJsonConverter<,>).MakeGenericType(strongIdType, valueType))!;
        }

        return (JsonConverter)Activator.CreateInstance(typeof(StrongIdJsonConverter<,>).MakeGenericType(strongIdType, valueType))!;
    }

    private static Boolean TryGetStrongIdType(Type candidateType, out Type strongIdType, out Type valueType)
    {
        var nullableUnderlyingType = Nullable.GetUnderlyingType(candidateType);
        strongIdType = nullableUnderlyingType ?? candidateType;

        var interfaceType = strongIdType
            .GetInterfaces()
            .FirstOrDefault(static interfaceCandidate =>
                interfaceCandidate.IsGenericType
                && interfaceCandidate.GetGenericTypeDefinition() == typeof(IStrongId<>));

        if (interfaceType is null)
        {
            valueType = typeof(Object);
            return false;
        }

        valueType = interfaceType.GetGenericArguments()[0];
        return true;
    }
}

internal sealed class StrongIdJsonConverter<TStrongId, TValue> : JsonConverter<TStrongId>
    where TStrongId : IStrongId<TValue>
{
    private static readonly MethodInfo FromMethod = ResolveFromMethod();

    public override TStrongId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException($"Cannot deserialize null into Strong ID type '{typeof(TStrongId).FullName}'.");
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return (TStrongId)FromMethod.Invoke(null, [value])!;
    }

    public override void Write(Utf8JsonWriter writer, TStrongId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        JsonSerializer.Serialize(writer, value.Value, options);
    }

    private static MethodInfo ResolveFromMethod()
    {
        var method = typeof(TStrongId).GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(TValue)],
            modifiers: null);

        if (method is null || method.ReturnType != typeof(TStrongId))
        {
            throw new InvalidOperationException($"Strong ID type '{typeof(TStrongId).FullName}' must expose a public static From({typeof(TValue).FullName}) method.");
        }

        return method;
    }
}

internal sealed class NullableStrongIdJsonConverter<TStrongId, TValue> : JsonConverter<TStrongId?>
    where TStrongId : struct, IStrongId<TValue>
{
    private static readonly MethodInfo FromMethod = ResolveFromMethod();

    public override TStrongId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return (TStrongId)FromMethod.Invoke(null, [value])!;
    }

    public override void Write(Utf8JsonWriter writer, TStrongId? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value.Value, options);
    }

    private static MethodInfo ResolveFromMethod()
    {
        var method = typeof(TStrongId).GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(TValue)],
            modifiers: null);

        if (method is null || method.ReturnType != typeof(TStrongId))
        {
            throw new InvalidOperationException($"Strong ID type '{typeof(TStrongId).FullName}' must expose a public static From({typeof(TValue).FullName}) method.");
        }

        return method;
    }
}
