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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Aiel.Execution;
using Aiel.Authorization.Testing;
using Aiel.Results;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;

namespace Aiel.Authorization.AspNetCore;

public sealed class RescheduleAppointmentTransportSampleTests
{
    [Fact]
    public async Task Endpoint_DelegatesOnceToApplicationService()
    {
        var applicationService = new RecordingAppointmentApplicationService(Result.Success());
        await using var factory = new PermissionsAspNetCoreWebApplicationFactory(applicationService);
        using var client = factory.CreateClient();
        var request = CreateRequest();

        var response = await client.PostAsJsonAsync(
            RescheduleAppointmentEndpoint.RoutePattern,
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        applicationService.CallCount.Should().Be(1);
        applicationService.LastAction.Should().NotBeNull();
        applicationService.LastAction!.AppointmentId.Should().Be(request.AppointmentId);
        applicationService.LastAction.LocationScopeKey.Value.Should().Be(request.LocationScopeKey);
        applicationService.LastAction.StartsAtUtc.Should().Be(request.StartsAtUtc);
        applicationService.LastAction.EndsAtUtc.Should().Be(request.EndsAtUtc);
    }

    [Fact]
    public async Task Endpoint_DoesNotDuplicatePermissionMetadata()
    {
        await using var factory = new PermissionsAspNetCoreWebApplicationFactory(new RecordingAppointmentApplicationService(Result.Success()));

        var endpointDataSource = factory.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Single(item => item.RoutePattern.RawText == RescheduleAppointmentEndpoint.RoutePattern);
        var handlerMethod = typeof(RescheduleAppointmentEndpoint).GetMethod(
            "HandleAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        endpoint.Metadata.OfType<IAuthorizeData>().Should().BeEmpty();
        handlerMethod.Should().NotBeNull();
        handlerMethod!.GetCustomAttributes(inherit: false).OfType<AuthorizeAttribute>().Should().BeEmpty();
        handlerMethod.GetCustomAttributes(inherit: false).OfType<DefinesPermissionAttribute>().Should().BeEmpty();
    }

    [Fact]
    public async Task HttpClient_PreservesSuccessfulResultSemantics()
    {
        await using var factory = new PermissionsAspNetCoreWebApplicationFactory(new RecordingAppointmentApplicationService(Result.Success()));
        using var httpClient = factory.CreateClient();
        var transportClient = new RescheduleAppointmentHttpClient(httpClient);

        var result = await transportClient.RescheduleAsync(
            CreateExecutionContext(),
            CreateAction(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Error.GetType().Name.Should().Be("NoError");
    }

    [Fact]
    public async Task HttpClient_PreservesFailedResultSemantics()
    {
        var failure = Result.Failure(
            PermissionErrors.PermissionDenied(
                PermissionName.From(RescheduleAppointmentPermissionMetadata.PermissionName)));
        await using var factory = new PermissionsAspNetCoreWebApplicationFactory(new RecordingAppointmentApplicationService(failure));
        using var httpClient = factory.CreateClient();
        var transportClient = new RescheduleAppointmentHttpClient(httpClient);

        var result = await transportClient.RescheduleAsync(
            CreateExecutionContext(),
            CreateAction(),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<PermissionDeniedError>();
    }

    private static RescheduleAppointmentRequest CreateRequest()
        => new()
        {
            AppointmentId = PermissionTestData.AppointmentIdAlpha,
            LocationScopeKey = PermissionTestData.ScopeKeyAlpha.Value,
            StartsAtUtc = DateTimeOffset.Parse("2026-05-26T15:00:00Z"),
            EndsAtUtc = DateTimeOffset.Parse("2026-05-26T16:00:00Z")
        };

    private static RescheduleAppointment CreateAction()
        => new(
            PermissionTestData.AppointmentIdAlpha,
            PermissionTestData.ScopeKeyAlpha,
            DateTimeOffset.Parse("2026-05-26T15:00:00Z"),
            DateTimeOffset.Parse("2026-05-26T16:00:00Z"));

    private static DefaultExecutionContext CreateExecutionContext()
        => DefaultExecutionContext.CreateRoot(new RescheduleAppointmentActor(PermissionTestData.SubjectKeyAlpha));
}
