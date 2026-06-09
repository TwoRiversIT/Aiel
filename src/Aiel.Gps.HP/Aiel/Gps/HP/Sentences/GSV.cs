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
/// Represents a satellite information record used in GSV messages.
/// </summary>
/// <remarks>
/// Contains the four pieces of information typically provided for each satellite:
/// PRN number, elevation, azimuth, and signal-to-noise ratio.
/// </remarks>
public struct SatelliteInfo
{
    /// <summary>
    /// Satellite PRN (Pseudo-Random Noise) number (01-32 for GPS).
    /// This is the unique identifier for the satellite.
    /// </summary>
    public Int32 PRN;

    /// <summary>
    /// Elevation above horizon in degrees (00-90).
    /// 90 degrees means the satellite is directly overhead.
    /// </summary>
    public Int32 Elevation;

    /// <summary>
    /// Azimuth angle in degrees from true north (000-359).
    /// 0 degrees is north, 90 is east, 180 is south, 270 is west.
    /// </summary>
    public Int32 Azimuth;

    /// <summary>
    /// Signal-to-Noise Ratio in dB (00-99).
    /// Higher values indicate stronger signal reception. Values above 40 are considered good.
    /// A value of 0 indicates the satellite is tracked but no signal is received.
    /// </summary>
    public Int32 SNR;

    /// <summary>Gets a value indicating whether this satellite information is valid (has a PRN).</summary>
    public readonly Boolean IsValid => PRN > 0;

    /// <inheritdoc/>
    public override readonly String ToString() => IsValid ? $"PRN{PRN:D2} El:{Elevation:D2}° Az:{Azimuth:D3}° SNR:{SNR:D2}dB" : "Empty";

    /// <summary>
    /// Creates a new SatelliteInfo with the specified values.
    /// </summary>
    /// <param name="prn">The satellite PRN number.</param>
    /// <param name="elevation">The elevation angle in degrees.</param>
    /// <param name="azimuth">The azimuth angle in degrees.</param>
    /// <param name="snr">The signal-to-noise ratio in dB.</param>
    /// <returns>A new SatelliteInfo structure.</returns>
    public static SatelliteInfo Create(Int32 prn, Int32 elevation, Int32 azimuth, Int32 snr)
    {
        return new SatelliteInfo
        {
            PRN = prn,
            Elevation = elevation,
            Azimuth = azimuth,
            SNR = snr
        };
    }
}

/// <summary>
/// Represents a GSV (GPS Satellites in View) NMEA message.
/// </summary>
/// <remarks>
/// <para>
/// This sentence provides information about GPS satellites in view including their PRN numbers,
/// elevation, azimuth, and signal-to-noise ratio. Multiple GSV sentences may be required to
/// report information for all satellites in view.
/// </para>
/// <para>
/// Example sentence: $GPGSV,2,1,08,01,40,083,46,02,17,308,41,12,07,344,39,14,22,228,45*75
/// </para>
/// </remarks>
[NmeaMessage("GPGSV")]
public struct GSV
{
    /// <summary>Total number of GSV messages in this cycle.</summary>
    public Int32 TotalMessages;

    /// <summary>Message number in the sequence (1-based).</summary>
    public Int32 MessageNumber;

    /// <summary>Total number of satellites in view.</summary>
    public Int32 SatellitesInView;

    /// <summary>Satellite 1 information (always present if any satellites).</summary>
    public SatelliteInfo SV1;

    /// <summary>Satellite 2 information (may be empty if not enough satellites).</summary>
    public SatelliteInfo SV2;

    /// <summary>Satellite 3 information (may be empty if not enough satellites).</summary>
    public SatelliteInfo SV3;

    /// <summary>Satellite 4 information (may be empty if not enough satellites).</summary>
    public SatelliteInfo SV4;

    /// <summary>NMEA checksum value.</summary>
    public Int32 Checksum;

    /// <summary>Gets the count of valid satellites in this message.</summary>
    public readonly Int32 ValidSatelliteCount
    {
        get
        {
            var count = 0;
            if (SV1.IsValid)
            {
                count++;
            }

            if (SV2.IsValid)
            {
                count++;
            }

            if (SV3.IsValid)
            {
                count++;
            }

            if (SV4.IsValid)
            {
                count++;
            }

            return count;
        }
    }

    /// <inheritdoc/>
    public override readonly String ToString() => $"{nameof(GSV)} {MessageNumber}/{TotalMessages} InView:{SatellitesInView} Valid:{ValidSatelliteCount}";
}

/// <summary>
/// Parser for GSV (GPS Satellites in View) NMEA sentences.
/// </summary>
/// <remarks>
/// The GSV sentence can be sent multiple times to cover all satellites in view.
/// Each message can contain information for up to 4 satellites.
/// </remarks>
[NmeaParser(typeof(GSV))]
public readonly struct GsvParser : INmeaParser<GSV>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for GSV sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GPGSV"u8;

    /// <summary>
    /// Parses a GSV NMEA sentence into a GSV structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed GSV message.</param>
    /// <remarks>
    /// Expected format: $GPGSV,total_msg,msg_num,sats_in_view,prn1,elev1,azim1,snr1,prn2,elev2,azim2,snr2,prn3,elev3,azim3,snr3,prn4,elev4,azim4,snr4*checksum
    /// </remarks>
    public void Parse(ref Lexer lexer, out GSV msg)
    {
        // Skip the sentence identifier (e.g., "GPGSV")
        lexer.ConsumeString();

        msg = new GSV()
        {
            TotalMessages = lexer.NextInteger(),
            MessageNumber = lexer.NextInteger(),
            SatellitesInView = lexer.NextInteger()
        };

        // Parse up to 4 satellite entries
        // Each satellite has: PRN, elevation, azimuth, SNR
        if (!lexer.EOL)
        {
            msg.SV1 = SatelliteInfo.Create(
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            msg.SV2 = SatelliteInfo.Create(
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            msg.SV3 = SatelliteInfo.Create(
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            msg.SV4 = SatelliteInfo.Create(
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger(),
                lexer.NextInteger());
        }

        msg.Checksum = lexer.NextChecksum();
    }
}
