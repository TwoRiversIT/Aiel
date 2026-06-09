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

using Aiel.IdGeneration.Internal;

namespace Aiel.IdGeneration;

/// <summary>
/// Generates unique IDs based on values from <see cref="IClock.GetCurrentInstant"/>. This class should be registered as a Singleton.
/// </summary>
/// <remarks>
/// <para>Generated IDs are only unique within the instance.</para>
/// <para>If you want to decode the generated ID into an <see cref="Instant"/> then use <see cref="Decode(String)"/>.</para>
/// </remarks>
public class TimeBasedIdGenerator(TimeProvider clock) : IIdGenerator
{
#if NET9_0_OR_GREATER
    private readonly Lock _syncRoot = new();
#else
    private readonly Object _syncRoot = new();
#endif

    private readonly TimeProvider _clock = clock;
    private Int64 _lastTickCount;

    /// <summary>
    /// Generates unique IDs based on values from <see cref="IClock.GetCurrentInstant"/>.
    /// </summary>
    /// <returns>a <see cref="String"/> containing a unique ID</returns>.
    /// <remarks>
    /// <para>Generated IDs are only unique within the instance.</para>
    /// <para>If you want to decode the generated ID into an <see cref="Instant"/> then use <see cref="Decode(String)"/>.</para>
    /// </remarks>
    public String NextId()
    {
        lock (_syncRoot)
        {
            var currentTicksCount = _clock.GetUtcNow().ToUnixTimeMilliseconds();
            if (_lastTickCount < currentTicksCount)
            {
                Interlocked.Exchange(ref _lastTickCount, currentTicksCount);
            }
            else if (_lastTickCount >= currentTicksCount)
            {
                Interlocked.Exchange(ref _lastTickCount, _lastTickCount + 1);
            }

            return Base36.Encode(_lastTickCount);
        }
    }

    public static DateTimeOffset Decode(String timeBasedId)
        => DateTimeOffset.FromUnixTimeMilliseconds(Base36.Decode(timeBasedId));
}
