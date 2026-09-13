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

using Aiel.Results;
using Aiel.Testing.Dummies;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Testing.Customers;

public class UpdateCustomerTests(CustomersFixture<CustomerApplicationService> fixture, ITestOutputHelper output)
    : CustomerTestBase<CustomerApplicationService>(fixture, output)
{
    private Guid Id { get; set; }
    private UpdateCustomerCommand Command { get; set; } = default!;
    private Result Result { get; set; } = default!;
    public Customer Updated { get; private set; } = default!;

    public override async ValueTask GivenAsync()
    {
        Id = Guid.NewGuid();
        Command = new UpdateCustomerCommand(Id, "Updated Name", "user@example.com");

        var repository = Services.GetRequiredService<ICustomerRepository>();
        var customer = new Customer(Id, "Original Name");
        await repository.CreateAsync(customer, CancellationToken);
    }

    public override async ValueTask WhenAsync()
        => Result = await SUT.UpdateCustomerAsync(Command, CancellationToken);

    public override async ValueTask ThenAsync()
    {
        var repository = Services.GetRequiredService<ICustomerRepository>();
        Updated = await repository.GetByIdAsync(Id, CancellationToken);
    }

    [Fact]
    public void Result_MustBeSuccess()
    {
        Result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Customer_MustBeUpdated()
    {
        Updated.Name.Should().Be("Updated Name");
        Updated.Email.Should().Be("user@example.com");
    }
}
