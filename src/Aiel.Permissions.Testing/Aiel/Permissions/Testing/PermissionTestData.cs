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

using Aiel.Actions;

namespace Aiel.Permissions.Testing;

/// <summary>
/// Provides well-known, non-default test values for use across permission test suites.
/// </summary>
/// <remarks>
/// All values are stable, valid, and non-default — safe to use without additional configuration.
/// Prefer these over ad-hoc test values to keep tests readable and consistent.
/// </remarks>
public static class PermissionTestData
{
    /// <summary>Gets a non-default grant identifier for test scenarios. Alpha variant.</summary>
    public static readonly PermissionGrantId GrantIdAlpha =
        PermissionGrantId.From(Guid.Parse("a1000000-0000-0000-0000-000000000001"));

    /// <summary>Gets a non-default grant identifier for test scenarios. Beta variant.</summary>
    public static readonly PermissionGrantId GrantIdBeta =
        PermissionGrantId.From(Guid.Parse("b2000000-0000-0000-0000-000000000002"));

    /// <summary>Gets a non-empty stable permission identifier for test scenarios. Alpha variant.</summary>
    public static readonly PermissionStableId StableIdAlpha =
        PermissionStableId.From("perm.testing.alpha");

    /// <summary>Gets a non-empty stable permission identifier for test scenarios. Beta variant.</summary>
    public static readonly PermissionStableId StableIdBeta =
        PermissionStableId.From("perm.testing.beta");

    /// <summary>Gets a non-empty stable permission identifier for rename migration scenarios.</summary>
    public static readonly PermissionStableId StableIdAppointment =
        PermissionStableId.From("perm.testing.appointment");

    /// <summary>Gets a non-default appointment identifier for reference-slice scenarios. Alpha variant.</summary>
    public static readonly Guid AppointmentIdAlpha =
        Guid.Parse("c2e9c208-6350-4d73-bcd6-a1dc525a4781");

    /// <summary>Gets a non-default appointment identifier for reference-slice scenarios. Beta variant.</summary>
    public static readonly Guid AppointmentIdBeta =
        Guid.Parse("f615ef70-1223-4fc5-8d63-dcdac2139b6f");

    /// <summary>Gets a valid <c>testing.read</c> permission name for use in test scenarios.</summary>
    public static readonly PermissionName PermissionNameRead =
        PermissionName.From("testing.read");

    /// <summary>Gets a valid <c>testing.write</c> permission name for use in test scenarios.</summary>
    public static readonly PermissionName PermissionNameWrite =
        PermissionName.From("testing.write");

    /// <summary>Gets a valid <c>testing.ChangeAppointment</c> permission name for rename migration scenarios.</summary>
    public static readonly PermissionName PermissionNameChangeAppointment =
        PermissionName.From("testing.ChangeAppointment");

    /// <summary>Gets a valid <c>testing.RescheduleAppointment</c> permission name for rename migration scenarios.</summary>
    public static readonly PermissionName PermissionNameRescheduleAppointment =
        PermissionName.From("testing.RescheduleAppointment");

    /// <summary>Gets a sample scope type name for use in test scenarios.</summary>
    public static readonly PermissionScopeTypeName ScopeTypeAlpha =
        PermissionScopeTypeName.From("TestScope");

    /// <summary>Gets a sample scope key for use in test scenarios. Alpha variant.</summary>
    public static readonly PermissionScopeKey ScopeKeyAlpha =
        PermissionScopeKey.From("scope-alpha");

    /// <summary>Gets a sample scope key for use in test scenarios. Beta variant.</summary>
    public static readonly PermissionScopeKey ScopeKeyBeta =
        PermissionScopeKey.From("scope-beta");

    /// <summary>Gets a resource scope key for appointment reference-slice scenarios. Alpha variant.</summary>
    public static readonly PermissionScopeKey AppointmentResourceScopeKeyAlpha =
        PermissionScopeKey.From("appointment-resource-alpha");

    /// <summary>Gets a resource scope key for appointment reference-slice scenarios. Beta variant.</summary>
    public static readonly PermissionScopeKey AppointmentResourceScopeKeyBeta =
        PermissionScopeKey.From("appointment-resource-beta");

    /// <summary>Gets a sample subject type name for use in test scenarios.</summary>
    public static readonly PermissionSubjectTypeName SubjectTypeAlpha =
        PermissionSubjectTypeName.From("TestSubject");

    /// <summary>Gets a sample subject key for use in test scenarios. Alpha variant.</summary>
    public static readonly PermissionSubjectKey SubjectKeyAlpha =
        PermissionSubjectKey.From("subject-alpha");

    /// <summary>Gets a sample subject key for use in test scenarios. Beta variant.</summary>
    public static readonly PermissionSubjectKey SubjectKeyBeta =
        PermissionSubjectKey.From("subject-beta");

    /// <summary>
    /// Creates a <see cref="PermissionDefinitionManifest"/> with well-known test values for the
    /// specified <typeparamref name="TAction"/>.
    /// </summary>
    /// <typeparam name="TAction">The action type the manifest governs.</typeparam>
    public static PermissionDefinitionManifest CreateSampleManifest<TAction>()
        where TAction : IAction
    {
        return new PermissionDefinitionManifest
        {
            StableId = StableIdAlpha,
            PermissionName = PermissionNameRead,
            ActionType = typeof(TAction),
            ScopeType = ScopeTypeAlpha,
            SubjectType = SubjectTypeAlpha,
            DisplayName = "Test permission",
            Description = "A test-only permission definition.",
            Lifecycle = PermissionLifecycle.Active,
            PreviousNames = []
        };
    }

    /// <summary>
    /// Creates a <see cref="PermissionDefinitionManifest"/> for <see cref="Fixtures.AlphaTestAction"/>
    /// with well-known test values.
    /// </summary>
    public static PermissionDefinitionManifest CreateSampleManifest()
        => CreateSampleManifest<Fixtures.AlphaTestAction>();

    /// <summary>
    /// Creates a renamed permission manifest for <see cref="Fixtures.RescheduleAppointmentTestAction"/>.
    /// </summary>
    public static PermissionDefinitionManifest CreateRescheduleAppointmentManifest()
        => new()
        {
            StableId = StableIdAppointment,
            PermissionName = PermissionNameRescheduleAppointment,
            ActionType = typeof(Fixtures.RescheduleAppointmentTestAction),
            ScopeType = ScopeTypeAlpha,
            SubjectType = SubjectTypeAlpha,
            DisplayName = "Reschedule appointment",
            Description = "A renamed permission definition used by integration tests.",
            Lifecycle = PermissionLifecycle.Active,
            PreviousNames = [PermissionNameChangeAppointment]
        };
}
