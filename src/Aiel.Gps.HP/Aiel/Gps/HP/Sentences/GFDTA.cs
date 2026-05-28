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

namespace Aiel.Gps.HP.Sentences;

/// <summary>
/// This is my GFDTA. There are many like it, but this one is mine.
/// </summary>
/// <remarks>
/// This sentence provides fluorometer measurement data including concentration, distance, light levels, and status information.
/// </remarks>
[NmeaMessage("GFDTA")]
public struct GFDTA
{
    /// <summary>
    /// Gets the concentration measurement value.
    /// </summary>
    public Double Concentration;

    /// <summary>
    /// Gets the R2 (coefficient of determination) value.
    /// </summary>
    public Int32 R2;

    /// <summary>
    /// Gets the distance measurement value.
    /// </summary>
    public Double Distance;

    /// <summary>
    /// Gets the light level measurement.
    /// </summary>
    public Int32 Light;

    /// <summary>
    /// Gets the date and time of the measurement.
    /// </summary>
    public DateTime DateTime;

    /// <summary>
    /// Gets the serial number of the device.
    /// </summary>
    public String SerialNumber;

    /// <summary>
    /// Gets the status of the measurement.
    /// </summary>
    public String Status;

    /// <summary>
    /// Gets the NMEA checksum value.
    /// </summary>
    public Int32 Checksum;

    /// <inheritdoc/>
    public override readonly String ToString()
        => $"{nameof(GFDTA)} {Concentration} {R2} {Distance} {Light} {DateTime} {SerialNumber} {Status}";
}

/// <summary>
/// Parser for GFDTA (Fluorometer Data) NMEA sentences.
/// </summary>
[NmeaParser(typeof(GFDTA))]
public readonly struct GfdtaParser : INmeaParser<GFDTA>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for GFDTA sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GFDTA"u8;

    /// <summary>
    /// Parses a GFDTA NMEA sentence into a GFDTA structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed GFDTA message.</param>
    public void Parse(ref Lexer lexer, out GFDTA msg)
    {
        // Skip the sentence identifier (e.g., "GFDTA")
        lexer.ConsumeString();

        msg = new GFDTA()
        {
            Concentration = lexer.NextDouble(),
            R2 = lexer.NextInteger(),
            Distance = lexer.NextDouble(),
            Light = lexer.NextInteger(),
            DateTime = lexer.NextDateTime(),
            SerialNumber = lexer.NextString(),
            Status = lexer.NextString(),
            Checksum = lexer.NextChecksum()
        };
    }
}

