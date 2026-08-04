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

using Aiel.Users;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Aiel.Logging;

/// <summary>
/// Enriches log events with a MachineName property containing <see cref="Environment.MachineName"/>.
/// </summary>
[DebuggerNonUserCode]
public class AielCurrentUserEnricher(IUserAccessor accessor) : ILogEventEnricher
{
    private readonly IUserAccessor _accessor = accessor;

    /// <summary>
    /// Enrich the log event.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
    [DebuggerStepThrough]
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var user = _accessor.Current ?? WellKnownUsers.Anonymous;

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(AielLoggingConsts.CurrentUser, user));

        // ToDo: When Impersonation is implemented, we will need to add the impersonator to the log event as well.
    }
}

public static partial class LoggerEnrichmentConfigurationExtensions
{
    public static LoggerConfiguration WithCurrentUser(this LoggerEnrichmentConfiguration enrichmentConfiguration, IUserAccessor userAccessor)
        => enrichmentConfiguration.With(new AielCurrentUserEnricher(userAccessor));
}
