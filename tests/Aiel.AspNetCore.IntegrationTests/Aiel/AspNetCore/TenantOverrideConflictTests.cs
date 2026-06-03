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
/// Contract tests for X-Tenant-ID override header conflict detection.
/// When the header presents a different tenant than the actor-resolved tenant,
/// the middleware must short-circuit with 409 Conflict before reaching the handler.
/// A matching header (same ID) must not be treated as a conflict.
/// </summary>
public sealed class TenantOverrideConflictTests
{
    [Fact]
    public async Task XTenantId_ConflictsWithResolvedTenant_Returns409()
    {
        var resolvedTenantId = new TenantId(Guid.NewGuid());
        var differentTenantId = new TenantId(Guid.NewGuid());
        var outcome = new TenantResolution.Resolved(new TenantIdentity(resolvedTenantId));
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            TenantResolutionConstants.TenantIdOverrideHeaderName,
            differentTenantId.Value.ToString("D"));

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task XTenantId_MatchesResolvedTenant_Returns200()
    {
        var resolvedTenantId = new TenantId(Guid.NewGuid());
        var outcome = new TenantResolution.Resolved(new TenantIdentity(resolvedTenantId));
        using var factory = new TenantPipelineWebApplicationFactory(new StubTenantResolver(outcome));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            TenantResolutionConstants.TenantIdOverrideHeaderName,
            resolvedTenantId.Value.ToString("D"));

        var response = await client.GetAsync("/tenant-required", TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Be(TestEndpointResponses.TenantRequired);
    }
}
