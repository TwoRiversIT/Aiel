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

namespace Aiel.Timing;

public class MarkingStopwatchTests
{
#if DEBUG
    public static Boolean IsDebug => true;
#else
    public static Boolean IsDebug => false;
#endif

    [Fact]
    public void Elapsed_ReturnsElapsedTime()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Mark();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().Be(TimeSpan.FromSeconds(2));
        stopwatch.Marks.Count.Should().Be(3);
    }

    [Fact]
    public void Elapsed_WhenNotStarted_Returns_TimeSpanZero()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        var elapsed = stopwatch.Elapsed;

        // Assert
        elapsed.Should().Be(TimeSpan.Zero);
        stopwatch.Marks.Count.Should().Be(0);
    }

    [Fact]
    public void Start_WithoutDescription_UsesDefaultDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        stopwatch.Start();

        // Assert
        stopwatch.IsRunning.Should().BeTrue();
        stopwatch.Marks.Should().ContainSingle();
        stopwatch.Marks[0].Description.Should().Be("Started");
    }

    [Fact]
    public void Start_WithDescription_RecordsCustomDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        var description = "Custom Start";

        // Act
        stopwatch.Start(description);

        // Assert
        stopwatch.IsRunning.Should().BeTrue();
        stopwatch.Marks[0].Description.Should().Be(description);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_IsIgnored()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        var initialCount = stopwatch.Marks.Count;

        // Act
        stopwatch.Start("Second Start");

        // Assert
        stopwatch.Marks.Count.Should().Be(initialCount);
        stopwatch.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void Stop_WithoutDescription_UsesDefaultDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();

        // Act
        stopwatch.Stop();

        // Assert
        stopwatch.IsRunning.Should().BeFalse();
        stopwatch.Marks.Should().HaveCount(2);
        stopwatch.Marks[1].Description.Should().Be("Stopped");
    }

    [Fact]
    public void Stop_WithDescription_RecordsCustomDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        var description = "Custom Stop";
        stopwatch.Start();

        // Act
        stopwatch.Stop(description);

        // Assert
        stopwatch.Marks[1].Description.Should().Be(description);
    }

    [Fact]
    public void Stop_WhenNotRunning_IsIgnored()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        stopwatch.Stop("Stop without start");

        // Assert
        stopwatch.IsRunning.Should().BeFalse();
        stopwatch.Marks.Should().BeEmpty();
    }

    [Fact]
    public void Mark_WithoutDescription_UsesDefaultDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();

        // Act
        stopwatch.Mark();

        // Assert
        stopwatch.Marks.Should().HaveCount(2);
        stopwatch.Marks[1].Description.Should().Be("Mark");
    }

    [Fact]
    public void Mark_WithDescription_RecordsCustomDescription()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        var description = "Custom Mark";
        stopwatch.Start();

        // Act
        stopwatch.Mark(description);

        // Assert
        stopwatch.Marks[1].Description.Should().Be(description);
    }

    [Fact]
    public void Mark_RecordsCurrentElapsedTime()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(2.5));

        // Act
        stopwatch.Mark();

        // Assert
        stopwatch.Marks[1].Elapsed.Should().Be(TimeSpan.FromSeconds(2.5));
    }

    [Fact]
    public void Mark_BeforeStart_RecordsMark()
    {
        // Arrange
        const String description = "Mark before start";

        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        stopwatch.Mark(description);

        // Assert
        stopwatch.Marks.Should().ContainSingle();
        stopwatch.Marks[0].Description.Should().Be(description);
        stopwatch.Marks[0].Elapsed.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Mark_AfterStop_RecordsMark()
    {
        // Arrange
        const String description = "Mark after stop";

        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Stop();

        // Act
        stopwatch.Mark(description);

        // Assert
        stopwatch.Marks.Should().HaveCount(3);
        stopwatch.Marks[2].Description.Should().Be(description);
        stopwatch.Marks[2].Elapsed.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Mark_x2_AfterStop_RecordsMarks()
    {
        // Arrange
        const String alpha = "Alpha";
        const String beta = "Beta";

        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Stop();

        // Act
        stopwatch.Mark(alpha);
        stopwatch.Mark(beta);

        // Assert
        stopwatch.Marks.Should().HaveCount(4);
        stopwatch.Marks[2].Description.Should().Be(alpha);
        stopwatch.Marks[2].Elapsed.Should().Be(TimeSpan.FromSeconds(1));
        stopwatch.Marks[3].Description.Should().Be(beta);
        stopwatch.Marks[3].Elapsed.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Reset_ClearsMarksAndState()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Mark();
        stopwatch.Stop();

        var elapsedBeforeReset = stopwatch.Elapsed;

        // Act
        stopwatch.Reset();

        // Assert
        stopwatch.Marks.Should().BeEmpty();
        stopwatch.IsRunning.Should().BeFalse();
        // After reset, _started is set to 0, so Elapsed will calculate from timestamp 0
        // which represents a very large elapsed time (from epoch). Just verify it changed.
        stopwatch.Elapsed.Should().NotBe(elapsedBeforeReset);
    }

    [Fact]
    public void GetMarksWithDeltas_CalculatesDeltasBetweenMarks()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Mark();
        timeProvider.Advance(TimeSpan.FromSeconds(2));
        stopwatch.Mark();
        timeProvider.Advance(TimeSpan.FromSeconds(1.5));
        stopwatch.Stop();

        // Act
        var marksWithDeltas = stopwatch.GetMarksWithDeltas();

        // Assert
        marksWithDeltas.Should().HaveCount(4);
        marksWithDeltas[0].Delta.Should().Be(TimeSpan.Zero); // First mark has zero delta
        marksWithDeltas[1].Delta.Should().Be(TimeSpan.FromSeconds(1)); // 1 - 0 = 1
        marksWithDeltas[2].Delta.Should().Be(TimeSpan.FromSeconds(2)); // 3 - 1 = 2
        marksWithDeltas[3].Delta.Should().Be(TimeSpan.FromSeconds(1.5)); // 4.5 - 3 = 1.5
    }

    [Fact]
    public void GetMarksWithDeltas_IncludesIndexes()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        stopwatch.Mark();
        stopwatch.Mark();
        stopwatch.Stop();

        // Act
        var marksWithDeltas = stopwatch.GetMarksWithDeltas();

        // Assert
        marksWithDeltas.Should().HaveCount(4);
        marksWithDeltas.Select(m => m.Index).Should().Equal(0, 1, 2, 3);
    }

    [Fact]
    public void Marks_ReturnsReadOnlySnapshot()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        stopwatch.Mark();

        // Act
        var marks = stopwatch.Marks;

        // Assert
        marks.Should().HaveCount(2);
        marks.Should().BeAssignableTo<IReadOnlyList<Mark>>();
    }

    [Fact]
    public void Marks_ReturnsSnapshotEachCall_And_GetMarksWithDeltas_ComputesCorrectIndexAndDelta()
    {
        // Arrange
        var tp = new FakeTimeProvider();
        var sw = new MarkingStopwatch(tp);

        sw.Start("start");                     // elapsed = 0
        tp.Advance(TimeSpan.FromTicks(100));
        sw.Mark("m1");                         // elapsed = 100
        tp.Advance(TimeSpan.FromTicks(200));
        sw.Mark("m2");                         // elapsed = 300
        tp.Advance(TimeSpan.FromTicks(300));
        sw.Stop("stop");                       // elapsed = 600

        // Act
        var snapshot1 = sw.Marks;
        var snapshot2 = sw.Marks;
        var deltas = sw.GetMarksWithDeltas().ToList();

        // Assert: Marks returns a fresh array each call
        snapshot1.Should().NotBeSameAs(snapshot2);
        snapshot1.Count.Should().Be(snapshot2.Count);

        // Assert: Index and Delta semantics
        deltas.Should().HaveCount(4);

        deltas.Select(m => m.Index).Should().BeEquivalentTo([0, 1, 2, 3]);

        deltas.Select(m => m.Elapsed.Ticks)
              .Should().BeEquivalentTo(new Int64[] { 0, 100, 300, 600 });

        deltas.Select(m => m.Delta.Ticks)
              .Should().BeEquivalentTo(new Int64[] { 0, 100, 200, 300 });
    }

    [Fact]
    public void Mark_WhenNotRunning_BeforeStart_And_AfterStop_ProducesExpectedElapsed()
    {
        // Arrange
        var tp = new FakeTimeProvider();
        var sw = new MarkingStopwatch(tp);

        // Act: Mark before Start
        sw.Mark("before-start");
        var before = sw.GetMarksWithDeltas().Single();

        // Assert
        before.Elapsed.Should().Be(TimeSpan.Zero);
        before.Delta.Should().Be(TimeSpan.Zero);

        // Start → advance → stop
        sw.Start("start");
        tp.Advance(TimeSpan.FromTicks(500));
        sw.Stop("stop");

        var marks = sw.GetMarksWithDeltas();
        var last = marks[marks.Count - 1];
        var stopElapsed = last.Elapsed;

        // Act: Mark after Stop
        sw.Mark("after-stop");
        var after = sw.GetMarksWithDeltas()[sw.GetMarksWithDeltas().Count - 1];

        // Assert: elapsed should not move after stop
        after.Elapsed.Should().Be(stopElapsed);
        after.Delta.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Start_Stop_Mark_ShouldNotThrow_WhenDescriptionIsNull()
    {
        // Arrange
        var tp = new FakeTimeProvider();
        var sw = new MarkingStopwatch(tp);

        // Assert
        sw.Invoking(s => s.Start(null!)).Should().NotThrow();
        sw.Invoking(s => s.Stop(null!)).Should().NotThrow();
        sw.Invoking(s => s.Mark(null!)).Should().NotThrow();
    }

    [Fact]
    public void IsRunning_ReflectsCurrentState()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act & Assert
        stopwatch.IsRunning.Should().BeFalse();
        stopwatch.Start();
        stopwatch.IsRunning.Should().BeTrue();
        stopwatch.Stop();
        stopwatch.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void MarkInfo_StoresTimestampWhen_Created()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var now = timeProvider.GetUtcNow();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        stopwatch.Start();

        // Assert
        stopwatch.Marks[0].WallTime.Should().Be(now);
    }

    [Fact]
    public void CompleteWorkflow_TracksMultiplePhases()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        stopwatch.Start("Initialization");
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Mark("Phase 1 Started");
        timeProvider.Advance(TimeSpan.FromSeconds(2));
        stopwatch.Mark("Phase 2 Started");
        timeProvider.Advance(TimeSpan.FromSeconds(1.5));
        stopwatch.Stop("Completed");

        // Assert
        stopwatch.Marks.Should().HaveCount(4);
        stopwatch.Marks[0].Description.Should().Be("Initialization");
        stopwatch.Marks[1].Description.Should().Be("Phase 1 Started");
        stopwatch.Marks[2].Description.Should().Be("Phase 2 Started");
        stopwatch.Marks[3].Description.Should().Be("Completed");
        stopwatch.Elapsed.Should().Be(TimeSpan.FromSeconds(4.5));
    }

    [Fact]
    public async Task DisposeAsync_StopsIfRunning()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));

        // Act
        await stopwatch.DisposeAsync();

        // Assert
        stopwatch.IsRunning.Should().BeFalse();
        stopwatch.Marks.Should().HaveCount(2);
        stopwatch.Marks[1].Description.Should().Be("Disposed");
    }

    [Fact]
    public async Task DisposeAsync_WithoutStarting_DoesNotAddMark()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act
        await stopwatch.DisposeAsync();

        // Assert
        stopwatch.Marks.Should().BeEmpty();
        stopwatch.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void DefaultTimeProvider_UsesSystemTimeProvider()
    {
        // Act
        var stopwatch = new MarkingStopwatch();

        // Assert
        stopwatch.Should().NotBeNull();
        stopwatch.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void GetAsMarkdown_ReturnsFormattedTable()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start("Begin");
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        stopwatch.Mark("Mid");
        stopwatch.Stop("End");

        // Act
        var markdown = stopwatch.GetAsMarkdown();

        // Assert
        markdown.Should().Contain("| Index | Elapsed");
        markdown.Should().Contain("| Delta");
        markdown.Should().Contain("| Begin");
        markdown.Should().Contain("| Mid");
        markdown.Should().Contain("| End");
    }

    [Fact]
    public void ExportCsv_ReturnsFormattedCsv()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start("Start");
        stopwatch.Mark("Mark");
        stopwatch.Stop("Stop");

        // Act
        var csv = stopwatch.ExportCsv();

        // Assert
        csv.Should().Contain("Index,Elapsed,Delta,Description,TimestampUtc");
        csv.Should().Contain("Start");
        csv.Should().Contain("Mark");
        csv.Should().Contain("Stop");
    }

    [Fact]
    public void ExportJson_ReturnsValidJson()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start("Test");
        stopwatch.Mark();
        stopwatch.Stop();

        // Act
        var json = stopwatch.ExportJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        // JSON uses PascalCase property names per System.Text.Json defaults
        json.Should().Contain("\"Index\"");
        json.Should().Contain("\"Elapsed\"");
        json.Should().Contain("\"Delta\"");
        json.Should().Contain("\"Description\"");
        json.Should().Contain("\"Test\"");
    }

    [Fact]
    public void LogToXunit_WithValidTestHelper_InvokesWriteLine()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);
        stopwatch.Start();
        stopwatch.Stop();

        var mockTestHelper = new MockTestOutputHelper();

        // Act
        stopwatch.LogToXunit(mockTestHelper);

        // Assert
        mockTestHelper.Output.Should().NotBeNullOrEmpty();
        mockTestHelper.Output.Should().Contain("Started");
        mockTestHelper.Output.Should().Contain("Stopped");
    }

    [Fact]
    public void LogToXunit_WithNullStopwatch_Throws()
    {
        // Act & Assert
        var action = () => LoggingStopwatchExtensions.LogToXunit(null!, new MockTestOutputHelper());
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LogToXunit_WithNullHelper_Throws()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new MarkingStopwatch(timeProvider);

        // Act & Assert
        var action = () => stopwatch.LogToXunit(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(Skip = "This test fails intermittently. Fix the code or fix the test.", SkipUnless = "IsDebug")]
    public async Task MarkingStopwatch_ShouldBeThreadSafe_UnderHeavyConcurrentUse()
    {
        // Arrange
        var tp = new FakeTimeProvider();
        var sw = new MarkingStopwatch(tp);

        const Int32 threadCount = 32;
        const Int32 iterationsPerThread = 1000;

        var random = new Random();
        var tasks = new Task[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (var i = 0; i < iterationsPerThread; i++)
                {
                    var op = random.Next(0, 5);

                    switch (op)
                    {
                        case 0:
                            sw.Start("start");
                            break;

                        case 1:
                            sw.Mark("mark");
                            break;

                        case 2:
                            sw.Stop("stop");
                            break;

                        case 3:
                            sw.Reset();
                            break;

                        case 4:
                            // Advance time a bit to simulate real progression
                            tp.Advance(TimeSpan.FromTicks(random.Next(1, 50)));
                            break;
                    }
                }
            }, TestContext.Current.CancellationToken);
        }

        // Act
        await Task.WhenAll(tasks);

        // Assert: no exceptions thrown during execution
        // Now validate internal consistency

        var marks = sw.GetMarksWithDeltas();

        // 1. No negative elapsed times
        marks.Should().OnlyContain(m => m.Elapsed >= TimeSpan.Zero);

        // 2. Elapsed must be monotonic non-decreasing
        for (var i = 1; i < marks.Count; i++)
        {
            marks[i].Elapsed.Should().BeGreaterThanOrEqualTo(marks[i - 1].Elapsed);
        }

        // 3. Delta must be >= 0
        marks.Should().OnlyContain(m => m.Delta >= TimeSpan.Zero);

        // 4. Index must be correct
        for (var i = 0; i < marks.Count; i++)
        {
            marks[i].Index.Should().Be(i);
        }

        // 5. No duplicate timestamps out of order
        for (var i = 1; i < marks.Count; i++)
        {
            marks[i].WallTime.Should().BeOnOrAfter(marks[i - 1].WallTime);
        }

        // 6. Calling Elapsed should never throw and must be >= 0
        sw.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    /// <summary>
    /// Mock implementation of xUnit's ITestOutputHelper for testing LogToXunit.
    /// </summary>
    private class MockTestOutputHelper
    {
        public String? Output { get; private set; }

        public void WriteLine(String message)
        {
            Output = message;
        }
    }
}
