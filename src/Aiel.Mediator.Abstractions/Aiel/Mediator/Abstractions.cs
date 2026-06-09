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

using Aiel.Actions;
using Aiel.Commands;
using Aiel.Queries;
using Aiel.Results;

namespace Aiel.Mediator;

/// <summary>
/// Handles a dispatched action and returns a <see cref="Result"/> that describes the outcome.
/// </summary>
/// <typeparam name="TAction">The action type this handler can process.</typeparam>
public interface IActionHandler<in TAction>
    where TAction : IAction
{
    /// <summary>
    /// Processes the supplied action.
    /// </summary>
    /// <param name="action">The action instance to handle.</param>
    /// <param name="cancellationToken">The token that cancels the dispatch.</param>
    /// <returns>A result that indicates whether the action completed successfully.</returns>
    ValueTask<Result> HandleAsync(TAction action, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a dispatched command.
/// </summary>
/// <typeparam name="TCommand">The command type this handler can process.</typeparam>
public interface ICommandHandler<in TCommand> : IActionHandler<TCommand>
    where TCommand : ICommand
{
}

/// <summary>
/// Handles a dispatched query for a projected value.
/// </summary>
/// <typeparam name="TQuery">The query type this handler can process.</typeparam>
/// <typeparam name="TDto">The value type requested by the query.</typeparam>
public interface IQueryHandler<in TQuery, TDto> : IActionHandler<TQuery>
    where TQuery : IQuery<TDto>
{
}

/// <summary>
/// Dispatches commands and queries to their registered handlers.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Dispatches a command to its registered handler.
    /// </summary>
    /// <param name="action">The command to dispatch.</param>
    /// <param name="cancellationToken">The token that cancels the dispatch.</param>
    /// <returns>A result that indicates whether the command completed successfully.</returns>
    ValueTask<Result> ExecuteAsync(
        ICommand action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a query to its registered handler.
    /// </summary>
    /// <typeparam name="TDto">The value type returned when the query succeeds.</typeparam>
    /// <param name="action">The query to dispatch.</param>
    /// <param name="cancellationToken">The token that cancels the dispatch.</param>
    /// <returns>
    /// A result that carries the query value when successful, or the failure that stopped the query.
    /// </returns>
    ValueTask<Result<TDto>> QueryAsync<TDto>(
        IQuery<TDto> action,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Marks a published notification that can fan out to multiple handlers.
/// </summary>
public interface INotification;

/// <summary>
/// Handles a published notification.
/// </summary>
/// <typeparam name="TNotification">The notification type this handler can process.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Processes the supplied notification.
    /// </summary>
    /// <param name="notification">The notification instance to handle.</param>
    /// <param name="cancellationToken">The token that cancels notification handling.</param>
    /// <returns>A task that completes when handling finishes.</returns>
    ValueTask HandleAsync(TNotification notification, CancellationToken cancellationToken);
}

/// <summary>
/// Publishes notifications to all registered notification handlers.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes a notification to every registered handler for its type.
    /// </summary>
    /// <typeparam name="TNotification">The notification type to publish.</typeparam>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="cancellationToken">The token that cancels publication.</param>
    /// <returns>A task that completes when publication finishes.</returns>
    ValueTask PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

/// <summary>
/// Wraps dispatched actions with cross-cutting behavior such as validation or logging.
/// </summary>
/// <typeparam name="TAction">The action type flowing through the pipeline.</typeparam>
public interface IPipelineBehavior<in TAction>
    where TAction : IAction
{
    /// <summary>
    /// Runs around the next behavior or handler in the action pipeline.
    /// </summary>
    /// <param name="request">The dispatched action being processed.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <param name="cancellationToken">The token that cancels the dispatch.</param>
    /// <returns>The result returned by the remaining pipeline.</returns>
    ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken);
}

/// <summary>
/// Represents the next step in a dispatched action pipeline.
/// </summary>
/// <returns>The result returned by the next behavior or terminal handler.</returns>
public delegate ValueTask<Result> ActionHandlerDelegate();
