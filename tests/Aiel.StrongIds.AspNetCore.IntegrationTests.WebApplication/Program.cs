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

using System.Diagnostics.CodeAnalysis;

namespace Aiel.StrongIds.AspNetCore.IntegrationTests;

[SuppressMessage("Roslynator", "RCS1102:Make class static", Justification = "No, it is used as an entry point for WebApplicationFactory<TEntryPoint>")]
public class Program
{
    private static void Main(String[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddStrongIdTypeConvertersFromAssemblyContaining<OrderId>();
        builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ConfigureForStrongIds());

        var app = builder.Build();

        app.MapGet("/orders/{id}", (OrderId id, CustomerId? customerId)
            => TypedResults.Ok(new StrongIdBindingResponse(id, customerId)));

        app.Run();
    }
}

public sealed record StrongIdBindingResponse(OrderId Id, CustomerId? CustomerId);

[StrongId<Guid>]
public readonly partial record struct OrderId;

[StrongId<Guid>]
public readonly partial record struct CustomerId;
