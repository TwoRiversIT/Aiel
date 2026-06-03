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

namespace Aiel.StrongIds;

/// <summary>
/// When applied to a struct or class, indicates that the source generator should generate a strong ID type
/// with the specified underlying value type.
/// </summary>
/// <typeparam name="TValue">The underlying value type for the strong ID.</typeparam>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class StrongIdAttribute<TValue> : Attribute
{
    /// <summary>
    /// When true, the generated strong ID type will not allow the default value of the underlying type.
    /// For example, if the underlying type is Guid, then Guid.Empty will not be allowed. If the
    /// underlying type is string, then null and empty string will not be allowed. Default is true.
    /// </summary>
    public Boolean DisallowDefault { get; init; } = true;

    // ToDo: This does not seem to be used anywhere. Do we need it? If not, we should remove it.
    public StrongIdBackingKind BackingKind { get; init; } = StrongIdBackingKind.Value;

    /// <summary>
    /// When true, the source generator will generate a TryFrom method for the strong ID type.
    /// Default is true.
    /// </summary>
    public Boolean GenerateTryFrom { get; init; } = true;
}

public enum StrongIdBackingKind
{
    Value,
    Reference,
}
