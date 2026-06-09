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
using System.Text.Json.Serialization.Metadata;

namespace Aiel.Results;

/// <summary>
/// Supports internal fuctionality and is not intended for public use.
/// </summary>
public static class PolymorphismInitializer
{
    private static readonly List<Action<JsonTypeInfo>> Hooks = [];

    /// <summary>
    /// Supports internal fuctionality and is not intended for public use.
    /// </summary>
    public static void Register(Action<JsonTypeInfo> hook) => Hooks.Add(hook);

    /// <summary>
    /// Supports internal fuctionality and is not intended for public use.
    /// </summary>
    public static void Apply(JsonTypeInfo ti)
    {
        foreach (var hook in Hooks)
        {
            hook(ti);
        }
    }
}

/// <summary>
/// A JSON type info resolver that applies polymorphism configuration for Error and ErrorCode types.
/// </summary>
internal sealed class ResultPatternPolymorphismResolver : IJsonTypeInfoResolver
{
    private readonly DefaultJsonTypeInfoResolver _resolver = new()
    {
        Modifiers =
        {
            PolymorphismInitializer.Apply
        }
    };

    /// <inheritdoc />
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        => _resolver.GetTypeInfo(type, options);
}
