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
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace Aiel.Gps.HP;

/// <summary>
/// High-performance batch reader for NMEA sentences from a stream.
/// </summary>
/// <remarks>
/// <para>
/// This class uses <see cref="System.IO.Pipelines"/> for efficient I/O and provides
/// an async enumerable interface for processing messages. Sentences that cannot be
/// parsed are available through <see cref="ReadErrorsAsync"/>.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var reader = new NmeaBatchReader(stream);
/// await foreach (var message in reader.ReadAsync())
/// {
///     message.Match(
///         onGLL: gll => Console.WriteLine($"Position: {gll.Latitude}, {gll.Longitude}"),
///         onGFDTA: gfdta => Console.WriteLine($"Concentration: {gfdta.Concentration}"));
/// }
/// </code>
/// </para>
/// <para>
/// For custom message types, provide a <see cref="NmeaParserRegistry"/>:
/// <code>
/// var registry = new NmeaParserRegistry();
/// registry.Register(new MyCustomParser());
///
/// var reader = new NmeaBatchReader(stream, registry: registry);
/// await foreach (var custom in reader.ReadCustomMessagesAsync())
/// {
///     // Handle custom messages (boxed)
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class NmeaBatchReader
{
    private const Byte Dollar = (Byte)'$';
    private const Byte CarriageReturn = (Byte)'\r';
    private const Byte LineFeed = (Byte)'\n';

    private readonly PipeReader _pipeReader;
    private readonly NmeaParserRegistry? _registry;
    private readonly Channel<ParseError> _errorChannel;
    private readonly Channel<Object> _customMessageChannel;
    private readonly Boolean _leaveOpen;

    private Boolean _started;

#if NET9_0_OR_GREATER
    private readonly Lock _syncRoot = new();
#else
    private readonly Object _syncRoot = new();
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="NmeaBatchReader"/> class.
    /// </summary>
    /// <param name="stream">The stream containing NMEA data.</param>
    /// <param name="leaveOpen">True to leave the stream open after the reader is disposed.</param>
    /// <param name="registry">Optional registry for custom parsers.</param>
    public NmeaBatchReader(Stream stream, Boolean leaveOpen = false, NmeaParserRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        _pipeReader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: leaveOpen));
        _registry = registry;
        _errorChannel = Channel.CreateUnbounded<ParseError>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });
        _customMessageChannel = Channel.CreateUnbounded<Object>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NmeaBatchReader"/> class.
    /// </summary>
    /// <param name="pipeReader">The pipe reader containing NMEA data.</param>
    /// <param name="registry">Optional registry for custom parsers.</param>
    public NmeaBatchReader(PipeReader pipeReader, NmeaParserRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(pipeReader);

        _pipeReader = pipeReader;
        _registry = registry;
        _errorChannel = Channel.CreateUnbounded<ParseError>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });
        _customMessageChannel = Channel.CreateUnbounded<Object>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });
        _leaveOpen = true; // We don't own it, so don't dispose
    }

    /// <summary>
    /// Gets the statistics collected during reading.
    /// </summary>
    public BatchReaderStatistics Statistics { get; } = new();

    /// <summary>
    /// Reads NMEA messages from the stream as an async enumerable.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the read operation.</param>
    /// <returns>An async enumerable of parsed NMEA messages (built-in types only).</returns>
    /// <exception cref="InvalidOperationException">Thrown if ReadAsync is called more than once.</exception>
    /// <remarks>
    /// Custom messages parsed via the registry are available through <see cref="ReadCustomMessagesAsync"/>.
    /// </remarks>
    public async IAsyncEnumerable<NmeaMessage> ReadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureNotStarted();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ReadResult result;
                try
                {
                    result = await _pipeReader.ReadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                var buffer = result.Buffer;
                Statistics.AddBytesRead(buffer.Length);

                // Process all complete sentences in the buffer
                while (TryReadSentence(ref buffer, out var sentence))
                {
                    Statistics.IncrementTotalSentences();

                    // Copy sentence to contiguous memory for parsing
                    var sentenceBytes = GetContiguousBytes(sentence);

                    // Try to parse as built-in message first
                    if (NmeaMessage.TryParse(sentenceBytes, out var message))
                    {
                        Statistics.IncrementParsedMessages();
                        yield return message;
                    }
                    // Try custom parser if registry is available
                    else if (TryParseCustom(sentenceBytes, out var customMessage))
                    {
                        Statistics.IncrementCustomMessages();
                        _customMessageChannel.Writer.TryWrite(customMessage!);
                    }
                    else
                    {
                        Statistics.IncrementErrors();
                        var error = new ParseError(
                            Encoding.UTF8.GetString(sentenceBytes),
                            "Unknown or unparseable sentence identifier");
                        _errorChannel.Writer.TryWrite(error);
                    }
                }

                // Tell the PipeReader how much of the buffer has been consumed
                _pipeReader.AdvanceTo(buffer.Start, buffer.End);

                // Stop if there's no more data coming
                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        finally
        {
            // Complete all channels
            _errorChannel.Writer.Complete();
            _customMessageChannel.Writer.Complete();

            // Complete the pipe reader
            await _pipeReader.CompleteAsync();
        }
    }

    /// <summary>
    /// Reads parse errors from the stream as an async enumerable.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the read operation.</param>
    /// <returns>An async enumerable of parse errors.</returns>
    /// <remarks>
    /// This method can be enumerated concurrently with <see cref="ReadAsync"/> to handle
    /// both successful messages and errors. The enumeration will complete when the stream
    /// ends or when the cancellation token is cancelled.
    /// </remarks>
    public async IAsyncEnumerable<ParseError> ReadErrorsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var error in _errorChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return error;
        }
    }

    /// <summary>
    /// Reads custom messages parsed by registered parsers as an async enumerable.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the read operation.</param>
    /// <returns>An async enumerable of custom messages (boxed).</returns>
    /// <remarks>
    /// This method can be enumerated concurrently with <see cref="ReadAsync"/> to handle
    /// both built-in and custom messages. The enumeration will complete when the stream
    /// ends or when the cancellation token is cancelled.
    /// </remarks>
    public async IAsyncEnumerable<Object> ReadCustomMessagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var message in _customMessageChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return message;
        }
    }

    /// <summary>
    /// Tries to parse a sentence using a custom parser from the registry.
    /// </summary>
    private Boolean TryParseCustom(ReadOnlySpan<Byte> sentenceBytes, out Object? message)
    {
        message = null;

        if (_registry == null)
        {
            return false;
        }

        // Get the identifier from the sentence
        var lexer = new Lexer(sentenceBytes);
        var identifier = lexer.PeekIdentifier();

        if (_registry.TryGetParser(identifier, out var parser) && parser != null)
        {
            try
            {
                message = parser.Parse(ref lexer);
                return true;
            }
            catch
            {
                // Parse failed, let it fall through to error handling
                return false;
            }
        }

        return false;
    }

    private void EnsureNotStarted()
    {
        lock (_syncRoot)
        {
            if (_started)
            {
                throw new InvalidOperationException("ReadAsync can only be called once per NmeaBatchReader instance.");
            }

            _started = true;
        }
    }

    /// <summary>
    /// Tries to read a complete NMEA sentence from the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read from. Will be sliced past the consumed data.</param>
    /// <param name="sentence">The extracted sentence bytes.</param>
    /// <returns>True if a complete sentence was found; otherwise, false.</returns>
    private static Boolean TryReadSentence(ref ReadOnlySequence<Byte> buffer, out ReadOnlySequence<Byte> sentence)
    {
        sentence = default;

        if (buffer.IsEmpty)
        {
            return false;
        }

        // Find the start of a sentence ($ character)
        var startPosition = FindByte(buffer, Dollar);
        if (startPosition == null)
        {
            // No sentence start found, consume all data
            buffer = buffer.Slice(buffer.End);
            return false;
        }

        // Slice to start at the $ character
        var searchBuffer = buffer.Slice(startPosition.Value);

        // Find the end of the sentence (\r\n or \n)
        var endPosition = FindLineEnd(searchBuffer);
        if (endPosition == null)
        {
            // No complete sentence yet, keep the data from $ onwards
            buffer = buffer.Slice(startPosition.Value);
            return false;
        }

        // Extract the sentence (including $ but excluding \r\n)
        sentence = searchBuffer.Slice(0, endPosition.Value);

        // Advance buffer past the sentence and line ending
        var afterLineEnd = searchBuffer.GetPosition(1, endPosition.Value);

        // Check if there's a \n after \r
        if (searchBuffer.Slice(endPosition.Value).FirstSpan.Length > 0 &&
            searchBuffer.Slice(endPosition.Value).FirstSpan[0] == CarriageReturn)
        {
            var afterCr = searchBuffer.Slice(afterLineEnd);
            if (!afterCr.IsEmpty && afterCr.FirstSpan.Length > 0 && afterCr.FirstSpan[0] == LineFeed)
            {
                afterLineEnd = searchBuffer.GetPosition(2, endPosition.Value);
            }
        }

        buffer = buffer.Slice(buffer.GetPosition(
            searchBuffer.Slice(0, afterLineEnd).Length,
            startPosition.Value));

        return true;
    }

    /// <summary>
    /// Finds the position of a byte in the sequence.
    /// </summary>
    private static SequencePosition? FindByte(ReadOnlySequence<Byte> buffer, Byte target)
    {
        var position = buffer.Start;

        foreach (var segment in buffer)
        {
            var index = segment.Span.IndexOf(target);
            if (index >= 0)
            {
                return buffer.GetPosition(index, position);
            }

            position = buffer.GetPosition(segment.Length, position);
        }

        return null;
    }

    /// <summary>
    /// Finds the position of the line end (\r or \n) in the sequence.
    /// </summary>
    private static SequencePosition? FindLineEnd(ReadOnlySequence<Byte> buffer)
    {
        var position = buffer.Start;

        foreach (var segment in buffer)
        {
            for (var i = 0; i < segment.Length; i++)
            {
                var b = segment.Span[i];
                if (b is CarriageReturn or LineFeed)
                {
                    return buffer.GetPosition(i, position);
                }
            }

            position = buffer.GetPosition(segment.Length, position);
        }

        return null;
    }

    /// <summary>
    /// Gets the bytes from a sequence as a contiguous span.
    /// </summary>
    /// <remarks>
    /// If the sequence is already contiguous (single segment), this returns
    /// the span directly. Otherwise, it copies to a new array.
    /// </remarks>
    private static ReadOnlySpan<Byte> GetContiguousBytes(ReadOnlySequence<Byte> sequence)
    {
        // Fast path: single segment
        if (sequence.IsSingleSegment)
        {
            return sequence.FirstSpan;
        }

        // Slow path: multiple segments, need to copy
        return sequence.ToArray();
    }
}
