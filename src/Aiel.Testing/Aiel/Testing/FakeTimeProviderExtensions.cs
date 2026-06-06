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

using Microsoft.Extensions.Time.Testing;

namespace Aiel.Testing;

public static class FakeTimeProviderExtensions
{
    public static void SetDate(this FakeTimeProvider timeProvider, Int32 year, Int32 month, Int32 day)
    {
        var newTime = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(newTime);
    }

    public static void DateIs(this FakeTimeProvider timeProvider, Int32 year, Int32 month, Int32 day)
    {
        var currentTime = timeProvider.GetUtcNow();
        var newTime = new DateTimeOffset(year, month, day, currentTime.Hour, currentTime.Minute, currentTime.Second, currentTime.Offset);
        timeProvider.SetUtcNow(newTime);
    }

    public static void DateTimeIs(this FakeTimeProvider timeProvider, Int32 year, Int32 month, Int32 day, Int32 hour, Int32 minute, Int32 second)
    {
        var newTime = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero);
        timeProvider.SetUtcNow(newTime);
    }

    public static void TimeIs(this FakeTimeProvider timeProvider, Int32 hour, Int32 minute, Int32 second)
    {
        var currentTime = timeProvider.GetUtcNow();
        var newTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, hour, minute, second, currentTime.Offset);
        timeProvider.SetUtcNow(newTime);
    }
}
