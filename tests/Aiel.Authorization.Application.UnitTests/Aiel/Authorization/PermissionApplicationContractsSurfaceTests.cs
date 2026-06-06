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

namespace Aiel.Authorization;

public sealed class PermissionApplicationContractsSurfaceTests
{
    [Fact]
    public void ContractsAssembly_ExposesExpectedNamespace()
    {
        typeof(IActionGate<>).Namespace.Should().Be("Aiel.Authorization");
        typeof(IActionCapabilityService).Namespace.Should().Be("Aiel.Authorization");
        typeof(IActionValidator<>).Namespace.Should().Be("Aiel.Authorization");
        typeof(IActionAuthorizationChecker<>).Namespace.Should().Be("Aiel.Authorization");
        typeof(IAuthorizationDefinitionRegistry).Namespace.Should().Be("Aiel.Authorization");
        typeof(IAuthorizationGrantEvaluator).Namespace.Should().Be("Aiel.Authorization");
        typeof(IAuthorizationScopeResolver<>).Namespace.Should().Be("Aiel.Authorization");
        typeof(IAuthorizationGrantStore).Namespace.Should().Be("Aiel.Authorization");
        typeof(IAuthorizationManager).Namespace.Should().Be("Aiel.Authorization");
        typeof(IResourceAuthorizationService).Namespace.Should().Be("Aiel.Authorization");
    }

    [Fact]
    public void ContractsAssembly_ExposesExpectedDtoTypes()
    {
        typeof(ActionCapabilityRequest).Namespace.Should().Be("Aiel.Authorization");
        typeof(ActionCapabilitySnapshot).Namespace.Should().Be("Aiel.Authorization");
        typeof(ActionCapabilityRequestMode).Namespace.Should().Be("Aiel.Authorization");
        typeof(CapabilityContinuationToken).Namespace.Should().Be("Aiel.Authorization");
        typeof(AuthorizationDefinitionManifest).Namespace.Should().Be("Aiel.Authorization");
        typeof(AuthorizationGrantSummary).Namespace.Should().Be("Aiel.Authorization");
        typeof(AuthorizationScopeResolution).Namespace.Should().Be("Aiel.Authorization");
        typeof(GrantPermissionRequest).Namespace.Should().Be("Aiel.Authorization");
        typeof(RevokeAuthorizationRequest).Namespace.Should().Be("Aiel.Authorization");
        typeof(ClientAuthorizationCapability).Namespace.Should().Be("Aiel.Authorization");
    }

    [Fact]
    public void ActionCapabilityRequest_ForSelectedPermissions_UsesExplicitEmptyContinuationToken()
    {
        var permission = PermissionName.From("documents.read");
        var request = ActionCapabilityRequest.ForSelectedPermissions(
            AuthorizationScopeTypeName.From("Tenant"),
            AuthorizationScopeKey.From("tenant-1"),
            [permission],
            CapabilityContinuationToken.Empty);

        request.Mode.Should().Be(ActionCapabilityRequestMode.SelectedPermissions);
        request.RequestedPermissions.Should().ContainSingle().Which.Should().Be(permission);
        request.ContinuationToken.IsEmpty.Should().BeTrue();
        request.ContinuationToken.Value.Should().BeEmpty();
    }

    [Fact]
    public void PermissionDefinitionManifest_RequiresActionType()
    {
        typeof(AuthorizationDefinitionManifest).GetProperty(nameof(AuthorizationDefinitionManifest.ActionType)).Should().NotBeNull();
        typeof(AuthorizationDefinitionManifest).GetProperty(nameof(AuthorizationDefinitionManifest.ActionType))!.PropertyType.Should().Be<Type>();
    }

    [Fact]
    public void PermissionDefinitionManifest_ExposesLifecycleAndPreviousNames()
    {
        typeof(AuthorizationDefinitionManifest).GetProperty(nameof(AuthorizationDefinitionManifest.Lifecycle)).Should().NotBeNull();
        typeof(AuthorizationDefinitionManifest).GetProperty(nameof(AuthorizationDefinitionManifest.Lifecycle))!.PropertyType.Should().Be<PermissionLifecycle>();

        typeof(AuthorizationDefinitionManifest).GetProperty(nameof(AuthorizationDefinitionManifest.PreviousNames)).Should().NotBeNull();
        typeof(AuthorizationDefinitionManifest).GetProperty(nameof(AuthorizationDefinitionManifest.PreviousNames))!.PropertyType
            .Should().Be<IReadOnlyList<PermissionName>>();
    }

    [Fact]
    public void PermissionApplicationErrors_ExposesExpectedErrorTypes()
    {
        typeof(MissingAuthorizationStoryError).Should().BeAssignableTo<Error>();
        typeof(AuthorizationDeniedError).Should().BeAssignableTo<Error>();
        typeof(AuthorizationValidationError).Should().BeAssignableTo<Error>();
    }

    [Fact]
    public void PermissionErrors_MissingAuthorizationStory_ReturnsCorrectType()
    {
        var permission = PermissionName.From("test.permission");
        var error = AuthorizationErrors.MissingAuthorizationStory(permission);
        error.Should().BeOfType<MissingAuthorizationStoryError>();
    }

    [Fact]
    public void PermissionErrors_PermissionDenied_ReturnsCorrectType()
    {
        var permission = PermissionName.From("test.permission");
        var error = AuthorizationErrors.PermissionDenied(permission);
        error.Should().BeOfType<AuthorizationDeniedError>();
    }

    [Fact]
    public void PermissionErrors_ValidationFailed_ReturnsCorrectType()
    {
        var permission = PermissionName.From("test.permission");
        var error = AuthorizationErrors.ValidationFailed(permission, "field is required");
        error.Should().BeOfType<AuthorizationValidationError>();
    }

    [Fact]
    public void PermissionErrors_MissingAuthorizationStory_MessageContainsPermissionName()
    {
        var permission = PermissionName.From("documents.read");
        var error = AuthorizationErrors.MissingAuthorizationStory(permission);
        error.Message.Should().Contain("documents.read");
    }

    [Fact]
    public void PermissionErrors_PermissionDenied_MessageContainsPermissionName()
    {
        var permission = PermissionName.From("documents.write");
        var error = AuthorizationErrors.PermissionDenied(permission);
        error.Message.Should().Contain("documents.write");
    }
}
