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

namespace Aiel.Gps.HP;

/// <summary>
/// Base interface for NMEA sentence parsers.
/// </summary>
public interface INmeaParser
{
    /// <summary>
    /// Gets the NMEA sentence identifier that this parser handles (e.g., "GPGLL", "GPRMC").
    /// </summary>
    ReadOnlySpan<Byte> Identifier { get; }
}

/// <summary>
/// Defines a parser for NMEA sentences of a specific message type.
/// </summary>
/// <typeparam name="TMessage">The type of message this parser produces.</typeparam>
public interface INmeaParser<TMessage> : INmeaParser
    where TMessage : struct
{
    /// <summary>
    /// Parses an NMEA sentence into a message of type TMessage.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="message">The parsed message.</param>
    void Parse(ref Lexer lexer, out TMessage message);
}
