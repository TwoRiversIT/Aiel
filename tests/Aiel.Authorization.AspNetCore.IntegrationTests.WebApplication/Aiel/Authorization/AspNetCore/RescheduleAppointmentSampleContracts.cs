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

using Aiel.Commands;
using Aiel.Execution;
using Aiel.Authorization.Testing;
using Aiel.Results;

namespace Aiel.Authorization.AspNetCore;

public static class RescheduleAppointmentPermissionMetadata
{
    public const String PermissionName = "sample.Scheduling.RescheduleAppointment";
    public const String GrantScopeType = "Location";
    public const String SubjectType = "User";
    public const String DisplayName = "Reschedule appointment";
    public const String Description = "Reference slice for Task 12 transport sample.";
    public const String StableId = "perm_test_sample_reschedule_appointment";
}

[DefinesPermission(
    RescheduleAppointmentPermissionMetadata.PermissionName,
    RescheduleAppointmentPermissionMetadata.GrantScopeType,
    RescheduleAppointmentPermissionMetadata.SubjectType,
    RescheduleAppointmentPermissionMetadata.DisplayName,
    Description = RescheduleAppointmentPermissionMetadata.Description,
    StableId = RescheduleAppointmentPermissionMetadata.StableId)]
public sealed record RescheduleAppointment(
    Guid AppointmentId,
    PermissionScopeKey LocationScopeKey,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : ICommand;

public interface IAppointmentApplicationService
{
    Task<Result> RescheduleAsync(
        IExecutionContext context,
        RescheduleAppointment action,
        CancellationToken cancellationToken = default);
}

public interface IExecutionContextFactory
{
    IExecutionContext Create(HttpContext httpContext);
}

public sealed class DefaultExecutionContextFactory : IExecutionContextFactory
{
    public IExecutionContext Create(HttpContext httpContext)
    {
        _ = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        return DefaultExecutionContext.CreateRoot(new RescheduleAppointmentActor(PermissionTestData.SubjectKeyAlpha));
    }
}

public sealed class UnconfiguredAppointmentApplicationService : IAppointmentApplicationService
{
    public Task<Result> RescheduleAsync(
        IExecutionContext context,
        RescheduleAppointment action,
        CancellationToken cancellationToken = default)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));
        _ = action ?? throw new ArgumentNullException(nameof(action));
        return Task.FromResult(Result.Failure(new ResultError("No sample appointment application service is configured.")));
    }
}

public sealed record RescheduleAppointmentActor(PermissionSubjectKey SubjectKey) : IActor;

public sealed class RescheduleAppointmentRequest
{
    private String? _locationScopeKey;

    public Guid AppointmentId { get; init; }

    public String LocationScopeKey
    {
        get => _locationScopeKey ?? String.Empty;
        init => _locationScopeKey = value;
    }

    public DateTimeOffset StartsAtUtc { get; init; }

    public DateTimeOffset EndsAtUtc { get; init; }
}
