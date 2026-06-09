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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Verifiers;

// ═══════════════════════════════════════════════════════════════════════
// Helper: test base that injects a custom AnalyzerConfigOptions
// ═══════════════════════════════════════════════════════════════════════

internal static class Helpers
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(compilation.GlobalNamespace);

        while (stack.Count > 0)
        {
            var ns = stack.Pop();
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol childNs)
                {
                    stack.Push(childNs);
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                }
            }
        }
    }
}

/// <summary>
/// Minimal <see cref="AnalyzerConfigOptions"/> that returns a single key/value pair.
/// Used to inject <c>build_property.AielEventIdsType</c> into analyzer tests.
/// </summary>
internal sealed class SingleKeyAnalyzerConfigOptions(String key, String value) : AnalyzerConfigOptions
{
    private readonly String _key = key;
    private readonly String _value = value;

    public override Boolean TryGetValue(String key, out String value)
    {
        if (key == _key)
        {
            value = _value;
            return true;
        }

        value = String.Empty;
        return false;
    }
}

/// <summary>
/// <see cref="AnalyzerConfigOptionsProvider"/> backed by a single key/value pair.
/// </summary>
internal sealed class SingleKeyOptionsProvider(String key, String value) : AnalyzerConfigOptionsProvider
{
    private readonly SingleKeyAnalyzerConfigOptions _options = new(key, value);

    public override AnalyzerConfigOptions GlobalOptions => _options;
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _options;
    public override AnalyzerConfigOptions GetOptions(AdditionalText text) => _options;
}
