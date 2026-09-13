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

using Aiel.Domain.Errors;
using Aiel.Results;

namespace Aiel.Testing.Dummies;

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
            return AfError.Notfound<Customer>(command.Id);
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
