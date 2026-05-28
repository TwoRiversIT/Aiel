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

namespace Aiel.Permissions;

public sealed class PermissionGrantTests
{
    [Fact]
    public void Create_RejectsDefaultIds_EmptyScope_EmptySubject_AndInvalidPermissionName()
    {
        var result = PermissionGrant.Create(
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            PermissionGrantDecision.Granted);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidPermissionGrantError>();
    }

    [Fact]
    public void Create_FromCatalogEntry_RejectsRemovedCatalogEntries()
    {
        var catalogEntry = CreateCatalogEntry(PermissionLifecycle.Removed);

        var result = PermissionGrant.Create(
            PermissionGrantId.From(Guid.Parse("8b2f8d89-6a95-47cd-8eec-c6f07a145ee5")),
            catalogEntry,
            PermissionScopeKey.From("clinic:west"),
            PermissionSubjectTypeName.From("User"),
            PermissionSubjectKey.From("user:42"),
            PermissionGrantDecision.Granted);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidPermissionGrantError>();
    }

    [Fact]
    public void Create_FromCatalogEntry_CapturesPermissionIdentity_AndMatchesScopeAndSubject()
    {
        var catalogEntry = CreateCatalogEntry();
        var scopeKey = PermissionScopeKey.From("clinic:west");
        var subjectType = PermissionSubjectTypeName.From("User");
        var subjectKey = PermissionSubjectKey.From("user:42");

        var result = PermissionGrant.Create(
            PermissionGrantId.From(Guid.Parse("549d88cb-7114-4b7b-8690-c5c53ab0a724")),
            catalogEntry,
            scopeKey,
            subjectType,
            subjectKey,
            PermissionGrantDecision.Prohibited);

        result.IsSuccess.Should().BeTrue();
        result.Value.PermissionStableId.Should().Be(catalogEntry.Id);
        result.Value.PermissionName.Should().Be(catalogEntry.PermissionName);
        result.Value.ScopeType.Should().Be(catalogEntry.ScopeType);
        result.Value.ScopeKey.Should().Be(scopeKey);
        result.Value.SubjectType.Should().Be(subjectType);
        result.Value.SubjectKey.Should().Be(subjectKey);
        result.Value.Decision.Should().Be(PermissionGrantDecision.Prohibited);
        result.Value.MatchesScope(PermissionScopeTypeName.From("Clinic"), scopeKey).Should().BeTrue();
        result.Value.MatchesScope(PermissionScopeTypeName.From("Tenant"), scopeKey).Should().BeFalse();
        result.Value.MatchesSubject(subjectType, subjectKey).Should().BeTrue();
        result.Value.MatchesSubject(subjectType, PermissionSubjectKey.From("user:100")).Should().BeFalse();
    }

    private static PermissionCatalogEntry CreateCatalogEntry(PermissionLifecycle lifecycle = PermissionLifecycle.Active)
    {
        var result = PermissionCatalogEntry.Create(
            PermissionStableId.From("perm_01k0task4grant0000000000000001"),
            PermissionName.From("Aviendha.Scheduling.Appointments.ChangeAppointment"),
            PermissionScopeTypeName.From("Clinic"),
            lifecycle);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
