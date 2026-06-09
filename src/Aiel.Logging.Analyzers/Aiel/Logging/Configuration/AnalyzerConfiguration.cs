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

// -----------------------------------------------------------------------
// AnalyzerConfiguration.cs
//
// 3-tier configuration resolution for the EventIds enum type:
//
//   Priority 1 — MSBuild CompilerVisibleProperty
//                Key:   build_property.AielEventIdsType
//                Set:   <AielEventIdsType>My.Namespace.MyEnum</AielEventIdsType>
//
//   Priority 2 — .editorconfig global key
//                Key:   aiel_event_ids_type
//                Value: My.Namespace.MyEnum
//
//   Priority 3 — Built-in default
//                Aiel.Logging.AielEventIds
//
// Usage in an analyzer:
//
//   context.RegisterCompilationStartAction(ctx =>
//   {
//       var config = AnalyzerConfiguration.Resolve(ctx.Options);
//       ctx.RegisterSyntaxNodeAction(nodeCtx =>
//       {
//           // ... analysis ...
//           var props = AnalyzerConfiguration.BuildDiagnosticProperties(config);
//           nodeCtx.ReportDiagnostic(Diagnostic.Create(descriptor,
//               location, properties: props, ...));
//       }, SyntaxKind.Attribute);
//   });
//
// Usage in a code fix:
//
//   var config = AnalyzerConfiguration.ReadFromDiagnostic(diagnostic);
//   // Use config.ShortName, config.FullTypeName, config.GetTypeSymbol(compilation)
// -----------------------------------------------------------------------

using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Logging.Configuration;

// ════════════════════════════════════════════════════════════════════════
// AnalyzerConfiguration  — static resolution + diagnostic helpers
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// Provides 3-tier resolution of the configured EventIds enum type and
/// helpers for stamping / reading that config in <see cref="Diagnostic"/>
/// property bags (so code fixes don't need to independently re-read options).
/// </summary>
public static class AnalyzerConfiguration
{
    // ── Public constants ─────────────────────────────────────────────────

    /// <summary>MSBuild CompilerVisibleProperty key read by the analyzer.</summary>
    public const String MsBuildPropertyKey = "AielEventIdsType";

    /// <summary>.editorconfig global option key.</summary>
    public const String EditorConfigKey = "aiel_event_ids_type";

    /// <summary>Fallback fully-qualified type name when no override is configured.</summary>
    public const String DefaultFullTypeName = "Aiel.Logging.AielEventIds";

    /// <summary>Fallback short type name when no override is configured.</summary>
    public const String DefaultShortName = "AielEventIds";

    /// <summary>Fallback member name when no matching member is found.</summary>
    public const String DefaultMemberName = "Replace_With_A_Valid_Member";

    /// <summary>Key used to stamp the full type name into <see cref="Diagnostic.Properties"/>.</summary>
    public const String DiagPropFullTypeName = "EventIdsFullTypeName";

    /// <summary>Key used to stamp the short type name into <see cref="Diagnostic.Properties"/>.</summary>
    public const String DiagPropShortName = "EventIdsShortName";

    // ── Resolution ────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the EventIds type configuration from <paramref name="options"/>
    /// using the 3-tier priority chain (MSBuild → .editorconfig → default).
    /// </summary>
    /// <param name="options">
    /// The <see cref="AnalyzerOptions"/> available inside
    /// <c>RegisterCompilationStartAction</c>.
    /// </param>
    public static EventIdsTypeConfig Resolve(AnalyzerOptions options)
    {
        var optionsProvider = options.AnalyzerConfigOptionsProvider;
        var globalOptions = optionsProvider.GlobalOptions;

        // Priority 1 — MSBuild CompilerVisibleProperty
        if ((!globalOptions.TryGetValue(MsBuildPropertyKey, out var msBuildValue)
             || String.IsNullOrWhiteSpace(msBuildValue))
            && !globalOptions.TryGetValue($"build_property.{MsBuildPropertyKey}", out msBuildValue))
        {
            msBuildValue = null;
        }

        if (msBuildValue is not null && !String.IsNullOrWhiteSpace(msBuildValue))
        {
            return EventIdsTypeConfig.FromFullTypeName(msBuildValue.Trim());
        }

        // Priority 2 — .editorconfig global key
        if ((!globalOptions.TryGetValue(EditorConfigKey, out var editorConfigValue)
             || String.IsNullOrWhiteSpace(editorConfigValue))
            && !globalOptions.TryGetValue($"build_property.{EditorConfigKey}", out editorConfigValue))
        {
            editorConfigValue = null;
        }

        if (editorConfigValue is not null && !String.IsNullOrWhiteSpace(editorConfigValue))
        {
            return EventIdsTypeConfig.FromFullTypeName(editorConfigValue.Trim());
        }

        // Priority 3 — Built-in default
        return EventIdsTypeConfig.FromFullTypeName(DefaultFullTypeName);
    }

    // ── Diagnostic property helpers ───────────────────────────────────────

    /// <summary>
    /// Builds an <see cref="ImmutableDictionary{TKey,TValue}"/> suitable for passing
    /// as the <c>properties</c> argument of
    /// <see cref="Diagnostic.Create(DiagnosticDescriptor,Location,ImmutableDictionary{String,String?},Object[])"/>.
    /// </summary>
    public static ImmutableDictionary<String, String?> BuildDiagnosticProperties(
        EventIdsTypeConfig config)
        => ImmutableDictionary<String, String?>.Empty
            .Add(DiagPropFullTypeName, config.FullTypeName)
            .Add(DiagPropShortName, config.ShortName);

    /// <summary>
    /// Reads the EventIds type config back from the property bag of a
    /// <see cref="Diagnostic"/> that was reported by one of the Aiel analyzers.
    /// Falls back to the default config if the properties are absent.
    /// </summary>
    /// <remarks>
    /// Code fixes should call this method instead of independently re-reading
    /// <see cref="AnalyzerConfigOptions"/> — the config is always available here
    /// regardless of the IDE's options-provider threading model.
    /// </remarks>
    public static EventIdsTypeConfig ReadFromDiagnostic(Diagnostic diagnostic)
    {
        if (diagnostic.Properties.TryGetValue(DiagPropFullTypeName, out var fullName) &&
            !String.IsNullOrWhiteSpace(fullName))
        {
            return EventIdsTypeConfig.FromFullTypeName(fullName!);
        }

        return EventIdsTypeConfig.FromFullTypeName(DefaultFullTypeName);
    }
}
