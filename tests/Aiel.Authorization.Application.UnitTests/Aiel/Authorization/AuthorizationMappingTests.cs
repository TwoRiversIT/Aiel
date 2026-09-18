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

using Aiel.Testing;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Authorization;

public class AuthorizationMappingTests(ConfiguratorTestFixture<AielAuthorizationApplicationUnitTests> fixture, ITestOutputHelper output)
    : ConfiguratorTestBase<AielAuthorizationApplicationUnitTests, ConfiguratorTestFixture<AielAuthorizationApplicationUnitTests>>(fixture, output)
{
    [Fact]
    public void Mapster_Works_with_DependencyInjection()
    {
        // Arrange
        var id = AuthorizationGrantId.From(Guid.NewGuid());

        var grant = AuthorizationGrant.Create(
            id,
            PermissionStableId.From("perm_01k0task5manager000000000001"),
            PermissionName.From("documents.read"),
            AuthorizationScopeTypeName.From("Tenant"),
            AuthorizationScopeKey.From("t-1"),
            AuthorizationSubjectTypeName.From("User"),
            AuthorizationSubjectKey.From("u-1"),
            AuthorizationGrantDecision.Granted).Value;
        var mapper = Services.GetRequiredService<IMapper>();

        // Act
        var dto = mapper.Map<AuthorizationGrantDto>(grant);

        // Assert
        dto.Should().NotBeNull();
        dto.GrantId.Should().Be(grant.Id);
        dto.PermissionName.Should().Be(grant.PermissionName);
        dto.ScopeType.Should().Be(grant.ScopeType);
        dto.ScopeKey.Should().Be(grant.ScopeKey);
        dto.SubjectType.Should().Be(grant.SubjectType);
        dto.SubjectKey.Should().Be(grant.SubjectKey);
        dto.Decision.Should().Be(grant.Decision);
    }
}
