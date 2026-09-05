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
/// Represents a Canadian postal code, consisting of a Forward Sortation Area (FSA) and a Local Delivery Unit (LDU).
/// </summary>
/// <param name="FSA">The Forward Sortation Area of the postal code.</param>
/// <param name="LDU">The Local Delivery Unit of the postal code.</param>
public sealed record PostalCode(String FSA, String LDU) : IPostCode
{
    /// <summary>
    /// Gets an empty <see cref="PostalCode"/> instance with empty FSA and LDU.
    /// </summary>
    public static readonly PostalCode Empty = new(String.Empty, String.Empty);

    /// <summary>
    /// Gets the full postal code as a string in the format "FSA LDU".
    /// </summary>
    public String Code => $"{FSA} {LDU}";

    /// <summary>
    /// Returns a string representation of the postal code in the format "FSA LDU".
    /// </summary>
    /// <returns>The full postal code as a string.</returns>
    public override String ToString() => Code;
}
