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

namespace Aiel.Pipeline;

/// <summary>
/// Shared structured-logging helpers for command and query pipeline behaviors.
/// </summary>
internal static partial class PipelineLoggingHelper
{
    [LoggerMessage(EventId = (Int32)AielEvent.PipelineDispatching, Level = LogLevel.Information, Message = "[{EventId}] Dispatching {InputType} [CorrelationId={CorrelationId}]")]
    internal static partial void LogDispatching(this ILogger logger, String inputType, Guid correlationId, AielEvent eventId = AielEvent.PipelineDispatching);

    [LoggerMessage(EventId = (Int32)AielEvent.PipelineSuccess, Level = LogLevel.Information, Message = "[{EventId}] {InputType} dispatched successfully [CorrelationId={CorrelationId}]")]
    internal static partial void LogSuccess(this ILogger logger, String inputType, Guid correlationId, AielEvent eventId = AielEvent.PipelineSuccess);

    [LoggerMessage(EventId = (Int32)AielEvent.PipelineFailure, Level = LogLevel.Warning, Message = "[{EventId}] {InputType} dispatch failed [CorrelationId={CorrelationId}]")]
    internal static partial void LogFailure(this ILogger logger, String inputType, Guid correlationId, AielEvent eventId = AielEvent.PipelineFailure);
}
