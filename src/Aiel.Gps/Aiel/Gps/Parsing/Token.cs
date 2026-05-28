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

namespace Aiel.Gps.Parsing;

public class Token
{
    private readonly Int64 _index;
    private readonly Double? _number;

    public Token(Int64 index, TokenType type)
        : this(index, type, null)
    {
    }

    public Token(Int64 index, TokenType type, String? letters)
    {
        _index = index;
        TokenType = type;
        Letters = letters ?? String.Empty;
    }

    public Token(Int64 index, TokenType type, Double number)
    {
        _index = index;
        TokenType = type;
        _number = number;
    }

    public TokenType TokenType { get; }
    public Double Number => _number ?? 0.0d;
    public String Letters { get; } = String.Empty;
    public override String ToString() => _number.HasValue ? $"Token({TokenType}, {_number}) at {_index}" : $"Token({TokenType}) at {_index}";
}
