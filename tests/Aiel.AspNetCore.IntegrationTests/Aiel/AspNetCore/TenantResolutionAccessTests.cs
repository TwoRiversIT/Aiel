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

using Aiel.MultiTenancy;
using System.Net;

namespace Aiel.AspNetCore;

/// <summary>
/// Contract tests for downstream ASP.NET tenant access after the middleware resolves the request tenant.
/// These tests lock the public access seam so consumers are not forced to depend on internal HTTP features.
/// </summary>
public sealed class TenantResolutionAccessTests
{
    [Fact]
    public async Task Resolved_TenantResolution_IsReadableFromHttpContext()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var outcome = new TenantResolution.Resolved(new TenantIdentity(tenantId));
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-resolution", TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Be(tenantId.Value.ToString("D"));
    }

    [Fact]
    public async Task Missing_TenantResolution_IsReadableFromHttpContext()
    {
        var outcome = new TenantResolution.Missing();
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-resolution", TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Be(TestEndpointResponses.TenantResolutionMissing);
    }

    [Fact]
    public async Task Resolved_TenantIdentity_IsReadableFromTenantAccessor()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var outcome = new TenantResolution.Resolved(new TenantIdentity(tenantId));
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-accessor", TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Be(tenantId.Value.ToString("D"));
    }
}
