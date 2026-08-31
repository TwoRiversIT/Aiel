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

using Driver = Aiel.Testing.CodeAnalysis.GenerateCS<Aiel.Results.Generators.ErrorClassGenerator>;

namespace Aiel.Results;

public class ErrorClassGeneratorTests
{
    [Fact]
    public async Task Internal_Sealed_Partial_Class_Should_Generate_CustomError()
    {
        const String testCode = """
            using Aiel.Results; 
            using System;

            namespace TestNamespace;

            internal sealed partial class InternalError : Error
            {
                public const String DefaultMessage = "No error.";
            }            
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("internal partial class InternalError : global::Aiel.Results.Error")
            .And.Contain("internal InternalError(String description)")
            .And.Contain("base(InternalErrorCode.Instance, description)")
            .And.Contain("public static readonly InternalErrorCode Instance = new()");

        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task Multiple_Public_Sealed_Partial_Class_Definitions_Should_Generate_Multiple_Errors()
    {
        const String testCode = """
            using Aiel.Results; 
            
            namespace TestNamespace;
            
            public sealed partial class AlphaError : Error;
            public sealed partial class BravoError : Error;
            public sealed partial class CharlieError : Error;
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("public partial class AlphaError : global::Aiel.Results.Error")
            .And.Contain("public AlphaError(String description)")
            .And.Contain("base(AlphaErrorCode.Instance, description)")
            .And.Contain("public static readonly AlphaErrorCode Instance = new()")
            .And.Contain("public partial class BravoError : global::Aiel.Results.Error")
            .And.Contain("public BravoError(String description)")
            .And.Contain("base(BravoErrorCode.Instance, description)")
            .And.Contain("public static readonly BravoErrorCode Instance = new()")
            .And.Contain("public partial class CharlieError : global::Aiel.Results.Error")
            .And.Contain("public CharlieError(String description)")
            .And.Contain("base(CharlieErrorCode.Instance, description)")
            .And.Contain("public static readonly CharlieErrorCode Instance = new()");
    }

    [Fact]
    public async Task Public_Partial_Class_Should_Not_Generate_CustomError()
    {
        // Missing sealed modifier, so the generator should not generate a CustomErrorCode class.
        const String testCode = """
            using Aiel.Results; 
            
            namespace TestNamespace;
            
            public partial class CustomError : Error;
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().BeEmpty();
        result.CompilationDiagnostics.Should().ContainSingle();

        // CS1729: 'Error' does not contain a constructor that takes 0 arguments.
        // This is expected because the generator did not generate a CustomErrorCode class,
        // therefore the compiler generated base constructor call in the CustomError class
        // prototype is invalid.
        result.CompilationDiagnostics[0].Id.Should().Be("CS1729");
    }

    [Fact]
    public async Task Public_Sealed_Class_Should_Not_Generate_CustomError()
    {
        // Missing partial modifier, so the generator should not generate a CustomErrorCode class.
        const String testCode = """
            using Aiel.Results; 
            
            namespace TestNamespace;
            
            public sealed class CustomError : Error;
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().BeEmpty();
        result.CompilationDiagnostics.Should().ContainSingle();

        // CS1729: 'Error' does not contain a constructor that takes 0 arguments.
        // This is expected because the generator did not generate a CustomErrorCode class,
        // therefore the compiler generated base constructor call in the CustomError class
        // prototype is invalid.
        result.CompilationDiagnostics[0].Id.Should().Be("CS1729");
    }

    [Fact]
    public async Task Public_Sealed_Partial_Class_Should_Generate_CustomError()
    {
        const String testCode = """
            using Aiel.Results; 
            
            namespace TestNamespace;
            
            public sealed partial class CustomError : Error;
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("public partial class CustomError : global::Aiel.Results.Error")
            .And.Contain("public CustomError(String description)")
            .And.Contain("base(CustomErrorCode.Instance, description)")
            .And.Contain("public static readonly CustomErrorCode Instance = new()");
    }

    [Fact]
    public async Task Public_Sealed_Partial_Class_With_Parameter_Should_Not_Generate_CustomError()
    {
        const String testCode = """
            using Aiel.Results; 
            using System;

            namespace TestNamespace;
            
            public sealed partial class CustomError(Boolean flag) : Error
            {
                public Boolean Flag { get; } = flag;
                public String Something { get; } = flag.ToString();
            }
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().BeEmpty();
        result.CompilationDiagnostics.Should().ContainSingle();

        // CS1729: 'Error' does not contain a constructor that takes 0 arguments.
        // This is expected because the generator did not generate a CustomErrorCode class,
        // therefore the compiler generated base constructor call in the CustomError class
        // prototype is invalid.
        result.CompilationDiagnostics[0].Id.Should().Be("CS1729");
    }

    [Fact]
    public async Task Public_Sealed_Partial_Class_With_Property_Should_Generate_CustomError()
    {
        const String testCode = """
            using Aiel.Results;
            using System;
            
            namespace TestNamespace;
            
            public sealed partial class CustomError : Error
            {
                public Boolean Flag { get; init; }
            }
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("public partial class CustomError : global::Aiel.Results.Error")
            .And.Contain("public CustomError(String description)")
            .And.Contain("base(CustomErrorCode.Instance, description)")
            .And.Contain("public static readonly CustomErrorCode Instance = new()");
    }

    [Fact]
    public async Task Public_Sealed_Partial_Class_With_Required_Property_Should_Generate_CustomError()
    {
        const String testCode = """
            using Aiel.Results; 
            using System;
            
            namespace TestNamespace;
            
            public sealed partial class CustomError : Error
            {
                public required Boolean Flag { get; init; }
            }
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("public partial class CustomError : global::Aiel.Results.Error")
            .And.Contain("public CustomError(String description)")
            .And.Contain("base(CustomErrorCode.Instance, description)")
            .And.Contain("public static readonly CustomErrorCode Instance = new()");
    }

    [Fact]
    public async Task Class_Overrides_GenerateDescription_Should_Generate_ParameterlessConstructor()
    {
        const String testCode = """
            using Aiel.Results; 
            using System;
            
            namespace TestNamespace;
            
            public sealed partial class CustomError : Error
            {
                public Int32 ID { get; init; }

                protected override String GenerateDescription()
                {
                    return $"CustomError with ID: {ID}";
                }
            }            
            """;

        var result = Driver.Generate(testCode);

        result.Should().NotBeNull();
        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("public partial class CustomError : global::Aiel.Results.Error")
            .And.Contain("public CustomError()")
            .And.Contain("public CustomError(String description)")
            .And.Contain("base(CustomErrorCode.Instance, description)")
            .And.Contain("public static readonly CustomErrorCode Instance = new()");
    }

    [Fact]
    public void Class_Overrides_GenerateDescription_Should_Generate_ParameterlessConstructor_And_Description()
    {
        const String testCode = """
            #nullable enable
            using Aiel.Results; 
            using System;
            
            namespace TestNamespace;
            
            public sealed partial class CustomError : Error
            {
                protected override String? DefaultDescription => "This is the default description for CustomError.";
            }            
            """;

        var result = Driver.Generate(testCode);
        result.Should().NotBeNull();
        result.CompilationDiagnostics.Should().BeEmpty();
        result.GeneratorDiagnostics.Should().BeEmpty();
        result.GeneratedSources.Should().ContainSingle();
        var source = result.GeneratedSources[0].Source.ToString();
        source.Should().NotBeNullOrWhiteSpace()
            .And.Contain("public partial class CustomError : global::Aiel.Results.Error")
            .And.Contain("public CustomError()")
            .And.Contain("public CustomError(String description)")
            .And.Contain("base(CustomErrorCode.Instance, description)")
            .And.Contain("public static readonly CustomErrorCode Instance = new()");
    }
}
