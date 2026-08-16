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

using Aiel.Results.Models;
using Aiel.Results.TestErrors;

namespace Aiel.Results.IntegrationTests;

public class Program
{
    private static void Main(String[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddResultPattern();

        var app = builder.Build();

        app.MapGet("/success", () => Task.FromResult(Result<IntrinsicTypes>.Success(new IntrinsicTypes())));

        app.MapGet("/failure", () => Task.FromResult(Result<IntrinsicTypes>.Failure(new SimpleError("Missing"))));

        app.MapGet("/collection/success", () => Task.FromResult(Result<IEnumerable<IntrinsicTypes>>.Success([new IntrinsicTypes(), new IntrinsicTypes() {
            BoolValue = true,
            DateTimeValue = DateTime.UtcNow,
            DecimalValue = 1.23m,
            DoubleValue = 4.56,
            FloatValue = 7.89f,
            GuidValue = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            IntValue = 84,
            StringValue = "Hello, World!"
        }])));

        app.MapGet("/collection/failure", () => Task.FromResult(Result<IEnumerable<IntrinsicTypes>>.Failure(new SimpleError("Missing"))));

        app.MapGet("/error", () => Task.FromResult(Result.Failure(new TransactionError("Transaction Error") { Reason = TransactionFailureReason.InsufficientFunds, TransactionId = "11111111-1111-1111-1111-111111111111" })));

        app.Run();
    }
}
