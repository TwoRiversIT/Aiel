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
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace Aiel.Customers;

public class Customer
{
    // For EF Core
    protected Customer()
    {
    }

    public Customer(Guid id, String name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name.Trim();
    }

    public Guid Id { get; internal set; }
    public String Name { get; internal set; } = String.Empty;
    public String? Email { get; internal set; } = String.Empty;

    public Customer SetName(String name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        return this;
    }

    public Customer SetEmail(String email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Email = email.Trim();
        return this;
    }
}

[Mapper]
public static partial class CustomerMapper
{
    public static partial CustomerDto ToDto(this Customer customer);
    public static partial Customer ToEntity(this CreateCustomerCommand dto);
}

public record CustomerDto(
    Guid Id,
    String Name,
    String? Email
);

public record CreateCustomerCommand(
    Guid Id,
    String Name,
    String? Email
);

public record GetCustomerByIdQuery(Guid Id);

public record UpdateCustomerCommand(
    Guid Id,
    String Name,
    String? Email
);

public interface ICustomerApplicationService
{
    Task<Result<Guid>> CreateCustomerAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> GetCustomerByIdAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default);
    Task<Result> UpdateCustomerAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default);
}

public class CustomerApplicationService(ICustomerRepository repository) : ICustomerApplicationService
{
    private readonly ICustomerRepository _repository = repository;

    public async Task<Result<Guid>> CreateCustomerAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var entity = command.ToEntity();

        var result = await _repository.CreateAsync(entity, cancellationToken);

        return result;
    }

    public async Task<Result<CustomerDto>> GetCustomerByIdAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(query.Id, cancellationToken);

        var dto = customer.ToDto();

        return Result<CustomerDto>.Success(dto);
    }

    public async Task<Result> UpdateCustomerAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            return new NotFoundError($"Customer with Id '{command.Id}' was not found.");
        }

        if (command.Name is not null)
        {
            existing.SetName(command.Name);
        }

        if (command.Email is not null)
        {
            existing.SetEmail(command.Email);
        }

        await _repository.UpdateAsync(existing, cancellationToken);

        return Result.Success();
    }
}

public class NotFoundError(String description) : Error(NotFoundErrorCode.Instance, description)
{
    internal sealed class NotFoundErrorCode : ErrorCode
    {
        public static readonly NotFoundErrorCode Instance = new();
        protected override String Name => "NotFoundError";
    }
}

public interface ICustomerRepository
{
    Task<Result<Guid>> CreateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
}

public class CustomerRepository(CustomerDbContext dbContext) : ICustomerRepository
{
    private readonly CustomerDbContext _dbContext = dbContext;

    public async Task<Result<Guid>> CreateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }

    public async Task<Customer> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity!;
    }

    public async Task<Result> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        //var existing = await _dbContext.Customers
        //    .SingleOrDefaultAsync(e => e.Id == customer.Id, cancellationToken);

        //if (existing is not null)
        //{
        //    existing.Name = customer.Name;
        //    existing.Email = customer.Email;
        //}

        _dbContext.Customers.Update(customer);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers { get; init; } = default!;
}
