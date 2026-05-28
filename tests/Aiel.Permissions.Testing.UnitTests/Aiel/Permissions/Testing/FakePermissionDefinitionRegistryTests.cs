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

namespace Aiel.Permissions.Testing;

public sealed class FakePermissionDefinitionRegistryTests
{
    [Fact]
    public void Empty_GetAll_ReturnsEmptyList_NotNull()
    {
        var registry = new FakePermissionDefinitionRegistry();
        registry.GetAll().Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void WithManifests_GetAll_ReturnsRegisteredManifests()
    {
        var manifest = PermissionTestData.CreateSampleManifest();
        var registry = new FakePermissionDefinitionRegistry([manifest]);

        registry.GetAll().Should().ContainSingle();
    }

    [Fact]
    public void TryGet_ForRegisteredName_ReturnsTrueAndManifest()
    {
        var manifest = PermissionTestData.CreateSampleManifest();
        var registry = new FakePermissionDefinitionRegistry([manifest]);

        var found = registry.TryGet(manifest.PermissionName, out var result);

        found.Should().BeTrue();
        result.Should().Be(manifest);
    }

    [Fact]
    public void TryGet_ForUnregisteredName_ReturnsFalse()
    {
        var registry = new FakePermissionDefinitionRegistry();

        var found = registry.TryGet(PermissionTestData.PermissionNameRead, out _);

        found.Should().BeFalse();
    }

    [Fact]
    public void TryGet_ForDifferentName_ReturnsFalse()
    {
        var manifest = PermissionTestData.CreateSampleManifest(); // uses PermissionNameRead
        var registry = new FakePermissionDefinitionRegistry([manifest]);

        var found = registry.TryGet(PermissionTestData.PermissionNameWrite, out _);

        found.Should().BeFalse();
    }

    [Fact]
    public void TryGetForAction_ForRegisteredAction_ReturnsTrueAndManifest()
    {
        var manifest = PermissionTestData.CreateSampleManifest<Fixtures.AlphaTestAction>();
        var registry = new FakePermissionDefinitionRegistry([manifest]);

        var found = registry.TryGetForAction<Fixtures.AlphaTestAction>(out var result);

        found.Should().BeTrue();
        result.Should().Be(manifest);
    }

    [Fact]
    public void TryGetForAction_ForUnregisteredAction_ReturnsFalse()
    {
        var registry = new FakePermissionDefinitionRegistry();

        var found = registry.TryGetForAction<Fixtures.AlphaTestAction>(out _);

        found.Should().BeFalse();
    }

    [Fact]
    public void TryGetForAction_ForDifferentAction_ReturnsFalse()
    {
        var manifest = PermissionTestData.CreateSampleManifest<Fixtures.AlphaTestAction>();
        var registry = new FakePermissionDefinitionRegistry([manifest]);

        var found = registry.TryGetForAction<Fixtures.BetaTestAction>(out _);

        found.Should().BeFalse();
    }

    [Fact]
    public void WithMultipleManifests_TryGet_ReturnsCorrectManifest()
    {
        var readManifest = PermissionTestData.CreateSampleManifest<Fixtures.AlphaTestAction>();
        var writeManifest = new PermissionDefinitionManifest
        {
            StableId = PermissionTestData.StableIdBeta,
            PermissionName = PermissionTestData.PermissionNameWrite,
            ActionType = typeof(Fixtures.BetaTestAction),
            ScopeType = PermissionTestData.ScopeTypeAlpha,
            SubjectType = PermissionTestData.SubjectTypeAlpha,
            DisplayName = "Test write permission"
        };

        var registry = new FakePermissionDefinitionRegistry([readManifest, writeManifest]);

        var found = registry.TryGet(PermissionTestData.PermissionNameWrite, out var result);

        found.Should().BeTrue();
        result.Should().Be(writeManifest);
    }
}
