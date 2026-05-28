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

public class GFDTA : NmeaMessage
{
    public const String Identifier = "GFDTA";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over the $GFDTA
        lexer.SkipString();

        var dta = new GFDTA
        {
            Concentration = lexer.NextDouble(),
            R2 = lexer.NextInteger(),
            Distance = lexer.NextDouble(),
            Light = lexer.NextInteger(),
            DateTime = lexer.NextDateTime(),
            SerialNumber = lexer.NextStringSlice(),
            Status = lexer.NextStringSlice(),
            Checksum = lexer.NextChecksum()
        };

        return dta;
    }

    public Double Concentration { get; private set; }
    public Int32 R2 { get; private set; }
    public Double Distance { get; private set; }
    public Int32 Light { get; private set; }
    public DateTime DateTime { get; private set; }
    public ReadOnlySequence<Byte> SerialNumber { get; private set; }
    public ReadOnlySequence<Byte> Status { get; private set; }
    public override String ToString() => $"{Identifier} {Concentration} {R2} {Distance} {Light} {DateTime} {SerialNumber} {Status}";
}

