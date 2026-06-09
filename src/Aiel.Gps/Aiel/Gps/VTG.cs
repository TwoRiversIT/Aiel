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

using System.Buffers;
using System.Text;

namespace Aiel.Gps;

public class VTG : NmeaMessage
{
    public const String Identifier = "GPVTG";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over GPVTG
        lexer.SkipString();

        return new VTG()
        {
            TrueTrack = lexer.NextDouble(),
            TrueTrackIndicator = lexer.NextChar(),
            MagneticTrack = lexer.NextDouble(),
            MagneticTrackIndicator = lexer.NextChar(),
            GroundSpeedN = lexer.NextDouble(),
            GroundSpeedNIndicator = lexer.NextChar(),
            GroundSpeedK = lexer.NextDouble(),
            GroundSpeedKIndicator = lexer.NextChar(),
            ModeIndicator = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }

    public Double TrueTrack { get; private set; }
    public Char TrueTrackIndicator { get; set; }
    public Double MagneticTrack { get; private set; }
    public Char MagneticTrackIndicator { get; private set; }
    public Double GroundSpeedN { get; private set; }
    public Char GroundSpeedNIndicator { get; private set; }
    public Double GroundSpeedK { get; private set; }
    public Char GroundSpeedKIndicator { get; private set; }
    public Char ModeIndicator { get; private set; }

    public override String ToString() => $"GPVTG {TrueTrack} {MagneticTrack} {GroundSpeedN} {GroundSpeedK}";
}
