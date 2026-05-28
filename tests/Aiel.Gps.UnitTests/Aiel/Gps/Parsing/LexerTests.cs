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

namespace Aiel.Gps.Parsing;

[Collection("Lexar")]
public class LexerTests
{
    [Fact]
    public void NextDate_parses_supported_date_formats()
    {
        var bytes = Encoding.UTF8.GetBytes("$BLARGH,2018/10/16,18/10/16,161074,20181016*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);
        var lexer = new Lexer(buffer);

        lexer.NextString().Should().Be("BLARGH");
        lexer.NextDate().Should().Be(new DateOnly(2018, 10, 16));
        lexer.NextDate().Should().Be(new DateOnly(2016, 10, 18));
        lexer.NextDate().Should().Be(new DateOnly(1974, 10, 16));
        lexer.NextDate().Should().Be(new DateOnly(2018, 10, 16));
    }

    [Fact]
    public void NextTime_parses_supported_time_formats()
    {
        var bytes = Encoding.UTF8.GetBytes("$BLARGH,010203.55,232608.666,222001,20:18:10*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);
        var lexer = new Lexer(buffer);

        lexer.NextString().Should().Be("BLARGH");
        lexer.NextTime().Should().Be(new TimeOnly(1, 2, 3, 550));
        lexer.NextTime().Should().Be(new TimeOnly(23, 26, 8, 666));
        lexer.NextTime().Should().Be(new TimeOnly(22, 20, 1));
        lexer.NextTime().Should().Be(new TimeOnly(20, 18, 10));
    }

    [Fact]
    public void NextValue_methods_should_return_correct_values()
    {
        var bytes = Encoding.UTF8.GetBytes("$BLARGH,232608.000,5057.1975,N,11134.8332,W,2,,781.7,M,161074,FF,2018/10/16 13:35:55*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);
        var lexer = new Lexer(buffer);

        lexer.NextString().Should().Be("BLARGH");
        lexer.NextTime().Should().Be(new TimeOnly(23, 26, 8));
        lexer.NextLatitude().Should().Be(50.953291666666665d);
        lexer.NextLongitude().Should().Be(-111.58055333333333d);
        lexer.NextInteger().Should().Be(2);
        lexer.NextInteger().Should().Be(0);
        lexer.NextDouble().Should().Be(781.7);
        lexer.NextChar().Should().Be('M');
        lexer.NextDate().Should().Be(new DateOnly(1974, 10, 16));
        lexer.NextHexadecimal().Should().Be(0xff);
        lexer.NextDateTime().Should().Be(new DateTime(2018, 10, 16, 13, 35, 55));
        lexer.NextChecksum().Should().Be(0x62);
    }

    [Fact]
    public void Advancing_past_the_end_throws()
    {
        var bytes = Encoding.UTF8.GetBytes("$BLARGH,,*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);
        var lexer = new Lexer(buffer);

        lexer.NextString().Should().Be("BLARGH");
        lexer.NextDouble().Should().Be(Double.NaN);
        lexer.NextChecksum().Should().Be(0x62);
    }
}
