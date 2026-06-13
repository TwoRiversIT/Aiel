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

using Aiel.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aiel.Authorization.AspNetCore;

public static class RescheduleAppointmentEndpoint
{
    public const String RoutePattern = "/appointments/reschedule";

    public static RouteHandlerBuilder MapRescheduleAppointment(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints.MapPost(RoutePattern, HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        RescheduleAppointmentRequest request,
        HttpContext httpContext,
        IAppointmentApplicationService applicationService,
        IExecutionContextFactory executionContextFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(applicationService);
        ArgumentNullException.ThrowIfNull(executionContextFactory);

        if (!AuthorizationScopeKey.TryCreate(request.LocationScopeKey, out var locationScopeKey))
        {
            return ToHttpResult(Result.Failure(new ResultError("LocationScopeKey must not be empty.")));
        }

        var action = new RescheduleAppointment(
            request.AppointmentId,
            locationScopeKey,
            request.StartsAtUtc,
            request.EndsAtUtc);

        var result = await applicationService.RescheduleAsync(
            executionContextFactory.Create(httpContext),
            action,
            cancellationToken).ConfigureAwait(false);

        return ToHttpResult(result);
    }

    private static JsonHttpResult<Result> ToHttpResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return TypedResults.Json(result, statusCode: StatusCodes.Status200OK);
        }

        var statusCode = result.Error switch
        {
            AuthorizationValidationError => StatusCodes.Status400BadRequest,
            AuthorizationDeniedError => StatusCodes.Status403Forbidden,
            ResultError => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Json(result, statusCode: statusCode);
    }
}
