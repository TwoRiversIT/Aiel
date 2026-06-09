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

using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace Aiel.Gps;

/// <summary>
/// Provides an asynchronous reader for NMEA messages from a stream.
/// </summary>
/// <remarks>
/// This class reads NMEA sentences from a GPS data stream and parses them into strongly-typed message objects.
/// The reader uses a background task to continuously parse the stream and makes messages available through
/// an async enumerable interface or individual read operations.
/// </remarks>
public sealed class NmeaReader : DisposableBase
{
    private readonly BufferBlock<NmeaMessage> _queue;
    private readonly BufferBlock<ParseError> _errorQueue;
    private readonly NmeaStreamReader _nmeaStreamReader;
    private readonly Stream _stream;
    private readonly ILogger<NmeaReader>? _logger;
    private readonly CancellationTokenSource _disposalCts = new();

#if NET9_0_OR_GREATER
    private readonly Lock _syncroot = new();
#else
    private readonly Object _syncroot = new();
#endif

    private Task? _task;
    private CancellationToken _readerCancellationToken;
    private Exception? _parseException;

    /// <summary>
    /// Initializes a new instance of the <see cref="NmeaReader"/> class.
    /// </summary>
    /// <param name="nmeaStreamReader">The NMEA stream reader with registered message parsers.</param>
    /// <param name="stream">The stream containing NMEA data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nmeaStreamReader"/> or <paramref name="stream"/> is null.</exception>
    public NmeaReader(NmeaStreamReader nmeaStreamReader, Stream stream, ILogger<NmeaReader>? logger = null)
    {
        _nmeaStreamReader = nmeaStreamReader ?? throw new ArgumentNullException(nameof(nmeaStreamReader));
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _logger = logger;
        _queue = new BufferBlock<NmeaMessage>(new DataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });
        _errorQueue = new BufferBlock<ParseError>(new DataflowBlockOptions { BoundedCapacity = DataflowBlockOptions.Unbounded });
    }

    private void InitializeStreamReader(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (_task == null)
        {
            lock (_syncroot)
            {
                if (_task == null)
                {
                    _readerCancellationToken = cancellationToken;

                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposalCts.Token);

                    _task = Task.Run(async () =>
                    {
                        try
                        {
                            await _nmeaStreamReader.ParseStreamAsync(
                                _stream,
                                (m) => _queue.Post(m),
                                (e) => _errorQueue.Post(e),
                                linkedCts.Token);
                        }
                        catch (OperationCanceledException) when (linkedCts.Token.IsCancellationRequested)
                        {
                            // Expected when cancelled
                        }
                        catch (Exception ex)
                        {
                            _parseException = ex;
                            throw;
                        }
                        finally
                        {
                            _queue.Complete();
                            _errorQueue.Complete();
                            linkedCts.Dispose();
                        }
                    }, linkedCts.Token);
                }
            }
        }
        else if (!_readerCancellationToken.Equals(cancellationToken))
        {
            throw new InvalidOperationException("Cannot use different CancellationToken instances across multiple read operations on the same NmeaReader instance.");
        }
    }

    /// <summary>
    /// Reads NMEA messages from the stream as an asynchronous enumerable sequence.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the read operation.</param>
    /// <returns>An asynchronous enumerable of <see cref="NmeaMessage"/> objects.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called with different cancellation tokens on subsequent calls.</exception>
    /// <exception cref="Exception">Thrown when an error occurs during parsing.</exception>
    /// <remarks>
    /// This method can be used with async foreach to process messages as they are parsed from the stream.
    /// The enumeration will complete when the stream ends or when the cancellation token is cancelled.
    /// Parse errors are available via <see cref="ReadErrorsAsync"/> and do not interrupt message enumeration.
    /// </remarks>
    public async IAsyncEnumerable<NmeaMessage> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        InitializeStreamReader(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            NmeaMessage? message;

            try
            {
                if (await _queue.OutputAvailableAsync(cancellationToken))
                {
                    message = await _queue.ReceiveAsync(cancellationToken);
                }
                else
                {
                    break;
                }
            }
            catch (InvalidOperationException)
            {
                break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (message != null)
            {
                yield return message;
            }

            if (_parseException != null)
            {
                throw new InvalidOperationException("An error occurred while parsing the NMEA stream.", _parseException);
            }
        }
    }

    /// <summary>
    /// Reads parse errors from the stream as an asynchronous enumerable sequence.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the read operation.</param>
    /// <returns>An asynchronous enumerable of <see cref="ParseError"/> objects.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called with different cancellation tokens on subsequent calls.</exception>
    /// <remarks>
    /// This method provides access to sentences that could not be parsed, similar to a dead-letter queue.
    /// It can be enumerated concurrently with <see cref="ReadAsync"/> to handle both successful messages and errors.
    /// The enumeration will complete when the stream ends or when the cancellation token is cancelled.
    /// </remarks>
    public async IAsyncEnumerable<ParseError> ReadErrorsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        InitializeStreamReader(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            ParseError? error;

            try
            {
                if (await _errorQueue.OutputAvailableAsync(cancellationToken))
                {
                    error = await _errorQueue.ReceiveAsync(cancellationToken);
                }
                else
                {
                    break;
                }
            }
            catch (InvalidOperationException)
            {
                break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (error != null)
            {
                yield return error;
            }

            if (_parseException != null)
            {
                throw new InvalidOperationException("An error occurred while parsing the NMEA stream.", _parseException);
            }
        }
    }

    /// <summary>
    /// Reads the next NMEA message from the stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the read operation.</param>
    /// <returns>The next <see cref="NmeaMessage"/>, or null if the stream has ended.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called with different cancellation tokens on subsequent calls or when an error occurs during parsing.</exception>
    /// <remarks>
    /// This method is useful when you need to read messages one at a time with manual control over the reading process.
    /// Consider using <see cref="ReadAsync"/> with async foreach for most scenarios.
    /// </remarks>
    public async ValueTask<NmeaMessage?> ReadNextAsync(CancellationToken cancellationToken = default)
    {
        InitializeStreamReader(cancellationToken);

        try
        {
            if (await _queue.OutputAvailableAsync(cancellationToken))
            {
                var message = await _queue.ReceiveAsync(cancellationToken);

                if (_parseException != null)
                {
                    throw new InvalidOperationException("An error occurred while parsing the NMEA stream.", _parseException);
                }

                return message;
            }
        }
        catch (InvalidOperationException)
        {
            // Queue is completed
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when cancelled
        }

        if (_parseException != null)
        {
            throw new InvalidOperationException("An error occurred while parsing the NMEA stream.", _parseException);
        }

        return null;
    }

    /// <summary>
    /// Asynchronously releases managed resources used by the <see cref="NmeaReader"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        await _disposalCts.CancelAsync();

        if (_task != null)
        {
            try
            {
                await _task;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred during disposal.");
            }

            _task.Dispose();
            _task = null;
        }

        _disposalCts.Dispose();
    }

    /// <summary>
    /// Releases managed resources used by the <see cref="NmeaReader"/>.
    /// </summary>
    /// <param name="disposing">True to release managed resources; false to release only unmanaged resources.</param>
    protected override void Dispose(Boolean disposing)
    {
        if (disposing && !IsDisposed)
        {
            _disposalCts.Cancel();

            if (_task != null)
            {
                // For synchronous disposal, we complete the task but do not block waiting for it
                // This is safer than GetAwaiter().GetResult() which can deadlock
                _task = null;
            }

            _disposalCts.Dispose();
        }

        base.Dispose(disposing);
    }
}
