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

using Microsoft.AspNetCore.Mvc.Testing;
using Aiel.StrongIds.AspNetCore.IntegrationTests;
using System.Text.Json;

namespace Aiel.StrongIds.AspNetCore;

public class StrongIdBindingTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task Route_and_query_binding_support_generated_strong_ids()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var client = _factory.CreateClient();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .ConfigureForStrongIds();

        var json = await client.GetStringAsync($"/orders/{orderId:D}?customerId={customerId:D}", TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<StrongIdBindingResponse>(json, options);

        result.Should().NotBeNull();
        result!.Id.Should().Be(OrderId.From(orderId));
        result.CustomerId.Should().Be(CustomerId.From(customerId));
    }
}
