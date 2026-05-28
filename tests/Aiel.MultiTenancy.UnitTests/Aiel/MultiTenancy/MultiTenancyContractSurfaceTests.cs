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

using Aiel.StrongIds;
using System.Reflection;

namespace Aiel.MultiTenancy;

public sealed class MultiTenancyContractSurfaceTests
{
    private static readonly NullabilityInfoContext NullabilityInfoContext = new();
    private static readonly Assembly MultiTenancyAssembly = typeof(IMultiTenant).Assembly;

    [Fact]
    public void PublicSurface_UsesApprovedTenantVocabulary()
    {
        var exportedTypeNames = MultiTenancyAssembly
            .GetExportedTypes()
            .Select(static type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        exportedTypeNames.Should().Contain("TenantId");
        exportedTypeNames.Should().Contain("TenantIdentity");
        exportedTypeNames.Should().Contain("TenantResolution");
        exportedTypeNames.Should().Contain("ITenantAccessor");
        exportedTypeNames.Should().Contain("ITenantResolver");
        exportedTypeNames.Should().NotContain("TenantContext");
        exportedTypeNames.Should().NotContain("ITenantProvider");
    }

    [Fact]
    public void TenantId_IsGuidBackedStrongIdentifierOnly()
    {
        var tenantIdType = GetRequiredPublicType("Aiel.MultiTenancy.TenantId");
        var publicPropertyNames = tenantIdType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(static property => property.Name)
            .ToArray();

        typeof(IStrongId<Guid>).IsAssignableFrom(tenantIdType).Should().BeTrue();
        publicPropertyNames.Should().Contain(nameof(IStrongId<>.Value));
        publicPropertyNames.Should().NotContain(name => name.Contains("Domain", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Host", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Display", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Storage", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Connection", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TenantIdentity_ReplacesTenantContext_AndKeepsPublicHintsNonNullable()
    {
        var tenantIdType = GetRequiredPublicType("Aiel.MultiTenancy.TenantId");
        var tenantIdentityType = GetRequiredPublicType("Aiel.MultiTenancy.TenantIdentity");
        var tenantIdProperty = tenantIdentityType.GetProperty("TenantId", BindingFlags.Public | BindingFlags.Instance);

        tenantIdProperty.Should().NotBeNull();
        tenantIdProperty!.PropertyType.Should().Be(tenantIdType);

        foreach (var property in tenantIdentityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(static property => property.PropertyType == typeof(String)))
        {
            NullabilityInfoContext.Create(property).ReadState.Should().NotBe(
                NullabilityState.Nullable,
                because: "public routing or display hints must not be nullable");
        }

        var publicPropertyNames = tenantIdentityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(static property => property.Name)
            .ToArray();

        publicPropertyNames.Should().NotContain(name => name.Contains("Storage", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Connection", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Membership", StringComparison.OrdinalIgnoreCase));
        publicPropertyNames.Should().NotContain(name => name.Contains("Catalog", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TenantResolution_ExposesExplicitClosedOutcomes_WithTypedReasonCodes()
    {
        var tenantIdentityType = GetRequiredPublicType("Aiel.MultiTenancy.TenantIdentity");
        var tenantResolutionType = GetRequiredPublicType("Aiel.MultiTenancy.TenantResolution");
        var publicOutcomeTypes = tenantResolutionType.GetNestedTypes(BindingFlags.Public);
        var publicOutcomeNames = publicOutcomeTypes.Select(static type => type.Name).ToArray();

        publicOutcomeNames.Should().Contain("Resolved");
        publicOutcomeNames.Should().Contain("Missing");
        publicOutcomeNames.Should().Contain("Ambiguous");
        publicOutcomeNames.Should().Contain("Rejected");
        publicOutcomeNames.Should().Contain("Error");
        publicOutcomeNames.Should().NotContain("None");

        var resolvedType = tenantResolutionType.GetNestedType("Resolved", BindingFlags.Public);
        resolvedType.Should().NotBeNull();
        resolvedType!
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Should()
            .Contain(property => property.PropertyType == tenantIdentityType);

        AssertHasTypedReasonMember(tenantResolutionType, "Rejected");
        AssertHasTypedReasonMember(tenantResolutionType, "Error");
    }

    [Fact]
    public void TenantResolutionErrorReason_StaysWithinControlPlaneResolutionScope()
    {
        var reasonNames = Enum.GetNames(GetRequiredPublicType("Aiel.MultiTenancy.TenantResolutionErrorReason"));

        reasonNames.Should().Contain(nameof(TenantResolutionErrorReason.MembershipLookupFailed));
        reasonNames.Should().Contain(nameof(TenantResolutionErrorReason.UnexpectedException));
        reasonNames.Should().NotContain("StoreUnavailable");
    }

    [Fact]
    public void AccessorAndResolverContracts_DoNotExposeNullableTenantResults()
    {
        var tenantIdentityType = GetRequiredPublicType("Aiel.MultiTenancy.TenantIdentity");
        var tenantResolutionType = GetRequiredPublicType("Aiel.MultiTenancy.TenantResolution");
        var tenantAccessorType = GetRequiredPublicType("Aiel.MultiTenancy.ITenantAccessor");
        var tenantResolverType = GetRequiredPublicType("Aiel.MultiTenancy.ITenantResolver");

        HasNonNullableReturnOf(tenantAccessorType, tenantIdentityType).Should().BeTrue();
        HasNonNullableReturnOf(tenantResolverType, tenantResolutionType).Should().BeTrue();
        HasNullableReturnOf(tenantAccessorType, tenantIdentityType).Should().BeFalse();
        HasNullableReturnOf(tenantResolverType, tenantIdentityType).Should().BeFalse();
        HasNullableReturnOf(tenantResolverType, tenantResolutionType).Should().BeFalse();
    }

    [Fact]
    public void IMultiTenant_UsesTenantIdRatherThanGuid()
    {
        var tenantIdType = GetRequiredPublicType("Aiel.MultiTenancy.TenantId");
        var tenantIdProperty = typeof(IMultiTenant).GetProperty(nameof(IMultiTenant.TenantId), BindingFlags.Public | BindingFlags.Instance);

        tenantIdProperty.Should().NotBeNull();
        tenantIdProperty!.PropertyType.Should().Be(tenantIdType);
    }

    [Fact]
    public void TenantResolutionConstants_UseApprovedTrustBoundaryValues()
    {
        var stringConstants = typeof(TenantResolutionConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(static field => field.IsLiteral && field.FieldType == typeof(String))
            .ToDictionary(static field => field.Name, static field => (String)field.GetRawConstantValue()!, StringComparer.Ordinal);

        stringConstants.Values.Should().Contain("sub");
        stringConstants.Values.Should().Contain("X-Tenant-ID");
        stringConstants.Values.Should().NotContain("tenantId");
        stringConstants.Values.Should().NotContain("X-Tenant-Domain");
        stringConstants.Keys.Should().NotContain("TenantIdClaimType");
        stringConstants.Keys.Should().NotContain("TenantDomainHeaderName");
    }

    private static void AssertHasTypedReasonMember(Type tenantResolutionType, String outcomeName)
    {
        var outcomeType = tenantResolutionType.GetNestedType(outcomeName, BindingFlags.Public);
        outcomeType.Should().NotBeNull();

        var reasonProperties = outcomeType!
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static property => property.Name.Contains("Reason", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        reasonProperties.Should().Contain(
            property =>
                property.PropertyType != typeof(String)
                && NullabilityInfoContext.Create(property).ReadState != NullabilityState.Nullable,
            because: $"{outcomeName} must expose a non-null typed reason code");
    }

    private static Type GetRequiredPublicType(String fullName)
    {
        var type = MultiTenancyAssembly.GetType(fullName, throwOnError: false, ignoreCase: false);

        type.Should().NotBeNull($"the public type {fullName} is part of the approved vocabulary");
        return type!;
    }

    private static Boolean HasNonNullableReturnOf(Type contractType, Type expectedType)
    {
        return GetReadableContractMembers(contractType)
            .Any(member => ReturnsType(member, expectedType) && !IsNullableReturn(member));
    }

    private static Boolean HasNullableReturnOf(Type contractType, Type expectedType)
    {
        return GetReadableContractMembers(contractType)
            .Any(member => ReturnsType(member, expectedType) && IsNullableReturn(member));
    }

    private static IEnumerable<MemberInfo> GetReadableContractMembers(Type contractType)
    {
        foreach (var property in contractType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            yield return property;
        }

        foreach (var method in contractType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                     .Where(static method => !method.IsSpecialName))
        {
            yield return method;
        }
    }

    private static Boolean ReturnsType(MemberInfo member, Type expectedType)
    {
        return member switch
        {
            PropertyInfo property => UnwrapAwaitable(property.PropertyType) == expectedType,
            MethodInfo method => UnwrapAwaitable(method.ReturnType) == expectedType,
            _ => false
        };
    }

    private static Boolean IsNullableReturn(MemberInfo member)
    {
        return member switch
        {
            PropertyInfo property => NullabilityInfoContext.Create(property).ReadState == NullabilityState.Nullable,
            MethodInfo method => NullabilityInfoContext.Create(method.ReturnParameter).ReadState == NullabilityState.Nullable,
            _ => false
        };
    }

    private static Type UnwrapAwaitable(Type returnType)
    {
        if (returnType.IsGenericType)
        {
            var genericTypeDefinition = returnType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Task<>) || genericTypeDefinition == typeof(ValueTask<>))
            {
                return returnType.GetGenericArguments()[0];
            }
        }

        return returnType;
    }
}
