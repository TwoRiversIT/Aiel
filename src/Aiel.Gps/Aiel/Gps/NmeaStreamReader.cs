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

using Aiel.Gps.Parsing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;

namespace Aiel.Gps;

/// <summary>
/// Provides high-performance parsing of NMEA 0183 sentences from a stream using System.IO.Pipelines.
/// </summary>
/// <remarks>
/// This class manages the parsing pipeline and coordinates registered message parsers to process NMEA sentences.
/// It uses a pipeline-based architecture for efficient, zero-allocation parsing of GPS data streams.
/// </remarks>
public partial class NmeaStreamReader(ILogger<NmeaStreamReader>? logger = null)
{
    /// <summary>
    /// The line feed byte used to delimit NMEA sentences.
    /// </summary>
    public static readonly Byte ByteLF = (Byte)'\n';
    private readonly ILogger<NmeaStreamReader> _logger = logger ?? new NullLogger<NmeaStreamReader>();
    private readonly List<NmeaMessage> _parsers = [];
    private Int32 _unparsedSequenceLength;
    private ExitReason _exitReason;

    /// <summary>
    /// Gets the total number of lines processed from the stream.
    /// </summary>
    public Int32 LineCount { get; private set; }

    /// <summary>
    /// Gets the total number of bytes received from the stream.
    /// </summary>
    public Int64 BytesReceived { get; private set; }

    /// <summary>
    /// Gets the total number of bytes read and processed.
    /// </summary>
    public Int64 BytesRead { get; private set; }

    /// <summary>
    /// Gets or sets the maximum number of consecutive unparsed lines before parsing is aborted.
    /// </summary>
    /// <remarks>
    /// Set to 0 (default) to disable this check. When enabled, parsing will stop after the specified
    /// number of consecutive lines that cannot be parsed by any registered parser.
    /// </remarks>
    public Int32 AbortAfterUnparsedLines { get; set; } = 0;

    /// <summary>
    /// Gets or sets the behavior when a parse error occurs.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="ParseErrorBehavior.Skip"/> which logs the error and continues parsing.
    /// Set to <see cref="ParseErrorBehavior.ThrowException"/> to halt parsing when an error occurs.
    /// </remarks>
    public ParseErrorBehavior ErrorBehavior { get; set; } = ParseErrorBehavior.Skip;

    /// <summary>
    /// Registers one or more NMEA message parsers with this stream reader.
    /// </summary>
    /// <param name="parsers">The message parsers to register.</param>
    /// <returns>This <see cref="NmeaStreamReader"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parsers"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no parsers are provided.</exception>
    /// <remarks>
    /// Parsers are checked in the order they are registered. Register more common message types first
    /// for optimal performance.
    /// </remarks>
    public NmeaStreamReader Register(params NmeaMessage[] parsers)
    {
        ArgumentNullException.ThrowIfNull(parsers);

        if (parsers.Length == 0)
        {
            throw new ArgumentException("At least one parser must be provided.", nameof(parsers));
        }

        _parsers.AddRange(parsers);

        return this;
    }

    /// <summary>
    /// Asynchronously parses NMEA sentences from a stream and invokes a callback for each parsed message.
    /// </summary>
    /// <param name="stream">The stream containing NMEA data.</param>
    /// <param name="callback">The callback to invoke for each successfully parsed message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the parsing operation.</param>
    /// <returns>An <see cref="ExitReason"/> indicating why parsing stopped.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no parsers have been registered.</exception>
    /// <remarks>
    /// This method uses System.IO.Pipelines for high-performance, low-allocation parsing.
    /// The callback is invoked on the parser thread, so it should execute quickly to avoid blocking.
    /// </remarks>
    public Task<ExitReason> ParseStreamAsync(Stream stream, Action<NmeaMessage> callback, CancellationToken cancellationToken = default)
        => ParseStreamAsync(stream, callback, null, cancellationToken);

    /// <summary>
    /// Asynchronously parses NMEA sentences from a stream and invokes callbacks for parsed messages and errors.
    /// </summary>
    /// <param name="stream">The stream containing NMEA data.</param>
    /// <param name="callback">The callback to invoke for each successfully parsed message.</param>
    /// <param name="errorCallback">The optional callback to invoke when a parse error occurs.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the parsing operation.</param>
    /// <returns>An <see cref="ExitReason"/> indicating why parsing stopped.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no parsers have been registered, or when <see cref="ErrorBehavior"/> is <see cref="ParseErrorBehavior.ThrowException"/> and a parse error occurs.</exception>
    /// <remarks>
    /// This method uses System.IO.Pipelines for high-performance, low-allocation parsing.
    /// The callbacks are invoked on the parser thread, so they should execute quickly to avoid blocking.
    /// If <paramref name="errorCallback"/> is provided, it will be invoked for each parse error regardless of <see cref="ErrorBehavior"/>.
    /// </remarks>
    public async Task<ExitReason> ParseStreamAsync(Stream stream, Action<NmeaMessage> callback, Action<ParseError>? errorCallback, CancellationToken cancellationToken = default)
    {
        if (_parsers.Count == 0)
        {
            throw new InvalidOperationException("At least one parser must be registered in this instance before parsing may begin.");
        }

        var pipe = new Pipe();
        var writing = FillPipeAsync(stream, pipe.Writer, cancellationToken);
        var reading = ReadPipeAsync(pipe.Reader, callback, errorCallback, cancellationToken);

        await Task.WhenAll(reading, writing);

        if (cancellationToken.IsCancellationRequested)
        {
            return ExitReason.CancellationRequested;
        }

        return _exitReason;
    }

    private async Task FillPipeAsync(Stream stream, PipeWriter writer, CancellationToken cancellationToken = default)
    {
        const Int32 minimumBufferSize = 512;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _exitReason = ExitReason.CancellationRequested;
                break;
            }

            try
            {
                // Allocate at least 512 bytes from the PipeWriter. NOTE: NMEA sentences must be less than 80 bytes.
                var memory = writer.GetMemory(minimumBufferSize);

                var bytesRead = await stream.ReadAsync(memory, cancellationToken);
                BytesReceived += bytesRead;
                if (bytesRead == 0)
                {
                    _logger.LogTrace("FillPipeAsync: Stream Ended");
                    _exitReason = ExitReason.StreamEnded;
                    break;
                }

                // Tell the PipeWriter how much was read from the Stream
                writer.Advance(bytesRead);

                // Make the data available to the PipeReader
                var result = await writer.FlushAsync(cancellationToken);
                if (result.IsCompleted)
                {
                    _logger.LogTrace("FillPipeAsync: Reader is Completed");
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                _exitReason = ExitReason.CancellationRequested;
                break;
            }
        }

        // Tell the PipeReader that there's no more data coming
        await writer.CompleteAsync();
    }

    private async Task ReadPipeAsync(PipeReader reader, Action<NmeaMessage> callback, Action<ParseError>? errorCallback, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await reader.ReadAsync(cancellationToken);

                var buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // Look for a EOL in the buffer
                    position = buffer.PositionOf(ByteLF);

                    if (position != null)
                    {
                        // Process the line
                        if (ParseSentence(buffer.Slice(0, position.Value), out var message, out var error))
                        {
                            callback(message);
                        }
                        else if (error != null)
                        {
                            errorCallback?.Invoke(error);
                        }

                        // Skip the line + the \n character (basically position)
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null && !cancellationToken.IsCancellationRequested);

                // Tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming
                if (result.IsCompleted)
                {
                    _logger.LogTrace("ReadPipeAsync: Writer is Completed");
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (AbortAfterUnparsedLines != 0 && _unparsedSequenceLength >= AbortAfterUnparsedLines)
            {
                LogParsingLine(_logger, _unparsedSequenceLength, AbortAfterUnparsedLines);

                _exitReason = ExitReason.TooManySequentialUnparsedLines;
                break;
            }
        }

        // Mark the PipeReader as complete
        await reader.CompleteAsync();
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "ReadPipeAsync: Sequntial Unparsed Lines: {UnparsedLines}  Limit: {Limit}")]
    private static partial void LogParsingLine(ILogger logger, int unparsedLines, int limit);

    private Boolean ParseSentence(ReadOnlySequence<Byte> payload, [NotNullWhen(true)] out NmeaMessage? message, out ParseError? error)
    {
        message = null;
        error = null;

        LineCount++;
        BytesRead += payload.Length;

        foreach (var parser in _parsers)
        {
            if (CanHandle(payload, parser))
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Parsing Line {LineCount} with {ParserName}: {Payload}",
                        LineCount, parser.GetType().Name, payload.ToString(Encoding.UTF8));
                }

                try
                {
                    message = parser.Parse(payload);
                    _unparsedSequenceLength = 0;
                    return true;
                }
                catch (Exception ex)
                {
                    var rawPayload = payload.ToString(Encoding.UTF8);
                    _logger.LogError(ex, "Line {LineCount} caused {ParserName}.Parse({Payload}) to generate the following exception:",
                        LineCount, parser.GetType().Name, rawPayload);

                    error = new ParseError(LineCount, rawPayload, parser.GetType(), ex);

                    if (ErrorBehavior == ParseErrorBehavior.ThrowException)
                    {
                        throw new InvalidOperationException($"Failed to parse line {LineCount} with {parser.GetType().Name}: {rawPayload}", ex);
                    }

                    // Continue to next parser if available
                }
            }
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("No parser found for Line {LineCount}: {Payload}", LineCount, payload.ToString(Encoding.UTF8));
        }

        _unparsedSequenceLength++;
        return false;
    }

    private Boolean CanHandle(ReadOnlySequence<Byte> payload, NmeaMessage parser)
    {
        try
        {
            if (parser.CanHandle(payload))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ParserName}.CanHandle({Payload}) generated the following exception:",
                parser.GetType().Name, payload.ToString(Encoding.UTF8));
        }

        return false;
    }
}
