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

using Aiel.Results.Analyzers;
using Aiel.Results.Internal;
using Microsoft.CodeAnalysis.Testing;

namespace Aiel.Results;

/// <summary>
/// Tests for ConstructorAnalyzer - validates that Error-derived types
/// have a single public string constructor.
/// </summary>
public class ConstructorAnalyzerTests : AnalyzerTestBase<ConstructorAnalyzer>
{
    [Fact]
    public async Task ValidError_WithSingleStringConstructor_ShouldNotReportDiagnostic()
    {
        const String testCode = """

using Aiel.Results;

public sealed class NotFoundError : Error
{
    public NotFoundError(String message) : base(NotFoundErrorCode.Instance, message) { }
    
    public sealed class NotFoundErrorCode : ErrorCode
    {
        public static readonly NotFoundErrorCode Instance = new();
        protected override String Name => "NotFoundError";
    }
}

""";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidError_WithInitProperties_ShouldNotReportDiagnostic()
    {
        const String testCode = """

using Aiel.Results;

public sealed class OrderNotFoundError : Error
{
    public String OrderId { get; init; }
    
    public OrderNotFoundError(String message) : base(OrderNotFoundErrorCode.Instance, message) { }
    
    public sealed class OrderNotFoundErrorCode : ErrorCode
    {
        public static readonly OrderNotFoundErrorCode Instance = new();
        protected override String Name => "OrderNotFoundError";
    }
}

""";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ErrorWithNoPublicConstructor_ShouldReportDiagnostic()
    {
        const String testCode = """

using Aiel.Results;

public sealed class InvalidError : Error
{
    private InvalidError(String message) : base(InvalidErrorCode.Instance, message) { }
    
    public sealed class InvalidErrorCode : ErrorCode
    {
        public static readonly InvalidErrorCode Instance = new();
        protected override String Name => "InvalidError";
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.DerivedErrorTypesMustHaveSingleStringConstructor)
            .WithArguments("InvalidError")
            .WithLocation(4, 21);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ErrorWithMultiplePublicConstructors_ShouldReportDiagnostic()
    {
        const String testCode = """

using Aiel.Results;

public sealed class ConflictError : Error
{
    public ConflictError(String message) : base(ConflictErrorCode.Instance, message) { }
    
    public ConflictError(String message, Int32 code) : base(ConflictErrorCode.Instance, message) { }
    
    public sealed class ConflictErrorCode : ErrorCode
    {
        public static readonly ConflictErrorCode Instance = new();
        protected override String Name => "ConflictError";
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.DerivedErrorTypesMustHaveSingleStringConstructor)
            .WithArguments("ConflictError")
            .WithLocation(4, 21);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ErrorConstructorWithNonStringParameter_ShouldReportDiagnostic()
    {
        const String testCode = """
            using Aiel.Results;

            public sealed class ValidationError : Error
            {
                public ValidationError(Int32 code) : base(ValidationErrorCode.Instance, "Validation failed") { }
    
                public sealed class ValidationErrorCode : ErrorCode
                {
                    public static readonly ValidationErrorCode Instance = new();
                    protected override String Name => "ValidationError";
                }
            }
            """;

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.DerivedErrorTypesMustHaveSingleStringConstructor)
            .WithArguments("ValidationError")
            .WithLocation(3, 21);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ErrorConstructorWithMultipleParameters_ShouldReportDiagnostic()
    {
        const String testCode = """
            using Aiel.Results;

            public sealed class ServerError : Error
            {
                public ServerError(String message, String details) : base(ServerErrorCode.Instance, message) { }
    
                public sealed class ServerErrorCode : ErrorCode
                {
                    public static readonly ServerErrorCode Instance = new();
                    protected override String Name => "ServerError";
                }
            }
            """;

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.DerivedErrorTypesMustHaveSingleStringConstructor)
            .WithArguments("ServerError")
            .WithLocation(3, 21);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonErrorClass_ShouldNotReportDiagnostic()
    {
        const String testCode = @"
            public sealed class NotAnError
            {
                public NotAnError() { }
            }
            ";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
