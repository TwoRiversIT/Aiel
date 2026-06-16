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

using Aiel.Testing.CodeAnalysis;
using Driver = Aiel.Testing.CodeAnalysis.GenerateCS<Aiel.Results.Generators.PolymorphismHookGenerator>;

namespace Aiel.Results;

public class PolymorphismHookGeneratorTests
{
    // Not implementing the other variations of this test because the PolymorphismHookGenerator uses the
    // same IsCandidate(SyntaxNode node) method as the ErrorClassGenerator, which already has tests for
    // all of the other variations.
    [Fact]
    public async Task Internal_Sealed_Partial_Class_Should_Generate_PolymorphismHook()
    {
        const String testCode = """
            using Aiel.Results; 

            namespace TestNamespace;

            internal sealed partial class CustomError : Error;
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratedSources.Should().HaveCount(1);
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("internal static class _PolymorphismInitializer")
            .And.Contain("new global::TestNamespace.CustomError(NotEmpty)")
            .And.Contain("new JsonDerivedType(typeof(global::TestNamespace.CustomError.CustomErrorCode)")
            .And.Contain("\"TestNamespace.CustomError.CustomErrorCode:v1\"");

        result.GeneratorDiagnostics.Should().BeEmpty();
        result.CompilationDiagnostics.Should().AllSatisfy(diagnostic => diagnostic.Id.Should().BeOneOf("CS7036", "CS1729", "CS0426"));
        // Expected because the ErrorClassGenerator has not generated the CustomError class, CustomErrorCode class, or the constructor that takes an ErrorCode and string.
        // CS7036: There is no argument given that corresponds to the required parameter 'errorCode' of 'Error.Error(ErrorCode, string)'
        // CS1729: 'CustomError' does not contain a constructor that takes 1 arguments,
        // CS0426: The type name 'CustomErrorCode' does not exist in the type 'CustomError'
    }

    [Fact]
    public void Multiple_Definitions_Should_Generate_Multiple_Polymorphism_Hooks()
    {
        const String testCode = """
            using Aiel.Results; 
            
            namespace TestNamespace;
            
            public sealed partial class AlphaError : Error;
            public sealed partial class BravoError : Error;
            public sealed partial class CharlieError : Error;
            """;

        var result = Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratedSources.Should().HaveCount(1);
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("internal static class _PolymorphismInitializer")
            .And.Contain("new global::TestNamespace.AlphaError(NotEmpty)")
            .And.Contain("new global::TestNamespace.BravoError(NotEmpty)")
            .And.Contain("new global::TestNamespace.CharlieError(NotEmpty)")
            .And.Contain("new JsonDerivedType(typeof(global::TestNamespace.AlphaError.AlphaErrorCode)")
            .And.Contain("\"TestNamespace.AlphaError.AlphaErrorCode:v1\"")
            .And.Contain("new JsonDerivedType(typeof(global::TestNamespace.AlphaError.AlphaErrorCode)")
            .And.Contain("\"TestNamespace.AlphaError.AlphaErrorCode:v1\"")
            .And.Contain("new JsonDerivedType(typeof(global::TestNamespace.BravoError.BravoErrorCode)")
            .And.Contain("\"TestNamespace.BravoError.BravoErrorCode:v1\"")
            .And.Contain("new JsonDerivedType(typeof(global::TestNamespace.CharlieError.CharlieErrorCode)")
            .And.Contain("\"TestNamespace.CharlieError.CharlieErrorCode:v1\"");

        result.CompilationDiagnostics.Should()
            .AllSatisfy(diagnostic => diagnostic.Id.Should().BeOneOf("CS7036", "CS1729", "CS0426", "CS8019"));
        // Expected because the ErrorClassGenerator has not generated the AlphaError class, AlphaErrorCode class, or the constructor that takes an ErrorCode and string.
        // CS7036: There is no argument given that corresponds to the required parameter 'errorCode' of 'Error.Error(ErrorCode, string)'
        // CS1729: 'AlphaError' does not contain a constructor that takes 1 arguments,
        // CS0426: The type name 'AlphaErrorCode' does not exist in the type 'AlphaError'
        // CS8019: Unnecessary using directive.
    }

    [Fact]
    public void Public_Sealed_Partial_Class_Should_Generate_Polymorphism_Hook()
    {
        const String testCode = """
            using Aiel.Results;
            using System;

            namespace TestNamespace;
            
            public sealed partial class CustomError : Error;
            """;

        var result = Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratedSources.Should().HaveCount(1);
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("internal static class _PolymorphismInitializer")
            .And.Contain("new global::TestNamespace.CustomError(NotEmpty)")
            .And.Contain("new JsonDerivedType(typeof(global::TestNamespace.CustomError.CustomErrorCode)")
            .And.Contain("\"TestNamespace.CustomError.CustomErrorCode:v1\"");

        result.CompilationDiagnostics.Should().AllSatisfy(diagnostic => diagnostic.Id.Should().BeOneOf("CS7036", "CS1729", "CS0426", "CS8019"));
        // Expected because the ErrorClassGenerator has not generated the CustomError class, CustomErrorCode class, or the constructor that takes an ErrorCode and string.
        // CS7036: There is no argument given that corresponds to the required parameter 'errorCode' of 'Error.Error(ErrorCode, string)'
        // CS1729: 'CustomError' does not contain a constructor that takes 1 arguments,
        // CS0426: The type name 'CustomErrorCode' does not exist in the type 'CustomError'
        // CS8019: Unnecessary using directive.
    }

    private static GeneratorRunResult Generate(String testCode)
    {
        return Driver.Generate(testCode);
    }
}
