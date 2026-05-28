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
/// Statistics collected during batch reading of NMEA sentences.
/// </summary>
public sealed class BatchReaderStatistics
{
    private Int64 _bytesRead;
    private Int64 _totalSentences;
    private Int64 _parsedMessages;
    private Int64 _customMessages;
    private Int64 _errors;

    /// <summary>
    /// Gets the total number of bytes read from the stream.
    /// </summary>
    public Int64 BytesRead => Interlocked.Read(ref _bytesRead);

    /// <summary>
    /// Gets the total number of NMEA sentences encountered.
    /// </summary>
    public Int64 TotalSentences => Interlocked.Read(ref _totalSentences);

    /// <summary>
    /// Gets the number of sentences successfully parsed into built-in messages.
    /// </summary>
    public Int64 ParsedMessages => Interlocked.Read(ref _parsedMessages);

    /// <summary>
    /// Gets the number of sentences parsed by custom (runtime-registered) parsers.
    /// </summary>
    public Int64 CustomMessages => Interlocked.Read(ref _customMessages);

    /// <summary>
    /// Gets the number of sentences that could not be parsed.
    /// </summary>
    public Int64 Errors => Interlocked.Read(ref _errors);

    /// <summary>
    /// Adds to the bytes read counter.
    /// </summary>
    internal void AddBytesRead(Int64 bytes) => Interlocked.Add(ref _bytesRead, bytes);

    /// <summary>
    /// Increments the total sentences counter.
    /// </summary>
    internal void IncrementTotalSentences() => Interlocked.Increment(ref _totalSentences);

    /// <summary>
    /// Increments the parsed messages counter (built-in messages).
    /// </summary>
    internal void IncrementParsedMessages() => Interlocked.Increment(ref _parsedMessages);

    /// <summary>
    /// Increments the custom messages counter.
    /// </summary>
    internal void IncrementCustomMessages() => Interlocked.Increment(ref _customMessages);

    /// <summary>
    /// Increments the errors counter.
    /// </summary>
    internal void IncrementErrors() => Interlocked.Increment(ref _errors);

    /// <summary>
    /// Returns a string representation of the statistics.
    /// </summary>
    public override String ToString()
        => $"BatchReaderStatistics {{ BytesRead={BytesRead}, TotalSentences={TotalSentences}, ParsedMessages={ParsedMessages}, CustomMessages={CustomMessages}, Errors={Errors} }}";
}
