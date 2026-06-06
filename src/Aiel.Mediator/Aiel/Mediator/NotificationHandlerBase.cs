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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aiel.Mediator;

internal abstract class NotificationHandlerBase
{
    public abstract ValueTask HandleAsync(
        Object notification,
        IServiceProvider provider,
        CancellationToken cancellationToken);
}

internal sealed class NotificationHandlerWrapper<TNotification>
    : NotificationHandlerBase
    where TNotification : INotification
{
    public override async ValueTask HandleAsync(
        Object notification,
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var typed = (TNotification)notification;
        var handlers = provider.GetServices<INotificationHandler<TNotification>>();

        // The ILoggerFactory is always registered by AddLogging() (which AddDispatcher calls),
        // so in production the real logger is always available. The fallback only exists for
        // test ServiceCollections that are built manually without logging infrastructure.
        var logger = provider.GetService<ILogger<NotificationHandlerWrapper<TNotification>>>()
            ?? NullLogger<NotificationHandlerWrapper<TNotification>>.Instance;

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(typed, cancellationToken);
            }
            catch (Exception ex)
            {
                // Every handler must be invoked even if a previous one fails.
                // Exceptions are logged so that a single bad handler does not
                // silently suppress the rest.
                logger.LogError(ex, "The {Handler} threw an exception. See the inner exception for details.", handler.GetType().Name);
            }
        }
    }
}

