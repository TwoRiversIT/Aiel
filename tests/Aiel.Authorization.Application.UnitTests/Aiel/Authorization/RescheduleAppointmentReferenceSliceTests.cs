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

using Aiel.Authorization.Testing;
using Aiel.Results;
using Aiel.Actions;
using Aiel.Actions.Commands;

namespace Aiel.Authorization;

public sealed class RescheduleAppointmentReferenceSliceTests
{
    [Fact]
    public async Task RescheduleAsync_WhenAppointmentIdIsDefault_ReturnsValidationErrorAndSkipsPermissionChecker()
    {
        var log = new List<String>();
        var services = CreateSliceServices(
            log,
            grantDecision: AuthorizationGrantDecision.Granted,
            resourceAuthorizationResult: Result.Success());
        var applicationService = CreateApplicationService(services);
        var command = new RescheduleAppointment(
            Guid.Empty,
            AuthorizationTestData.ScopeKeyAlpha,
            DateTimeOffset.Parse("2026-05-26T15:00:00Z"),
            DateTimeOffset.Parse("2026-05-26T16:00:00Z"));

        var result = await applicationService.RescheduleAsync(
            CreateExecutionContext(),
            command,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<AuthorizationValidationError>();
        services.GrantEvaluator.CallCount.Should().Be(0);
        log.Should().Equal("validate");
    }

    [Fact]
    public async Task RescheduleAsync_WhenGrantIsDenied_DoesNotLoadAggregate()
    {
        var log = new List<String>();
        var services = CreateSliceServices(
            log,
            grantDecision: AuthorizationGrantDecision.Prohibited,
            resourceAuthorizationResult: Result.Success());
        var applicationService = CreateApplicationService(services);
        var command = CreateValidCommand();

        var result = await applicationService.RescheduleAsync(
            CreateExecutionContext(),
            command,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<AuthorizationDeniedError>();
        services.Repository.LoadCallCount.Should().Be(0);
        log.Should().Equal("validate", "resolve-scope", "resolve-subject", "grant");
    }

    [Fact]
    public async Task RescheduleAsync_WhenResourceAuthorizationFails_DoesNotSave()
    {
        var log = new List<String>();
        var services = CreateSliceServices(
            log,
            grantDecision: AuthorizationGrantDecision.Granted,
            resourceAuthorizationResult: Result.Failure(
                AuthorizationErrors.PermissionDenied(PermissionName.From(GeneratedAuthorizationNames.RescheduleAppointment))));
        var applicationService = CreateApplicationService(services);
        var command = CreateValidCommand();

        var result = await applicationService.RescheduleAsync(
            CreateExecutionContext(),
            command,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<AuthorizationDeniedError>();
        services.Repository.LoadCallCount.Should().Be(1);
        services.Repository.SaveCallCount.Should().Be(0);
        services.Repository.Appointment.RescheduleCallCount.Should().Be(0);
        log.Should().Equal("validate", "resolve-scope", "resolve-subject", "grant", "load", "resource");
    }

    [Fact]
    public async Task RescheduleAsync_WhenEverythingSucceeds_ReschedulesOnceAndSavesOnce()
    {
        var log = new List<String>();
        var services = CreateSliceServices(
            log,
            grantDecision: AuthorizationGrantDecision.Granted,
            resourceAuthorizationResult: Result.Success());
        var applicationService = CreateApplicationService(services);
        var command = CreateValidCommand();

        var result = await applicationService.RescheduleAsync(
            CreateExecutionContext(),
            command,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        services.Repository.LoadCallCount.Should().Be(1);
        services.ResourceAuthorization.CallCount.Should().Be(1);
        services.Repository.Appointment.RescheduleCallCount.Should().Be(1);
        services.Repository.SaveCallCount.Should().Be(1);
        log.Should().Equal("validate", "resolve-scope", "resolve-subject", "grant", "load", "resource", "reschedule", "save");
    }

    [Fact]
    public void GeneratedPermissionManifests_ContainsRescheduleAppointmentMetadata()
    {
        var manifest = GeneratedAuthorizationManifests.GetManifests()
            .Single(item => item.ActionType == typeof(RescheduleAppointment));

        manifest.PermissionName.Value.Should().Be(RescheduleAppointmentMetadata.ActionName);
        manifest.StableId.Value.Should().Be(RescheduleAppointmentMetadata.StableId);
        manifest.ScopeType.Value.Should().Be(RescheduleAppointmentMetadata.GrantScopeType);
        manifest.SubjectType.Value.Should().Be(RescheduleAppointmentMetadata.SubjectType);
        manifest.PreviousNames.Should().BeEmpty();
    }

    private static RescheduleAppointment CreateValidCommand()
        => new(
            AuthorizationTestData.AppointmentIdAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            DateTimeOffset.Parse("2026-05-26T15:00:00Z"),
            DateTimeOffset.Parse("2026-05-26T16:00:00Z"));

    private static DefaultExecutionContext CreateExecutionContext()
        => DefaultExecutionContext.CreateRoot(new RescheduleAppointmentActor(AuthorizationTestData.SubjectKeyAlpha));

    private static AppointmentApplicationService CreateApplicationService(SliceServices services)
    {
        return new AppointmentApplicationService(
            new DefaultActionGate<RescheduleAppointment>(services.ServiceProvider),
            services.Repository,
            services.ResourceAuthorization);
    }

    private static SliceServices CreateSliceServices(
        List<String> log,
        AuthorizationGrantDecision grantDecision,
        Result resourceAuthorizationResult)
    {
        var validator = new RescheduleAppointmentValidator(log);
        var scopeResolver = new RescheduleAppointmentScopeResolver(log);
        var subjectResolver = new RescheduleAppointmentSubjectResolver(log);
        var grantEvaluator = new RecordingPermissionGrantEvaluator(log, grantDecision);
        var checker = new RescheduleAppointmentPermissionChecker(grantEvaluator, scopeResolver, subjectResolver);
        var repository = new RecordingAppointmentRepository(log);
        var resourceAuthorization = new RecordingResourceAuthorizationService(log, resourceAuthorizationResult);
        var serviceProvider = new SliceServiceProvider(validator, checker);

        return new SliceServices(serviceProvider, grantEvaluator, repository, resourceAuthorization);
    }

    private sealed record SliceServices(
        SliceServiceProvider ServiceProvider,
        RecordingPermissionGrantEvaluator GrantEvaluator,
        RecordingAppointmentRepository Repository,
        RecordingResourceAuthorizationService ResourceAuthorization);
}

internal static class RescheduleAppointmentMetadata
{
    public const String ActionName = "sample.Scheduling.RescheduleAppointment";
    public const String GrantScopeType = "Location";
    public const String ResourceScopeType = "Appointment";
    public const String SubjectType = "User";
    public const String DisplayName = "Reschedule appointment";
    public const String Description = "Reference slice for Task 11.";
    public const String StableId = "perm_test_sample_reschedule_appointment";
}

[AuthorizationDefinition(
    RescheduleAppointmentMetadata.ActionName,
    RescheduleAppointmentMetadata.GrantScopeType,
    RescheduleAppointmentMetadata.SubjectType,
    RescheduleAppointmentMetadata.DisplayName,
    Description = RescheduleAppointmentMetadata.Description,
    StableId = RescheduleAppointmentMetadata.StableId)]
internal sealed record RescheduleAppointment(
    Guid AppointmentId,
    AuthorizationScopeKey LocationScopeKey,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc) : ICommand;

internal interface IAppointmentApplicationService
{
    Task<Result> RescheduleAsync(
        IExecutionContext context,
        RescheduleAppointment action,
        CancellationToken cancellationToken = default);
}

internal sealed class AppointmentApplicationService(
    IActionGate<RescheduleAppointment> gate,
    IAppointmentRepository repository,
    IResourceAuthorizationService resourceAuthorizationService) : IAppointmentApplicationService
{
    public async Task<Result> RescheduleAsync(
        IExecutionContext context,
        RescheduleAppointment action,
        CancellationToken cancellationToken = default)
    {
        var gateResult = await gate.AuthorizeAsync(context, action, cancellationToken).ConfigureAwait(false);
        if (!gateResult.IsSuccess)
        {
            return Result.Failure(gateResult.Error);
        }

        var appointmentResult = await repository.LoadAsync(action.AppointmentId, cancellationToken).ConfigureAwait(false);
        if (!appointmentResult.IsSuccess)
        {
            return Result.Failure(appointmentResult.Error);
        }

        var resourceResult = await resourceAuthorizationService.AuthorizeAsync(
            gateResult.Value,
            PermissionName.From(GeneratedAuthorizationNames.RescheduleAppointment),
            AuthorizationScopeTypeName.From(RescheduleAppointmentMetadata.ResourceScopeType),
            appointmentResult.Value.ResourceScopeKey,
            cancellationToken).ConfigureAwait(false);
        if (!resourceResult.IsSuccess)
        {
            return resourceResult;
        }

        appointmentResult.Value.Reschedule(action.StartsAtUtc, action.EndsAtUtc);
        return await repository.SaveAsync(appointmentResult.Value, cancellationToken).ConfigureAwait(false);
    }
}

internal sealed class RescheduleAppointmentValidator(List<String> log)
    : IActionValidator<RescheduleAppointment>
{
    public Task<Result> ValidateAsync(
        IActionExecutionContext<RescheduleAppointment> context,
        CancellationToken cancellationToken = default)
    {
        log.Add("validate");

        if (context.Action.AppointmentId == Guid.Empty)
        {
            return Task.FromResult(
                Result.Failure(
                    AuthorizationErrors.ValidationFailed(
                        PermissionName.From(GeneratedAuthorizationNames.RescheduleAppointment),
                        "AppointmentId must not be empty.")));
        }

        return Task.FromResult(Result.Success());
    }
}

internal sealed class RescheduleAppointmentScopeResolver(List<String> log)
    : IAuthorizationScopeResolver<RescheduleAppointment>
{
    public Task<Result<AuthorizationScopeResolution>> ResolveAsync(
        IActionExecutionContext<RescheduleAppointment> context,
        CancellationToken cancellationToken = default)
    {
        log.Add("resolve-scope");
        return Task.FromResult(Result.Success(new AuthorizationScopeResolution(
            AuthorizationScopeTypeName.From(RescheduleAppointmentMetadata.GrantScopeType),
            context.Action.LocationScopeKey)));
    }
}

internal sealed class RescheduleAppointmentSubjectResolver(List<String> log)
    : IAuthorizationSubjectResolver<RescheduleAppointment>
{
    public AuthorizationSubjectKey ResolveSubjectKey(IActionExecutionContext<RescheduleAppointment> context)
    {
        log.Add("resolve-subject");
        return ((RescheduleAppointmentActor)context.Actor).SubjectKey;
    }
}

internal interface IAppointmentRepository
{
    Task<Result<TestAppointment>> LoadAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> SaveAsync(TestAppointment appointment, CancellationToken cancellationToken = default);
}

internal sealed class RecordingAppointmentRepository(List<String> log) : IAppointmentRepository
{
    public TestAppointment Appointment { get; } = new(log, AuthorizationTestData.AppointmentResourceScopeKeyAlpha);

    public Int32 LoadCallCount { get; private set; }

    public Int32 SaveCallCount { get; private set; }

    public Task<Result<TestAppointment>> LoadAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        LoadCallCount++;
        log.Add("load");
        return Task.FromResult(Result.Success(Appointment));
    }

    public Task<Result> SaveAsync(TestAppointment appointment, CancellationToken cancellationToken = default)
    {
        SaveCallCount++;
        log.Add("save");
        return Task.FromResult(Result.Success());
    }
}

internal sealed class RecordingPermissionGrantEvaluator(List<String> log, AuthorizationGrantDecision decision)
    : IAuthorizationGrantEvaluator
{
    public Int32 CallCount { get; private set; }

    public Task<Result<AuthorizationGrantDecision?>> EvaluateAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        log.Add("grant");
        return Task.FromResult(Result.Success<AuthorizationGrantDecision?>(decision));
    }
}

internal sealed class RecordingResourceAuthorizationService(List<String> log, Result result)
    : IResourceAuthorizationService
{
    public Int32 CallCount { get; private set; }

    public Task<Result> AuthorizeAsync(
        IExecutionContext context,
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        log.Add("resource");
        return Task.FromResult(result);
    }
}

internal sealed class TestAppointment(List<String> log, AuthorizationScopeKey resourceScopeKey)
{
    public AuthorizationScopeKey ResourceScopeKey { get; } = resourceScopeKey;

    public Int32 RescheduleCallCount { get; private set; }

    public void Reschedule(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        RescheduleCallCount++;
        log.Add("reschedule");
    }
}

internal sealed record RescheduleAppointmentActor(AuthorizationSubjectKey SubjectKey) : IActor;

internal sealed class SliceServiceProvider(
    IActionValidator<RescheduleAppointment> validator,
    IActionAuthorizationChecker<RescheduleAppointment> checker) : IServiceProvider
{
    public Object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IActionValidator<RescheduleAppointment>))
        {
            return validator;
        }

        if (serviceType == typeof(IActionAuthorizationChecker<RescheduleAppointment>))
        {
            return checker;
        }

        return null;
    }
}
