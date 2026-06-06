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

namespace Aiel.IdGeneration;

public class TimeBasedIdGeneratorTests
{
    [Fact]
    public void NextId_ReturnsNonEmptyString()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);

        var id = generator.NextId();

        Assert.NotEmpty(id);
    }

    [Fact]
    public void NextId_GeneratesUniqueIds()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);
        var ids = new HashSet<String>();

        for (var i = 0; i < 1000; i++)
        {
            var id = generator.NextId();
            Assert.True(ids.Add(id), $"Duplicate ID generated: {id}");
        }
    }

    [Fact]
    public void NextId_GeneratesIncreasingIds()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);
        var previousId = generator.NextId();

        for (var i = 0; i < 100; i++)
        {
            var currentId = generator.NextId();
            Assert.True(String.CompareOrdinal(currentId, previousId) > 0,
                $"IDs not in increasing order: {previousId} >= {currentId}");
            previousId = currentId;
        }
    }

    [Fact]
    public async Task NextId_IsSafeForConcurrentAccess()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);
        var ids = new System.Collections.Concurrent.ConcurrentBag<String>();
        var tasks = new List<Task>();

        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < 100; j++)
                {
                    ids.Add(generator.NextId());
                }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);

        var uniqueIds = ids.Distinct().ToList();
        Assert.Equal(1000, uniqueIds.Count);
    }

    [Fact]
    public void NextId_HandlesRapidCalls()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);
        var ids = new List<String>();

        for (var i = 0; i < 10000; i++)
        {
            ids.Add(generator.NextId());
        }

        var uniqueIds = ids.Distinct().ToList();
        Assert.Equal(10000, uniqueIds.Count);
    }

    [Fact]
    public void Decode_ReturnsReasonableTimestamp()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);
        var beforeGeneration = DateTimeOffset.UtcNow;

        var id = generator.NextId();

        var afterGeneration = DateTimeOffset.UtcNow;
        var decoded = TimeBasedIdGenerator.Decode(id);

        Assert.InRange(decoded, beforeGeneration.AddSeconds(-1), afterGeneration.AddSeconds(1));
    }

    [Fact]
    public void Decode_WithMultipleIds_ReturnsIncreasingTimestamps()
    {
        var generator = new TimeBasedIdGenerator(TimeProvider.System);
        var id1 = generator.NextId();
        Thread.Sleep(10);
        var id2 = generator.NextId();

        var timestamp1 = TimeBasedIdGenerator.Decode(id1);
        var timestamp2 = TimeBasedIdGenerator.Decode(id2);

        Assert.True(timestamp2 >= timestamp1,
            $"Decoded timestamps not in order: {timestamp1} >= {timestamp2}");
    }

    [Fact]
    public void NextId_WithFakeTimeProvider_GeneratesConsistentIds()
    {
        var fakeTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var fakeProvider = new FakeTimeProvider(fakeTime);
        var generator = new TimeBasedIdGenerator(fakeProvider);

        var id1 = generator.NextId();
        var id2 = generator.NextId();

        Assert.NotEqual(id1, id2);
        Assert.True(String.CompareOrdinal(id2, id1) > 0);
    }

    [Fact]
    public void Decode_RoundTrip_PreservesMillisecondPrecision()
    {
        var fakeTime = new DateTimeOffset(2024, 6, 15, 12, 30, 45, 123, TimeSpan.Zero);
        var fakeProvider = new FakeTimeProvider(fakeTime);
        var generator = new TimeBasedIdGenerator(fakeProvider);

        var id = generator.NextId();
        var decoded = TimeBasedIdGenerator.Decode(id);

        Assert.Equal(fakeTime.ToUnixTimeMilliseconds(), decoded.ToUnixTimeMilliseconds());
    }

    private class FakeTimeProvider(DateTimeOffset fixedTime) : TimeProvider
    {
        private readonly DateTimeOffset _fixedTime = fixedTime;

        public override DateTimeOffset GetUtcNow() => _fixedTime;
    }
}
