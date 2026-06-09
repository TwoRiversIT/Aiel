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
/// Interface for custom NMEA parsers that can be registered at runtime.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the compile-time <see cref="INmeaParser{TMessage}"/> interface, this interface
/// returns a boxed <see cref="Object"/>. This is necessary because custom parsers are
/// registered at runtime and their message types cannot be known at compile time.
/// </para>
/// <para>
/// Each parse operation will allocate one object (the boxed message). For high-throughput
/// scenarios where allocation matters, consider using the source-generated discriminated
/// union approach with <see cref="NmeaMessageAttribute"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class MyCustomParser : ICustomNmeaParser
/// {
///     public ReadOnlySpan&lt;Byte&gt; Identifier =&gt; "MYCST"u8;
///
///     public Object Parse(ref Lexer lexer)
///     {
///         return new MyCustomMessage
///         {
///             Field1 = lexer.NextInteger(),
///             Field2 = lexer.NextString()
///         };
///     }
/// }
/// </code>
/// </example>
public interface ICustomNmeaParser
{
    /// <summary>
    /// Gets the NMEA sentence identifier that this parser handles.
    /// </summary>
    /// <remarks>
    /// This should return the identifier without the '$' prefix or talker ID.
    /// For example, for "$GPMYCST,..." sentences, return "MYCST"u8 or "GPMYCST"u8
    /// depending on whether you want to match with or without the talker ID.
    /// </remarks>
    ReadOnlySpan<Byte> Identifier { get; }

    /// <summary>
    /// Parses an NMEA sentence and returns the message as a boxed object.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence fields.</param>
    /// <returns>The parsed message, boxed as an <see cref="Object"/>.</returns>
    /// <remarks>
    /// The lexer will be positioned after the sentence identifier (first comma).
    /// The parser should read all fields and optionally validate the checksum.
    /// </remarks>
    Object Parse(ref Lexer lexer);
}
