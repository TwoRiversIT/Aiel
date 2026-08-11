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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Aiel.Framework;

public sealed class WebApplicationInitializationContext(IApplicationBuilder webApplication)
    : InitializationContext(webApplication.ApplicationServices), IHost, IApplicationBuilder, IEndpointRouteBuilder
{
    private readonly IApplicationBuilder _applicationBuilder = webApplication;
    private readonly IHost _host = webApplication as IHost
        ?? throw new AielException("The provided IApplicationBuilder must implement IHost.");
    private readonly IEndpointRouteBuilder _endpointRouteBuilder = webApplication as IEndpointRouteBuilder
        ?? throw new AielException("The provided IApplicationBuilder must implement IEndpointRouteBuilder.");

    public IServiceProvider Services => _host.Services;

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _host.StopAsync(cancellationToken);

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => _applicationBuilder.Use(middleware);

    void IDisposable.Dispose() => _host.Dispose();

    // IApplicationBuilder implementation
    IServiceProvider IApplicationBuilder.ApplicationServices { get => _applicationBuilder.ApplicationServices; set => _applicationBuilder.ApplicationServices = value; }
    IDictionary<String, Object?> IApplicationBuilder.Properties => _applicationBuilder.Properties;
    IFeatureCollection IApplicationBuilder.ServerFeatures => _applicationBuilder.ServerFeatures;
    RequestDelegate IApplicationBuilder.Build() => _applicationBuilder.Build();
    IApplicationBuilder IApplicationBuilder.New() => _applicationBuilder.New();

    // IEndpointRouteBuilder implementation
    ICollection<EndpointDataSource> IEndpointRouteBuilder.DataSources => _endpointRouteBuilder.DataSources;
    IServiceProvider IEndpointRouteBuilder.ServiceProvider => _endpointRouteBuilder.ServiceProvider;
    IApplicationBuilder IEndpointRouteBuilder.CreateApplicationBuilder() => _endpointRouteBuilder.CreateApplicationBuilder();
}
