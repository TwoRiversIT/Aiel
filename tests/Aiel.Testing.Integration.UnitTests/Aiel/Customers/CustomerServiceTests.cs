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

using Aiel.Testing.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Customers;

public class CustomerServiceTests(CustomersFixture fixture, ITestOutputHelper output)
    : CustomerTestBase<CustomerApplicationService>(fixture, output)
{
    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturn_Success()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new CreateCustomerCommand(id, "Acme Corporation", "contact@acme.com");
        var result = await SUT.CreateCustomerAsync(command, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);

        var repository = Services.GetRequiredService<ICustomerRepository>();
        var created = await repository.GetByIdAsync(id, CancellationToken);
        created.Name.Should().Be("Acme Corporation");
        created.Email.Should().Be("contact@acme.com");
    }

    [Fact]
    public async Task GetCustomer_WhenExists_ShouldReturnCustomer()
    {
        // Arrange - Create test data using the repository directly
        var id = Guid.NewGuid();
        var repository = Services.GetRequiredService<ICustomerRepository>();
        var customer = new Customer(id, "Test Corp");
        await repository.CreateAsync(customer, TestContext.Current.CancellationToken);

        // Act - Use the SUT to retrieve it
        var query = new GetCustomerByIdQuery(id);
        var result = await SUT.GetCustomerByIdAsync(query, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be("Test Corp");
        result.Value.Email.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateCustomer_WhenExists_ShouldPersistChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = Services.GetRequiredService<ICustomerRepository>();
        var customer = new Customer(id, "Original Name");
        await repository.CreateAsync(customer, CancellationToken);

        // Act
        var command = new UpdateCustomerCommand(id, "Updated Name", "user@example.com");
        await SUT.UpdateCustomerAsync(command, CancellationToken);

        // Assert
        var updated = await repository.GetByIdAsync(id, CancellationToken);
        updated.Name.Should().Be("Updated Name");
        updated.Email.Should().Be("user@example.com");
    }
}
