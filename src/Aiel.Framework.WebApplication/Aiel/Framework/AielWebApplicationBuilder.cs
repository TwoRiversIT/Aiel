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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aiel.Framework;

public sealed class AielWebApplicationBuilder : IHostApplicationBuilder
{
    private readonly IHostApplicationBuilder _hostApplicationBuilder;
    private readonly WebApplicationBuilder _webApplicationBuilder;

    internal AielWebApplicationBuilder(WebApplicationBuilder webApplicationBuilder)
    {
        _webApplicationBuilder = webApplicationBuilder;
        _hostApplicationBuilder = webApplicationBuilder;
    }

    public IHostEnvironment Environment => _hostApplicationBuilder.Environment;
    public IServiceCollection Services => _webApplicationBuilder.Services;
    public IConfigurationManager Configuration => _hostApplicationBuilder.Configuration;
    public ILoggingBuilder Logging => _webApplicationBuilder.Logging;
    public IMetricsBuilder Metrics => _webApplicationBuilder.Metrics;

    public ConfigureWebHostBuilder WebHost => _webApplicationBuilder.WebHost;
    public ConfigureHostBuilder Host => _webApplicationBuilder.Host;

    IDictionary<Object, Object> IHostApplicationBuilder.Properties => _hostApplicationBuilder.Properties;

    void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure)
        => _hostApplicationBuilder.ConfigureContainer(factory, configure);

    public WebApplication Build() => _webApplicationBuilder.Build();
}
