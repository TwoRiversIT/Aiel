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

namespace Aiel.Results.Generators;

public class ErrorInf(String ns, String error, String errorCode)
{
    public static ErrorInf FromSymbol(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        var errorName = symbol.Name;
        var codeName = errorName + GeneratorConsts.Suffix;

        return new ErrorInf(ns ?? "", errorName, codeName);
    }

    public String Namespace { get; } = ns;
    public String ErrorName { get; } = error;
    public String FqErrorName => HasNamespace ? $"{GeneratorConsts.Global}{Namespace}.{ErrorName}" : $"{GeneratorConsts.Global}{ErrorName}";
    public String ErrorCodeName { get; } = errorCode;
    public String ErrorAndCodeName => $"{ErrorName}.{ErrorCodeName}";
    public String FqErrorCodeName => HasNamespace ? $"{GeneratorConsts.Global}{Namespace}.{ErrorName}.{ErrorCodeName}" : $"{GeneratorConsts.Global}{ErrorName}.{ErrorCodeName}";

    public String FqInstance => HasNamespace
        ? $"{GeneratorConsts.Global}{Namespace}.{ErrorName}.{ErrorCodeName}.{GeneratorConsts.Instance}"
        : $"{GeneratorConsts.Global}{ErrorName}.{ErrorCodeName}.{GeneratorConsts.Instance}";

    public Boolean HasNamespace => !String.IsNullOrEmpty(Namespace);

    public String TypeDiscriminator => $"{Namespace}.{ErrorName}.{ErrorCodeName}:v1";
}
