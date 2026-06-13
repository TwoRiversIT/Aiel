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

using Aiel.Actions.Commands;
using Aiel.Actions.Queries;
using Aiel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Mediator;

internal sealed class Dispatcher(
    IServiceScopeFactory scopeFactory,
    DispatcherRegistry registry) : ISender, IPublisher
{
    public async ValueTask<Result> ExecuteAsync(
        ICommand action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!registry.ActionWrappers.TryGetValue(action.GetType(), out var wrapper))
        {
            throw new InvalidOperationException(
                $"No handler registered for action type '{action.GetType().FullName}'.");
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        return await wrapper.HandleAsync(action, scope.ServiceProvider, cancellationToken);
    }

    public async ValueTask<Result<TDto>> QueryAsync<TDto>(
        IQuery<TDto> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!registry.ActionWrappers.TryGetValue(action.GetType(), out var wrapper))
        {
            throw new InvalidOperationException(
                $"No handler registered for action type '{action.GetType().FullName}'.");
        }

        // The pipeline runs as Result throughout (behaviors are TDto-agnostic).
        // If the handler succeeded it returned a Result<TDto>; cast directly.
        // If a behavior short-circuited with a plain Result.Failure, promote it
        // to Result<TDto>.Failure so the caller gets the correct type in both paths.
        await using var scope = scopeFactory.CreateAsyncScope();
        var result = await wrapper.HandleAsync(action, scope.ServiceProvider, cancellationToken);
        return result as Result<TDto> ?? Result<TDto>.Failure(result.Error);
    }

    public async ValueTask PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!registry.NotificationWrappers.TryGetValue(notification.GetType(), out var wrapper))
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        await wrapper.HandleAsync(notification, scope.ServiceProvider, cancellationToken);
    }
}
