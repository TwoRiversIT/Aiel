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

using Aiel.Gps.Parsing;
using System.Buffers;

namespace Aiel.Gps;

/// <summary>
/// Represents the base class for all NMEA message types.
/// </summary>
/// <remarks>
/// This abstract class defines the contract for parsing NMEA 0183 sentences.
/// Derived classes implement specific message types (GGA, RMC, GSA, etc.) and provide
/// parsing logic for their respective sentence formats.
/// </remarks>
public abstract class NmeaMessage
{
    /// <summary>
    /// Gets the NMEA sentence identifier key for this message type.
    /// </summary>
    /// <remarks>
    /// This is typically a 6-byte sequence like "$GPGGA" or "$GPRMC" that identifies the sentence type.
    /// </remarks>
    protected abstract ReadOnlyMemory<Byte> Key { get; }

    /// <summary>
    /// Determines whether this parser can handle the specified NMEA sentence.
    /// </summary>
    /// <param name="sentence">The NMEA sentence to check.</param>
    /// <returns>True if this parser can handle the sentence; otherwise, false.</returns>
    /// <remarks>
    /// The default implementation checks if the sentence starts with the parser's <see cref="Key"/> value.
    /// Override this method to provide custom sentence detection logic.
    /// </remarks>
    public virtual Boolean CanHandle(ReadOnlySequence<Byte> sentence)
    {
        if (sentence.Length == 0 || sentence.Length < Key.Span.Length)
        {
            return false;
        }

        return sentence.Slice(0, 6).IsMatch(Key.Span);
    }

    /// <summary>
    /// Parses the specified NMEA sentence into a strongly-typed message object.
    /// </summary>
    /// <param name="sentence">The NMEA sentence to parse.</param>
    /// <returns>A parsed <see cref="NmeaMessage"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sentence cannot be parsed.</exception>
    /// <remarks>
    /// Implementations should use the <see cref="OriginalLexer"/> class to parse individual fields from the sentence.
    /// </remarks>
    public abstract NmeaMessage Parse(ReadOnlySequence<Byte> sentence);

    /// <summary>
    /// Gets the checksum value from the NMEA sentence.
    /// </summary>
    /// <remarks>
    /// The checksum is a hexadecimal value that appears at the end of the sentence after an asterisk (*).
    /// It is calculated as the XOR of all bytes between the $ and * characters.
    /// </remarks>
    public Int32 Checksum { get; protected set; }
}
