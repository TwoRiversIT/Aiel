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
/// Contract tests for tenant-required endpoints.
/// Each resolution outcome must map to a deterministic HTTP status code.
/// </summary>
public sealed class TenantRequiredEndpointTests
{
    [Fact]
    public async Task Resolved_TenantResolution_Returns200()
    {
        var outcome = new TenantResolution.Resolved(new TenantIdentity(new TenantId(Guid.NewGuid())));
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Be(TestEndpointResponses.TenantRequired);
    }

    [Fact]
    public async Task Missing_TenantResolution_Returns401()
    {
        var outcome = new TenantResolution.Missing();
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ambiguous_TenantResolution_Returns400()
    {
        var outcome = new TenantResolution.Ambiguous();
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Rejected_TenantResolution_Returns403()
    {
        var outcome = new TenantResolution.Rejected(TenantRejectionReason.TenantInactive);
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Error_TenantResolution_Returns503()
    {
        var outcome = new TenantResolution.Error(TenantResolutionErrorReason.MembershipLookupFailed);
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
