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
/// Tests for ResultHttpClientUsageAnalyzer - validates that developers use
/// ResultHttpClientExtensions methods instead of generic HttpClient JSON methods
/// with Result types.
/// </summary>
public class ResultHttpClientUsageAnalyzerTests : AnalyzerTestBase<ResultHttpClientUsageAnalyzer>
{
    [Fact]
    public async Task GetFromJsonAsync_WithResultType_ShouldReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;
using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<Result<int>>("url");
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.Prefer_ResultHttpClientExtensions)
            .WithLocation(9, 28);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetFromJsonAsync_WithResultOfT_ShouldReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;
using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<Result<string>>("url");
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.Prefer_ResultHttpClientExtensions)
            .WithLocation(9, 28);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PostAsJsonAsync_WithResultType_ShouldReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;
using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.PostAsJsonAsync<Result<int>>("url", new { data = "test" });
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.Prefer_ResultHttpClientExtensions)
            .WithLocation(9, 28);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PutAsJsonAsync_WithResultType_ShouldReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;
using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.PutAsJsonAsync<Result<int>>("url", new { data = "test" });
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.Prefer_ResultHttpClientExtensions)
            .WithLocation(9, 28);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PatchAsJsonAsync_WithResultType_ShouldReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;
using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.PatchAsJsonAsync<Result<int>>("url", new { data = "test" });
    }
}

""";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.Prefer_ResultHttpClientExtensions)
            .WithLocation(9, 28);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ReadFromJsonAsync_WithResultType_ShouldReportDiagnostic()
    {
        const String testCode = @"
using System.Net.Http.Json;
using Aiel.Results;

class Program
{
    async Task Test(HttpContent content)
    {
        var result = await content.ReadFromJsonAsync<Result<int>>();
    }
}
";

        var diagnostic = new DiagnosticResult(DiagnosticDescriptors.Prefer_ResultHttpClientExtensions)
            .WithLocation(9, 28);

        var test = CreateTest(testCode, diagnostic);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetFromJsonAsync_WithNonResultType_ShouldNotReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;

class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

class Program
{
    async Task Test(HttpClient client)
    {
        var user = await client.GetFromJsonAsync<UserDto>("url");
    }
}

""";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetFromJsonAsync_WithoutGenericArgument_ShouldNotReportDiagnostic()
    {
        const String testCode = """

using System.Net.Http.Json;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.GetAsync("url");
    }
}

""";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ResultHttpClientExtensions_GetResultAsync_ShouldNotReportDiagnostic()
    {
        const String testCode = """

using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.GetResultAsync<int>("url");
    }
}

""";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ResultHttpClientExtensions_PostAndReturnResultAsync_ShouldNotReportDiagnostic()
    {
        const String testCode = """

using Aiel.Results;

class Program
{
    async Task Test(HttpClient client)
    {
        var result = await client.PostAndReturnResultAsync<object, int>("url", new { });
    }
}

""";

        var test = CreateTest(testCode);
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
