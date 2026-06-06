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
/// Represents an error that occurred while parsing an NMEA sentence.
/// </summary>
/// <remarks>
/// This class captures information about sentences that could not be parsed,
/// allowing consumers to inspect and handle parse failures.
/// </remarks>
public sealed class ParseError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParseError"/> class.
    /// </summary>
    /// <param name="lineNumber">The line number in the stream where the error occurred.</param>
    /// <param name="rawPayload">The raw bytes of the sentence that failed to parse.</param>
    /// <param name="parserType">The type of parser that attempted to parse the sentence.</param>
    /// <param name="exception">The exception that was thrown during parsing.</param>
    public ParseError(Int32 lineNumber, String rawPayload, Type parserType, Exception exception)
    {
        LineNumber = lineNumber;
        RawPayload = rawPayload;
        ParserType = parserType;
        Exception = exception;
        Sentence = rawPayload;
        ErrorMessage = exception.Message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseError"/> class with a simple error message.
    /// </summary>
    /// <param name="sentence">The sentence that failed to parse.</param>
    /// <param name="errorMessage">A description of why the parse failed.</param>
    public ParseError(String sentence, String errorMessage)
    {
        Sentence = sentence;
        ErrorMessage = errorMessage;
        RawPayload = sentence;
        LineNumber = 0;
        ParserType = null;
        Exception = null;
    }

    /// <summary>
    /// Gets the line number in the stream where the error occurred.
    /// </summary>
    public Int32 LineNumber { get; }

    /// <summary>
    /// Gets the raw payload of the sentence that failed to parse.
    /// </summary>
    public String RawPayload { get; }

    /// <summary>
    /// Gets the type of parser that attempted to parse the sentence.
    /// </summary>
    public Type? ParserType { get; }

    /// <summary>
    /// Gets the exception that was thrown during parsing.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the sentence that failed to parse.
    /// </summary>
    public String Sentence { get; }

    /// <summary>
    /// Gets a description of why the parse failed.
    /// </summary>
    public String ErrorMessage { get; }

    /// <inheritdoc/>
    public override String ToString()
    {
        if (ParserType != null)
        {
            return $"Line {LineNumber}: {ParserType.Name} failed to parse '{RawPayload}' - {Exception?.Message}";
        }

        return $"Parse error: '{Sentence}' - {ErrorMessage}";
    }
}

