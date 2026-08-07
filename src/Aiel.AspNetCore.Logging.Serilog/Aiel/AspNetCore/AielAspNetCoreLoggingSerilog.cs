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
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Aiel.AspNetCore;

[DependsOn(typeof(AielLoggingAbstractions))]
public sealed class AielAspNetCoreLoggingSerilog : AielDependencyConfigurator
{
    public override Task ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        // register IHttpContextAccessor and the enricher in the app's DI container
        context.Services.AddHttpContextAccessor();
        context.Services.AddSingleton<Serilog.Core.ILogEventEnricher, AielRequestHeadersEnricher>();

        // configure Serilog and let it resolve enrichers from the real service provider
        context.Services.AddSerilog((services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        return Task.CompletedTask;
    }
}
