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

using Aiel.Logging.Configuration;
using Aiel.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers;

// ═══════════════════════════════════════════════════════════════════════
// Unit tests for AnalyzerConfiguration itself
// ═══════════════════════════════════════════════════════════════════════

public sealed class AnalyzerConfigurationUnitTests
{
    [Fact]
    public void FromFullTypeName_SplitsCorrectly()
    {
        var cfg = EventIdsTypeConfig.FromFullTypeName("Acme.Logging.AcmeEventIds");
        cfg.FullTypeName.Should().Be("Acme.Logging.AcmeEventIds");
        cfg.ShortName.Should().Be("AcmeEventIds");
    }

    [Fact]
    public void FromFullTypeName_NoNamespace_ShortNameEqualsFullName()
    {
        var cfg = EventIdsTypeConfig.FromFullTypeName("MyEventIds");
        cfg.FullTypeName.Should().Be("MyEventIds");
        cfg.ShortName.Should().Be("MyEventIds");
    }

    [Fact]
    public void DefaultConstants_AreCorrect()
    {
        AnalyzerConfiguration.DefaultFullTypeName.Should().Be("Aiel.Logging.AielEventIds");
        AnalyzerConfiguration.DefaultShortName.Should().Be("AielEventIds");
    }

    [Fact]
    public void MsBuildPropertyKey_MatchesExpectedFormat()
    {
        AnalyzerConfiguration.MsBuildPropertyKey.Should().Be("AielEventIdsType");
    }

    [Fact]
    public void EditorConfigKey_MatchesExpectedFormat()
    {
        AnalyzerConfiguration.EditorConfigKey.Should().Be("aiel_event_ids_type");
    }

    [Fact]
    public void BuildDiagnosticProperties_ContainsBothKeys()
    {
        var cfg = EventIdsTypeConfig.FromFullTypeName("Acme.Logging.AcmeEventIds");
        var props = AnalyzerConfiguration.BuildDiagnosticProperties(cfg);

        props.Should().ContainKey(AnalyzerConfiguration.DiagPropFullTypeName);
        props.Should().ContainKey(AnalyzerConfiguration.DiagPropShortName);
        props[AnalyzerConfiguration.DiagPropFullTypeName].Should().Be("Acme.Logging.AcmeEventIds");
        props[AnalyzerConfiguration.DiagPropShortName].Should().Be("AcmeEventIds");
    }

    [Fact]
    public void ReadFromDiagnostic_WithProperties_ReturnsCorrectConfig()
    {
        // Build a fake diagnostic with the expected properties.
        var props = ImmutableDictionary<String, String?>.Empty
            .Add(AnalyzerConfiguration.DiagPropFullTypeName, "Acme.Logging.AcmeEventIds")
            .Add(AnalyzerConfiguration.DiagPropShortName, "AcmeEventIds");

        var diag = Diagnostic.Create(
            DiagnosticDescriptors.UseAielEventIds,
            Location.None,
            properties: props,
            messageArgs: ["AcmeEventIds", "Acme.Logging.AcmeEventIds"]);

        var cfg = AnalyzerConfiguration.ReadFromDiagnostic(diag);
        cfg.FullTypeName.Should().Be("Acme.Logging.AcmeEventIds");
        cfg.ShortName.Should().Be("AcmeEventIds");
    }
}
