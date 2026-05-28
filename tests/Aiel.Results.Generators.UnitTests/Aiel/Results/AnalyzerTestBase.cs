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

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Aiel.Results;

public abstract class AnalyzerTestBase<TAnalyzer>
    where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new()
{
    private const String CommonUsings = """
        global using System;
        global using System.Net.Http;
        global using System.Threading.Tasks;
        """;

    private const String HttpJsonStubs = """
        namespace System.Net.Http.Json;

        public static class TestHttpClientJsonExtensions
        {
            public static Task<TResponse?> GetFromJsonAsync<TResponse>(this HttpClient client, String? requestUri)
            {
                throw new NotImplementedException();
            }

            public static Task<TResponse?> PostAsJsonAsync<TResponse>(this HttpClient client, String? requestUri, Object value)
            {
                throw new NotImplementedException();
            }

            public static Task<TResponse?> PutAsJsonAsync<TResponse>(this HttpClient client, String? requestUri, Object value)
            {
                throw new NotImplementedException();
            }

            public static Task<TResponse?> PatchAsJsonAsync<TResponse>(this HttpClient client, String? requestUri, Object value)
            {
                throw new NotImplementedException();
            }
        }

        public static class TestHttpContentJsonExtensions
        {
            public static Task<TResponse?> ReadFromJsonAsync<TResponse>(this HttpContent content)
            {
                throw new NotImplementedException();
            }
        }
        """;

    private const String AielResultsStubs = """
        namespace Aiel.Results;

        public abstract class Error
        {
            protected Error(ErrorCode code, String message)
            {
            }
        }

        public abstract class ErrorCode
        {
            protected abstract String Name { get; }
        }

        public readonly struct Result
        {
        }

        public readonly struct Result<T>
        {
        }

        public static class ResultHttpClientExtensions
        {
            public static Task<Result<TResponse>> GetResultAsync<TResponse>(this HttpClient client, String url)
            {
                throw new NotImplementedException();
            }

            public static Task<Result<TResponse>> PostAndReturnResultAsync<TRequest, TResponse>(
                this HttpClient client,
                String url,
                TRequest data)
            {
                throw new NotImplementedException();
            }
        }
        """;

    /// <summary>
    /// Creates a test for an analyzer that should report no diagnostics.
    /// </summary>
    protected CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest(String testCode)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        ConfigureTest(test);
        return test;
    }

    /// <summary>
    /// Creates a test for an analyzer with expected diagnostics.
    /// </summary>
    protected CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest(
        String testCode,
        params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
        };

        ConfigureTest(test);
        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test;
    }

    private static void ConfigureTest(CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> test)
    {
        test.TestState.Sources.Add(CommonUsings);
        test.TestState.Sources.Add(HttpJsonStubs);
        test.TestState.Sources.Add(AielResultsStubs);
    }
}
