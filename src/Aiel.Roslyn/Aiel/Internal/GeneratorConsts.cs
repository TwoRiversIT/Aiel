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

using System;

namespace Aiel.Internal;

/// <summary>
/// Constants shared between the runtime library and code generators.
/// </summary>
internal static class GeneratorConsts
{
    public const String Company = "Aiel";
    public const String Resolver = "JsonTypeInfoResolver";
    public const String MessageParameter = "message";
    public const String Error = "Error";
    public const String ErrorCode = Error + Suffix;
    public const String ErrorCodeFilename = Root + "_GeneratedErrors" + Extension;
    public const String Extension = ".g.cs";
    public const String FqErrorCodeType = Global + Root + "." + ErrorCode;
    public const String FqErrorType = Global + Root + ".Error";
    public const String FqPolymorphismType = Global + Root + "." + Polymorphism + Initializer;
    public const String FqRegistryType = Global + Root + "." + Registry;
    public const String GeneratedPolymorphism = "_" + Polymorphism + Initializer;
    public const String Global = "global::";
    public const String Initializer = "Initializer";
    public const String Instance = "Instance";
    public const String JsonTypeInfoResolverFilename = Root + "_" + Resolver + Extension;
    public const String Polymorphism = "Polymorphism";
    public const String PolymorphismIntitializer = Polymorphism + Initializer;
    public const String PolymorphismFilename = Root + "_" + Polymorphism + Extension;
    public const String RegisterMethod = "Register";
    public const String Registry = "ErrorRegistry";
    public const String ResultPatternServiceCollectionExtensions = Root + "ServiceCollectionExtensions";
    public const String Results = "Results";
    public const String Root = Company + "." + Results;
    public const String ServiceCollectionExtensionMethod = "Add" + Root;
    public const String Suffix = "Code";
    public const String TypeDiscriminatorPropertyName = "TypeDiscriminatorPropertyName";
}
