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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aiel.Framework;

public sealed class AielWebApplication : IHost, IApplicationBuilder, IEndpointRouteBuilder, IAsyncDisposable
{
    private readonly WebApplication _webApplication;
    private readonly IApplicationBuilder _applicationBuilder;
    private readonly IEndpointRouteBuilder _endpointRouteBuilder;

    internal AielWebApplication(WebApplication application)
    {
        _webApplication = application;
        _applicationBuilder = application;
        _endpointRouteBuilder = application;
    }

    /// <summary>
    /// The application's configured services.
    /// </summary>
    public IServiceProvider Services => _webApplication.Services;

    /// <summary>
    /// The application's configured <see cref="IConfiguration"/>.
    /// </summary>
    public IConfiguration Configuration => _webApplication.Services.GetRequiredService<IConfiguration>();

    /// <summary>
    /// The application's configured <see cref="IWebHostEnvironment"/>.
    /// </summary>
    public IWebHostEnvironment Environment => _webApplication.Services.GetRequiredService<IWebHostEnvironment>();

    /// <summary>
    /// Allows consumers to be notified of application lifetime events.
    /// </summary>
    public IHostApplicationLifetime Lifetime => _webApplication.Services.GetRequiredService<IHostApplicationLifetime>();

    /// <summary>
    /// The default logger for the application.
    /// </summary>
    public ILogger Logger => _webApplication.Logger;

    /// <summary>
    /// The list of URLs that the HTTP server is bound to.
    /// </summary>
    public ICollection<String> Urls => _webApplication.Urls;

    IServiceProvider IApplicationBuilder.ApplicationServices
    {
        get => _applicationBuilder.ApplicationServices;
        set => _applicationBuilder.ApplicationServices = value;
    }

    // IApplicationBuilder implementation
    IFeatureCollection IApplicationBuilder.ServerFeatures => _applicationBuilder.ServerFeatures;

    IDictionary<String, Object?> IApplicationBuilder.Properties => _applicationBuilder.Properties;

    ICollection<EndpointDataSource> IEndpointRouteBuilder.DataSources => _endpointRouteBuilder.DataSources;
    IApplicationBuilder IApplicationBuilder.New() => _applicationBuilder.New();
    internal RequestDelegate BuildRequestDelegate() => _applicationBuilder.Build();
    RequestDelegate IApplicationBuilder.Build() => BuildRequestDelegate();

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => _applicationBuilder.Use(middleware);

    // IEndpointRouteBuilder implementation
    IServiceProvider IEndpointRouteBuilder.ServiceProvider => Services;
    IApplicationBuilder IEndpointRouteBuilder.CreateApplicationBuilder() => ((IApplicationBuilder)this).New();

    // IHost implementation
    public Task StartAsync(CancellationToken cancellationToken = default) => _webApplication.StartAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken = default) => _webApplication.StopAsync(cancellationToken);

    // Dispose implementations
    void IDisposable.Dispose() => ((IDisposable)_webApplication).Dispose();
    public ValueTask DisposeAsync() => _webApplication.DisposeAsync();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplication"/> class with preconfigured defaults.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="AielWebApplication"/>.</returns>
    public static AielWebApplication Create(String[]? args = null)
        => new(WebApplication.CreateBuilder(args ?? []).Build());

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with preconfigured defaults.
    /// </summary>
    /// <returns>The <see cref="AielWebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateBuilder()
        => new(WebApplication.CreateBuilder());

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with preconfigured defaults.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateBuilder(String[] args)
        => new(WebApplication.CreateBuilder(args));

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with preconfigured defaults.
    /// </summary>
    /// <param name="options">The <see cref="WebApplicationOptions"/> to configure the <see cref="WebApplicationBuilder"/>.</param>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateBuilder(WebApplicationOptions options)
        => new(WebApplication.CreateBuilder(options));

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with minimal defaults.
    /// </summary>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateSlimBuilder()
        => new(WebApplication.CreateSlimBuilder());

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with minimal defaults.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateSlimBuilder(String[] args)
        => new(WebApplication.CreateSlimBuilder(args));

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with minimal defaults.
    /// </summary>
    /// <param name="options">The <see cref="WebApplicationOptions"/> to configure the <see cref="WebApplicationBuilder"/>.</param>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateSlimBuilder(WebApplicationOptions options)
        => new(WebApplication.CreateSlimBuilder(options));

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with no defaults.
    /// </summary>
    /// <param name="options">The <see cref="WebApplicationOptions"/> to configure the <see cref="WebApplicationBuilder"/>.</param>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static AielWebApplicationBuilder CreateEmptyBuilder(WebApplicationOptions options)
        => new(WebApplication.CreateEmptyBuilder(options));
}
