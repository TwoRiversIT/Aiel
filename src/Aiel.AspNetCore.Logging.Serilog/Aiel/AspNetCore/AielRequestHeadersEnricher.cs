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

using Aiel.Framework;
using Aiel.Logging;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Aiel.AspNetCore;

public sealed class AielRequestHeadersEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    private const String UserAgent = "UserAgent";

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor
        ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    [DebuggerStepThrough]
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null)
        {
            return;
        }

        Add(ctx, propertyFactory, logEvent, AielLoggingConsts.Instance, AielHeaders.ClientInstanceHeader);
        Add(ctx, propertyFactory, logEvent, AielLoggingConsts.Version, AielHeaders.ClientVersionHeader);
        Add(ctx, propertyFactory, logEvent, AielLoggingConsts.UserAgent, UserAgent);
    }

    private static void Add(HttpContext ctx, ILogEventPropertyFactory factory, LogEvent logEvent, String property, String header)
    {
        // cache in HttpContext.Items to avoid repeated header parsing during the same request
        var cached = ctx.Items[header] as String;
        if (cached == null)
        {
            if (ctx.Request.Headers.TryGetValue(header, out var value))
            {
                cached = value.FirstOrDefault();
                ctx.Items[header] = cached;
            }
        }

        if (!String.IsNullOrWhiteSpace(cached))
        {
            logEvent.AddPropertyIfAbsent(factory.CreateProperty(property, cached, false));
        }
    }
}
