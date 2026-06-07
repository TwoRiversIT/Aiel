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

using Aiel.Internal;
using Microsoft.CodeAnalysis;

namespace Aiel.Analyzers.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00001 is raised when an assembly referencing Aiel does not declare exactly one public,
    /// non-abstract <see cref="Dependencies.AielDependencyConfigurator"/> subclass or 
    /// <see cref="Dependencies.AielApplication"/> subclass with a
    /// public parameterless constructor. This type serves as the logical root for the
    /// assembly's participation in application configuration, initialization, and
    /// dependency resolution.
    /// </summary>
    public static readonly DiagnosticDescriptor RootDependencyRequired = new(
        id: DiagnosticRuleIDs.AIEL00001_RootDependencyRequiredId,
        title: "Assemblies referencing the `Aiel` NuGet package directly or transitively must declare their dependency type, either `AielDependencyConfigurator` or `AielApplication`",
        messageFormat: "The '{0}' assembly must declare exactly one public sealed class with a public parameterless constructor that inherits `AielApplication` or `AielDependencyConfigurator`",
        category: DiagnosticMetadata.UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Any assembly that references Aiel directly or transitively must define exactly one public sealed class with a public parameterless constructor, inheriting from either `Aiel.Dependencies.AielDependencyConfigurator` or `Aiel.Dependencies.AielApplication`. These types serve as the root for the dependency graph.",
        customTags: []);

}
