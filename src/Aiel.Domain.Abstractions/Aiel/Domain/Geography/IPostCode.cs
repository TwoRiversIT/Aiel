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

namespace Aiel.Domain.Geography;

/// <summary>
/// Represents a Canadian Postal Code or a US ZIP code in a geographic context.
/// </summary>
public interface IPostCode
{

    /// <summary>
    /// Gets the human-readable postal code or ZIP code.
    /// </summary>
    String Code { get; }
}

/// <summary>
/// Represents an empty or uninitialized postal code. This can be used as a
/// default value when no valid postal code is available.
/// </summary>
public sealed record PostCode() : IPostCode
{
    /// <summary>
    /// Gets a singleton instance of an empty postal code. This can be used to represent
    /// </summary>
    public static readonly PostCode Empty = new();

    /// <summary>
    /// Gets the human-readable postal code or ZIP code. For the empty postal code, this will always return an empty string.
    /// </summary>
    public String Code => String.Empty;
}
