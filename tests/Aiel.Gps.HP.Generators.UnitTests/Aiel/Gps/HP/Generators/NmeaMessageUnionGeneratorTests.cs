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

using Aiel.Gps.HP.Generators.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Aiel.Gps.HP.Generators;

public class NmeaMessageUnionGeneratorTests
{
    [Fact]
    public void When_NmeaMessageAttribute_IsDetected_GeneratesUnion()
    {
        const String testCode = """
            namespace Aiel.Gps.HP.Sentences;

            [NmeaMessage("GPGLL")]
            public struct GLL
            {
                public Double Latitude;
                public Double Longitude;
                public TimeOnly FixTime;
                public Char DataActive;
                public Int32 Checksum;
                public override readonly String ToString() => $"{nameof(GLL)} {Latitude} {Longitude} {FixTime} {DataActive}";
            }

            [NmeaParser(typeof(GLL))]
            public readonly struct GllParser : INmeaParser<GLL>
            {
                public ReadOnlySpan<Byte> Identifier => "GPGLL"u8;

                public void Parse(ref Lexer lexer, out GLL msg)
                {
                    lexer.ConsumeString();

                    msg = new GLL()
                    {
                        Latitude = lexer.NextLatitude(),
                        Longitude = lexer.NextLongitude(),
                        FixTime = lexer.NextTime(),
                        DataActive = lexer.NextChar()
                    };
                }
            }            
            """;

        var result = RunGenerator(testCode);

        result.GeneratedTrees.Length.Should().Be(1);

        var checkerTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("NmeaMessage"));

        checkerTree.Should().NotBeNull();

        var sourceText = checkerTree.GetText(TestContext.Current.CancellationToken);
        sourceText.Should().NotBeNull();
        var source = sourceText.ToString();
        source.Should().Contain("public partial struct NmeaMessage");
        source.Should().Contain("public Aiel.Gps.HP.Sentences.GLL GLL;");
    }

    [Fact]
    public void When_NmeaMessageAttribute_IsNotDetected_Raises_AIEL00019_Diagnostic()
    {
        const String testCode = """
            namespace Aiel.Gps.HP.Sentences;

            public struct GLL
            {
                public Double Latitude;
                public Double Longitude;
                public TimeOnly FixTime;
                public Char DataActive;
                public Int32 Checksum;
                public override readonly String ToString() => $"{nameof(GLL)} {Latitude} {Longitude} {FixTime} {DataActive}";
            }

            public readonly struct GllParser : INmeaParser<GLL>
            {
                public ReadOnlySpan<Byte> Identifier => "GPGLL"u8;

                public void Parse(ref Lexer lexer, out GLL msg)
                {
                    lexer.ConsumeString();

                    msg = new GLL()
                    {
                        Latitude = lexer.NextLatitude(),
                        Longitude = lexer.NextLongitude(),
                        FixTime = lexer.NextTime(),
                        DataActive = lexer.NextChar()
                    };
                }
            }            
            """;

        var result = RunGenerator(testCode);

        result.GeneratedTrees.Length.Should().Be(0);
        result.Diagnostics.Length.Should().Be(1);
        var diagnostic = result.Diagnostics[0];
        diagnostic.Descriptor.Should().Be(DiagnosticDescriptors.NoNmeaMessageTypesDiscovered);
    }

    [Fact]
    public void When_AielGpsHp_IsNotReferenced_DoesNothing()
    {
        const String testCode = """
            namespace Aiel.Gps.HP.Sentences;

            public struct GLL
            {
                public Double Latitude;
                public Double Longitude;
                public TimeOnly FixTime;
                public Char DataActive;
                public Int32 Checksum;
                public override readonly String ToString() => $"{nameof(GLL)} {Latitude} {Longitude} {FixTime} {DataActive}";
            }

            public readonly struct GllParser : INmeaParser<GLL>
            {
                public ReadOnlySpan<Byte> Identifier => "GPGLL"u8;

                public void Parse(ref Lexer lexer, out GLL msg)
                {
                    lexer.ConsumeString();

                    msg = new GLL()
                    {
                        Latitude = lexer.NextLatitude(),
                        Longitude = lexer.NextLongitude(),
                        FixTime = lexer.NextTime(),
                        DataActive = lexer.NextChar()
                    };
                }
            }            
            """;

        var result = RunGenerator(testCode, includeStubs: false);

        result.GeneratedTrees.Length.Should().Be(0);
        result.Diagnostics.Length.Should().Be(0);
    }

    private static GeneratorDriverRunResult RunGenerator(String source, Boolean includeStubs = true)
        => RunGeneratorWithUpdatedCompilation(source, includeStubs).RunResult;

    private static (CSharpCompilation Compilation, GeneratorDriverRunResult RunResult) RunGeneratorWithUpdatedCompilation(String source, Boolean includeStubs = true)
    {
        var trees = new List<SyntaxTree>()
        {
            CSharpSyntaxTree.ParseText(source)
        };

        if (includeStubs)
        {
            trees.Add(CSharpSyntaxTree.ParseText(Stubs.Attributes));
        }

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "NmeaMessageUnionGeneratorTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new NmeaMessageUnionGenerator();
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()]);
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return ((CSharpCompilation)outputCompilation, updatedDriver.GetRunResult());
    }
}
