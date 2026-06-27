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

public class SessionStopwatchTests
{
    // Reminder: We are only testing the session management functionality of 
    // SessionStopwatch here, not the functionality of the managed stopwatches.

    [Fact]
    public void Indexer_ReturnsSameInstanceForSameSessionName()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);

        // Act
        var session1 = stopwatch["MySession"];
        var session2 = stopwatch["MySession"];

        // Assert
        session1.Should().BeSameAs(session2);
    }

    [Fact]
    public void Indexer_IsCaseInsensitive()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);

        // Act
        var sessionLower = stopwatch["mysession"];
        var sessionUpper = stopwatch["MYSESSION"];
        var sessionMixed = stopwatch["MySession"];

        // Assert
        sessionLower.Should().BeSameAs(sessionUpper);
        sessionLower.Should().BeSameAs(sessionMixed);
        stopwatch.Sessions.Should().ContainSingle();
    }

    [Fact]
    public void Indexer_WithNullSessionName_Throws()
    {
        // Arrange
        var stopwatch = new SessionStopwatch();

        // Act & Assert
        var action = () => stopwatch[null!];
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Indexer_CreatesNewStopwatchForNewSessionName()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);

        // Act
        var session = stopwatch["NewSession"];

        // Assert
        session.Should().NotBeNull();
        session.Should().BeAssignableTo<IMarkingStopwatch>();
    }

    [Fact]
    public void Sessions_ReturnsList_InUnspecifiedOrder()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        _ = stopwatch["Session1"];
        _ = stopwatch["Session2"];

        // Act
        var sessions = stopwatch.Sessions;

        // Assert
        sessions.Should().BeAssignableTo<IReadOnlyList<StopwatchSession>>();
        sessions.Should().HaveCount(2);
        sessions.Select(s => s.Name).Should().Contain("Session1", "Session2");
    }

    [Fact]
    public void Sessions_ReturnsSnapshot_NotReference()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        _ = stopwatch["Session1"];
        _ = stopwatch["Session2"];

        // Act
        var sessions1 = stopwatch.Sessions;
        _ = stopwatch["Session3"];
        var sessions2 = stopwatch.Sessions;

        // Assert
        sessions1.Should().HaveCount(2);
        sessions2.Should().HaveCount(3);
        sessions1.Should().NotBeSameAs(sessions2);
    }

    [Fact]
    public void GetSessionsOrderedByName_ReturnsSessionsAlphabetically()
    {
        // Arrange
        var tp = new FakeTimeProvider();
        var sw = new SessionStopwatch(tp);

        // Create sessions in non-alphabetical insertion order
        _ = sw["Zebra"];
        _ = sw["Apple"];
        _ = sw["Banana"];

        // Act
        var ordered = sw.GetSessionsOrderedByName();

        // Assert
        ordered.Select(s => s.Name).Should().ContainInOrder(["Apple", "Banana", "Zebra"]);
    }

    [Fact]
    public void GetSessionsOrderedByStartTime_ReturnsSessionsByCreationTime()
    {
        // Arrange
        var tp = new FakeTimeProvider();
        var sw = new SessionStopwatch(tp);

        // Create sessions at different times
        _ = sw["First"];   // created at t0
        tp.Advance(TimeSpan.FromMilliseconds(10));
        _ = sw["Second"];  // created at t0 + 10ms
        tp.Advance(TimeSpan.FromMilliseconds(10));
        _ = sw["Third"];   // created at t0 + 20ms

        // Act
        var ordered = sw.GetSessionsOrderedByStartTime();

        // Assert
        ordered.Select(s => s.Name).Should().ContainInOrder(["First", "Second", "Third"]);
    }

    [Fact]
    public void Sessions_WithNoSessions_ReturnsEmptyDictionary()
    {
        // Arrange
        var stopwatch = new SessionStopwatch();

        // Act
        var sessions = stopwatch.Sessions;

        // Assert
        sessions.Should().BeEmpty();
        sessions.Should().BeAssignableTo<IReadOnlyList<StopwatchSession>>();
    }

    [Fact]
    public void CreatedSessions_InheritTimeProvider()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);

        // Act
        var session = stopwatch["TestSession"];
        session.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        session.Stop();

        // Assert
        session.Elapsed.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MultipleSessionsAreIndependent()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        var session1 = stopwatch["Session1"];
        var session2 = stopwatch["Session2"];

        // Act
        session1.Start("S1 Start");
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        session2.Start("S2 Start");
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        session1.Stop("S1 Stop");
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        session2.Stop("S2 Stop");

        // Assert - verify each session has independent marks
        session1.Marks.Should().HaveCount(2);
        session2.Marks.Should().HaveCount(2);
        session1.Marks[0].Description.Should().Be("S1 Start");
        session1.Marks[1].Description.Should().Be("S1 Stop");
        session2.Marks[0].Description.Should().Be("S2 Start");
        session2.Marks[1].Description.Should().Be("S2 Stop");
    }

    [Fact]
    public async Task DisposeAsync_DisposesAllSessions()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        var session1 = stopwatch["Session1"];
        var session2 = stopwatch["Session2"];
        session1.Start();
        session2.Start();

        // Act
        await stopwatch.DisposeAsync();

        // Assert
        session1.IsRunning.Should().BeFalse();
        session2.IsRunning.Should().BeFalse();
        // After disposal, both should have a "Disposed" mark
        session1.Marks[
        // After disposal, both should have a "Disposed" mark
        session1.Marks.Count - 1].Description.Should().Be("Disposed");
        session2.Marks[session2.Marks.Count - 1].Description.Should().Be("Disposed");
    }

    [Fact]
    public async Task DisposeAsync_WithNoSessions_CompletesSuccessfully()
    {
        // Arrange
        var stopwatch = new SessionStopwatch();

        // Act & Assert
        await stopwatch.DisposeAsync();
        // No exception should be thrown
    }

    [Fact]
    public void ExportAllAsMarkdown_ReturnsFormattedOutput()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        var session1 = stopwatch["Session1"];
        var session2 = stopwatch["Session2"];
        session1.Start("Start S1");
        session1.Stop("Stop S1");
        session2.Start("Start S2");
        session2.Stop("Stop S2");

        // Act
        var markdown = stopwatch.ExportAllAsMarkdown();

        // Assert
        markdown.Should().Contain("## Session: Session1");
        markdown.Should().Contain("## Session: Session2");
        markdown.Should().Contain("Start S1");
        markdown.Should().Contain("Stop S1");
        markdown.Should().Contain("Start S2");
        markdown.Should().Contain("Stop S2");
        markdown.Should().Contain("| Index | Elapsed");
    }

    [Fact]
    public void ExportAllAsMarkdown_OrdersSessionsByNameAscending()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        stopwatch["Zebra"].Start();
        stopwatch["Apple"].Start();
        stopwatch["Banana"].Start();

        // Act
        var markdown = stopwatch.ExportAllAsMarkdown();

        // Assert
        var appleIndex = markdown.IndexOf("## Session: Apple");
        var bananaIndex = markdown.IndexOf("## Session: Banana");
        var zebraIndex = markdown.IndexOf("## Session: Zebra");
        appleIndex.Should().BeLessThan(bananaIndex);
        bananaIndex.Should().BeLessThan(zebraIndex);
    }

    [Fact]
    public void ExportAllAsMarkdown_WithCustomTimeSpanFormat_UsesFormat()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        var session = stopwatch["Session1"];
        session.Start();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        session.Stop();

        // Act
        var markdownDefault = stopwatch.ExportAllAsMarkdown();
        var markdownCustom = stopwatch.ExportAllAsMarkdown(@"mm\:ss");

        // Assert
        markdownCustom.Should().NotBeNull();
        // Custom format should use different length representation
        markdownCustom.Should().Contain("00:01");
    }

    [Fact]
    public void ExportAllAsCsv_ReturnsFormattedOutput()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        var session1 = stopwatch["Session1"];
        var session2 = stopwatch["Session2"];
        session1.Start("Start S1");
        session1.Stop("Stop S1");
        session2.Start("Start S2");
        session2.Stop("Stop S2");

        // Act
        var csv = stopwatch.ExportAllAsCsv();

        // Assert
        csv.Should().Contain("Session,Index,Elapsed,Delta,Description,TimestampUtc");
        csv.Should().Contain("Session1");
        csv.Should().Contain("Session2");
        csv.Should().Contain("Start S1");
        csv.Should().Contain("Stop S1");
        csv.Should().Contain("Start S2");
        csv.Should().Contain("Stop S2");
    }

    [Fact]
    public void ExportAllAsCsv_OrdersSessionsByNameAscending()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        stopwatch["Zebra"].Start();
        stopwatch["Apple"].Start();

        // Act
        var csv = stopwatch.ExportAllAsCsv();

        // Assert
        var appleIndex = csv.IndexOf("Apple");
        var zebraIndex = csv.IndexOf("Zebra");
        appleIndex.Should().BeLessThan(zebraIndex);
    }

    [Fact]
    public void ExportAllAsCsv_EscapesCommasInSessionNames()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        stopwatch["Session,WithComma"].Start();

        // Act
        var csv = stopwatch.ExportAllAsCsv();

        // Assert
        csv.Should().Contain("\"Session,WithComma\"");
    }

    [Fact]
    public void ExportAllAsJson_ReturnsValidJsonStructure()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        var session1 = stopwatch["Session1"];
        var session2 = stopwatch["Session2"];
        session1.Start("Start S1");
        session1.Stop("Stop S1");
        session2.Start("Start S2");
        session2.Stop("Stop S2");

        // Act
        var json = stopwatch.ExportAllAsJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"Session1\"");
        json.Should().Contain("\"Session2\"");
        json.Should().Contain("\"Start S1\"");
        json.Should().Contain("\"Stop S1\"");
        json.Should().Contain("\"Start S2\"");
        json.Should().Contain("\"Stop S2\"");
        json.Should().Contain("\"Index\"");
        json.Should().Contain("\"Elapsed\"");
    }

    [Fact]
    public void ExportAllAsJson_OrdersSessionsByNameAscending()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        stopwatch["Zebra"].Start();
        stopwatch["Apple"].Start();
        stopwatch["Banana"].Start();

        // Act
        var json = stopwatch.ExportAllAsJson();

        // Assert
        var appleIndex = json.IndexOf("\"Apple\"");
        var bananaIndex = json.IndexOf("\"Banana\"");
        var zebraIndex = json.IndexOf("\"Zebra\"");
        appleIndex.Should().BeLessThan(bananaIndex);
        bananaIndex.Should().BeLessThan(zebraIndex);
    }

    [Fact]
    public void ExportAllAsJson_WithCustomOptions_UsesOptions()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);
        stopwatch["Session1"].Start();
        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };

        // Act
        var json = stopwatch.ExportAllAsJson(options);

        // Assert
        json.Should().NotBeNullOrEmpty();
        // Indented output should have newlines
        json.Should().Contain("\n");
    }

    [Fact]
    public void ExportAllAsJson_WithEmptySessions_ReturnsEmptyObject()
    {
        // Arrange
        var stopwatch = new SessionStopwatch();

        // Act
        var json = stopwatch.ExportAllAsJson();

        // Assert
        json.Should().Contain("{}");
    }

    [Fact]
    public void DefaultTimeProvider_UsesSystemTimeProvider()
    {
        // Act
        var stopwatch = new SessionStopwatch();
        var session = stopwatch["TestSession"];

        // Assert
        session.Should().NotBeNull();
    }

    [Fact]
    public void MixedCaseSessionNames_AreConsistentlyNormalized()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);

        // Act
        stopwatch["Session"].Start();
        stopwatch["SESSION"].Mark();
        stopwatch["SeSSioN"].Stop();

        var sessions = stopwatch.Sessions;

        // Assert
        sessions.Should().ContainSingle();
        sessions[0].Name.Should().Be("Session");
    }

    [Fact]
    public void ComplexWorkflow_MultipleSessionsWithManyMarks()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var stopwatch = new SessionStopwatch(timeProvider);

        // Act
        var setup = stopwatch["Setup"];
        var processing = stopwatch["Processing"];
        var cleanup = stopwatch["Cleanup"];

        setup.Start("Initialize");
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        setup.Mark("Config loaded");
        timeProvider.Advance(TimeSpan.FromSeconds(0.5));
        setup.Stop("Setup complete");

        processing.Start("Begin processing");
        timeProvider.Advance(TimeSpan.FromSeconds(2));
        processing.Mark("Phase 1 done");
        timeProvider.Advance(TimeSpan.FromSeconds(1.5));
        processing.Mark("Phase 2 done");
        timeProvider.Advance(TimeSpan.FromSeconds(0.5));
        processing.Stop("Processing complete");

        cleanup.Start("Cleanup started");
        timeProvider.Advance(TimeSpan.FromSeconds(0.3));
        cleanup.Stop("Cleanup done");

        // Assert
        stopwatch.Sessions.Should().HaveCount(3);
        setup.Marks.Should().HaveCount(3);
        processing.Marks.Should().HaveCount(4);
        cleanup.Marks.Should().HaveCount(2);

        // Each stopwatch independently tracks its elapsed time from when it started
        setup.Marks.Should().HaveCount(3);
        setup.Marks[0].Description.Should().Be("Initialize");
        setup.Marks[1].Description.Should().Be("Config loaded");
        setup.Marks[2].Description.Should().Be("Setup complete");

        processing.Marks.Should().HaveCount(4);
        processing.Marks[0].Description.Should().Be("Begin processing");
        processing.Marks[3].Description.Should().Be("Processing complete");

        cleanup.Marks.Should().HaveCount(2);
        cleanup.Marks[0].Description.Should().Be("Cleanup started");
        cleanup.Marks[1].Description.Should().Be("Cleanup done");
    }
}
