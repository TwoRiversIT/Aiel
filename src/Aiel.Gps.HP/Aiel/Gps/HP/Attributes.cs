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
/// Marks a struct as an NMEA message type that should be included in the generated discriminated union.
/// </summary>
/// <remarks>
/// Apply this attribute to message structs (like GLL, GGA, RMC) to include them in the
/// source-generated <c>NmeaMessage</c> discriminated union. The generator will create
/// type-safe accessors and pattern matching support for all marked types.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="NmeaMessageAttribute"/> class.
/// </remarks>
/// <param name="identifier">The NMEA sentence identifier (e.g., "GPGLL", "GPRMC").</param>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class NmeaMessageAttribute(String identifier) : Attribute
{

    /// <summary>
    /// Gets the NMEA sentence identifier for this message type.
    /// </summary>
    public String Identifier { get; } = identifier;
}

/// <summary>
/// Marks a struct as the parser for an NMEA message type.
/// </summary>
/// <remarks>
/// Apply this attribute to parser structs that implement <see cref="INmeaParser{TMessage}"/>.
/// The generator will use these parsers to dispatch parsing based on the sentence identifier.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="NmeaParserAttribute"/> class.
/// </remarks>
/// <param name="messageType">The type of message this parser produces.</param>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class NmeaParserAttribute(Type messageType) : Attribute
{

    /// <summary>
    /// Gets the type of message this parser produces.
    /// </summary>
    public Type MessageType { get; } = messageType;
}
