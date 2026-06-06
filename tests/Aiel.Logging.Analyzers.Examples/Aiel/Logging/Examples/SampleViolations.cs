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

// -----------------------------------------------------------------------
// SampleViolations.cs — Template / demo of Aiel logging rule violations
//
// This file intentionally contains code that triggers AIEL001–AIEL005
// so developers can see what the analyzers flag and how the code fixes
// transform the violations.
//
// DO NOT include this file in production builds.
// -----------------------------------------------------------------------

#pragma warning disable // suppress warnings in this demo-only file

using Aiel.Logging.Examples;
using Microsoft.Extensions.Logging;

namespace Aiel.Logging.Template;

public static partial class SampleViolations
{
    // ── AIEL001 violation ────────────────────────────────────────────────
    // EventId uses a raw integer literal instead of (int)AielEventIds.XXX.
    //
    // Fix: replace `1000` with `(int)AielEventIds.ServiceStart`
    [LoggerMessage(
        EventId = 1000,                          // ← AIEL001: raw int
        Level = LogLevel.Information,
        Message = "[{EventId}] Service started")]
    public static partial void Aiel001_RawInt(
        this ILogger logger,
        AielEventIds eventId = AielEventIds.ServiceStart);

    // ── AIEL001 violation (wrong enum) ────────────────────────────────────
    // EventId uses a cast from an unrecognised enum.
    //
    // Fix: replace the cast expression with `(int)AielEventIds.ServiceStart`
    [LoggerMessage(
        EventId = (int)SomeOtherEnum.Foo,        // ← AIEL001: wrong enum
        Level = LogLevel.Warning,
        Message = "[{EventId}] Wrong enum")]
    public static partial void Aiel001_WrongEnum(
        this ILogger logger,
        AielEventIds eventId = AielEventIds.ServiceStart);

    // ── AIEL002 violation ────────────────────────────────────────────────
    // No optional AielEventIds parameter.
    //
    // Fix: append `AielEventIds eventId = AielEventIds.ServiceStop`
    [LoggerMessage(
        EventId = (int)AielEventIds.ServiceStop,
        Level = LogLevel.Information,
        Message = "[{EventId}] Service stopped")]
    public static partial void Aiel002_MissingParam(   // ← AIEL002: no eventId param
        this ILogger logger);

    // ── AIEL003 violation ────────────────────────────────────────────────
    // Message template does not start with "[{EventId}]".
    //
    // Fix: prepend "[{EventId}] " to the message string
    [LoggerMessage(
        EventId = (int)AielEventIds.RequestStart,
        Level = LogLevel.Debug,
        Message = "Request received")]             // ← AIEL003: no [{{EventId}}] placeholder
    public static partial void Aiel003_MissingPlaceholder(
        this ILogger logger,
        AielEventIds eventId = AielEventIds.RequestStart);

    // ── AIEL004 violation ────────────────────────────────────────────────
    // Direct ILogger extension call instead of a [LoggerMessage] partial method.
    //
    // Fix: replace with the appropriate SampleLog.XXX call (manual step),
    //      or use the "Remove" fix if the call is redundant.
    public static void Aiel004_DirectCall(ILogger logger)
    {
        logger.LogInformation("Service started"); // ← AIEL004: direct ILogger call
    }

    // ── AIEL005 violation ────────────────────────────────────────────────
    // The attribute EventId and the parameter default disagree.
    //
    // Fix A: update attribute EventId to match parameter → (int)AielEventIds.RequestEnd
    // Fix B: update parameter default to match attribute → AielEventIds.RequestStart
    [LoggerMessage(
        EventId = (int)AielEventIds.RequestStart, // ← AIEL005: mismatch
        Level = LogLevel.Information,
        Message = "[{EventId}] Request ended")]
    public static partial void Aiel005_Mismatch(
        this ILogger logger,
        AielEventIds eventId = AielEventIds.RequestEnd); // ← different member
}

/// <summary>Placeholder enum to illustrate AIEL001 wrong-enum violation.</summary>
public enum SomeOtherEnum
{
    Foo = 99,
}
