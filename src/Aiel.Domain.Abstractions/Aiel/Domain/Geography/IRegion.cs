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
/// Represents a geographical region with a name and code. Usually a US State
/// or a Canadian Province. This interface defines the contract for any region
/// implementation, allowing for consistent access to region information
/// across different parts of the application.
/// </summary>
public interface IRegion
{
    /// <summary>
    /// Gets the name of the region.
    /// </summary>
    String Name { get; }

    /// <summary>
    /// Gets the code of the region.
    /// </summary>
    String Code { get; }
}

/// <summary>
/// Represents an empty or uninitialized region. This can be used as a
/// default value when no valid region is available.
/// </summary>
public sealed record Region() : IRegion
{
    /// <summary>
    /// Gets a singleton instance of an empty region. This can be used to represent
    /// a default value when no valid region is available.
    /// </summary>
    public static readonly Region Empty = new();

    /// <summary>
    /// Gets the name of the region. For the empty region, this will always return an empty string.
    /// </summary>
    public String Name => String.Empty;

    /// <summary>
    /// Gets the code of the region. For the empty region, this will always return an empty string.
    /// </summary>
    public String Code => String.Empty;
}
