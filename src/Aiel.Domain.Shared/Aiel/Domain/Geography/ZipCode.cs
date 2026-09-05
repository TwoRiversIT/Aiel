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
/// Represents a US ZIP code with an optional 4-digit extension (ZIP+4).
/// </summary>
/// <param name="Zip">The 5-digit ZIP code.</param>
/// <param name="PlusFour">The optional 4-digit ZIP+4 extension.</param>
public sealed record ZipCode(Int32 Zip, Int32 PlusFour = 0) : IPostCode
{
    /// <summary>
    /// Gets the full ZIP code as a string in the format "ZIP" or "ZIP-PlusFour" if the PlusFour is greater than 0.
    /// </summary>
    public String Code => PlusFour > 0 ? $"{Zip}-{PlusFour:D4}" : Zip.ToString();

    /// <summary>
    /// Returns a string representation of the ZIP code in the format "ZIP" or "ZIP-PlusFour" if the PlusFour is greater than 0.
    /// </summary>
    /// <returns>The string representation of the ZIP code.</returns>
    public override String ToString() => Code;
}
