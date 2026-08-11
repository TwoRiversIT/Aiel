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

using Aiel.Fakes;
using Microsoft.AspNetCore.Http.Features;

namespace Aiel.Framework;

public class WebApplicationDependencyManagerTests : AielDependencyManagerTests
{
    public override DependencyManager CreateDependencyManager(IEnumerable<DependencyDescriptor> descriptors)
        => new WebApplicationDependencyManager(descriptors);

    public override InitializationContext CreateInitializationContextAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<IAielEnvironment>(FakeAielEnvironment.Create());

        var serviceProvider = services.BuildServiceProvider();
        return new WebApplicationInitializationContext(new TestHostApplication(serviceProvider));
    }

    private class TestHostApplication(IServiceProvider services) : IApplicationBuilder, IHost, IEndpointRouteBuilder
    {
        public IServiceProvider ApplicationServices { get; set; } = services;
        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();
        public IDictionary<String, Object?> Properties { get; } = new Dictionary<String, Object?>();
        public IServiceProvider Services { get; } = services;
        public IServiceProvider ServiceProvider { get; } = services;
        public ICollection<EndpointDataSource> DataSources { get; } = [];

        public void Dispose()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public RequestDelegate Build()
        {
            throw new NotImplementedException();
        }

        public IApplicationBuilder New()
        {
            throw new NotImplementedException();
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            throw new NotImplementedException();
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            throw new NotImplementedException();
        }
    }
}
