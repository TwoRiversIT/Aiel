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
using System.Collections.Frozen;
using System.Reflection;

namespace Aiel.Mediator;

/// <summary>
/// Configures handler discovery and pipeline behaviors before registering the dispatcher.
/// </summary>
public sealed class DispatcherBuilder
{
    private readonly IServiceCollection _services;
    private readonly Assembly[] _assemblies;
    private readonly List<Type> _behaviorTypes = [];

    internal DispatcherBuilder(IServiceCollection services, Assembly[] assemblies)
    {
        _services = services;
        _assemblies = assemblies;
    }

    /// <summary>
    /// Adds an open generic pipeline behavior to the dispatcher pipeline.
    /// </summary>
    /// <param name="openGenericBehaviorType">
    /// The open generic behavior type to resolve for each dispatched action, such as <c>typeof(ValidationBehavior&lt;&gt;)</c>.
    /// </param>
    /// <returns>The current builder so you can continue chaining configuration.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="openGenericBehaviorType"/> is <see langword="null"/>.
    /// </exception>
    public DispatcherBuilder WithBehavior(Type openGenericBehaviorType)
    {
        ArgumentNullException.ThrowIfNull(openGenericBehaviorType);

        if (!openGenericBehaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"'{openGenericBehaviorType.FullName}' must be an open generic type definition (e.g. typeof(MyBehavior<>)).",
                nameof(openGenericBehaviorType));
        }

        var pipelineBehaviorDef = typeof(IPipelineBehavior<>);
        var implementsPipeline = openGenericBehaviorType
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == pipelineBehaviorDef);

        if (!implementsPipeline)
        {
            throw new ArgumentException(
                $"'{openGenericBehaviorType.FullName}' must implement '{pipelineBehaviorDef.FullName}'.",
                nameof(openGenericBehaviorType));
        }

        _behaviorTypes.Add(openGenericBehaviorType);
        return this;
    }

    /// <summary>
    /// Scans the configured assemblies, registers discovered handlers, and finalizes the dispatcher services.
    /// </summary>
    /// <returns>The original service collection so additional services can be registered.</returns>
    public IServiceCollection Build()
    {
        // Closed behavior service types resolved per-action at wrapper construction time.
        // The wrapper walks the array from the end back to the beginning so the first
        // registered behavior becomes the outermost wrapper.
        var behaviorTypesInRegistrationOrder = _behaviorTypes.ToArray();

        // Register open-generic behavior implementations for DI resolution.
        foreach (var behaviorType in _behaviorTypes)
        {
            _services.AddScoped(behaviorType, behaviorType);
        }

        var actionWrappers = new Dictionary<Type, ActionHandlerBase>();
        var notificationWrappers = new Dictionary<Type, NotificationHandlerBase>();

        foreach (var assembly in _assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType)
                    {
                        continue;
                    }

                    var def = iface.GetGenericTypeDefinition();

                    if (def == typeof(IActionHandler<>))
                    {
                        _services.AddScoped(iface, type);
                        var actionType = iface.GetGenericArguments()[0];
                        if (!actionWrappers.ContainsKey(actionType))
                        {
                            // Close the behavior types against this specific action type.
                            var closedBehaviorTypes = behaviorTypesInRegistrationOrder
                                .Select(b => b.MakeGenericType(actionType))
                                .ToArray();

                            actionWrappers[actionType] =
                                (ActionHandlerBase)Activator.CreateInstance(
                                    typeof(ActionHandlerWrapper<>).MakeGenericType(actionType),
                                    [closedBehaviorTypes])!;
                        }
                    }
                    else if (def == typeof(INotificationHandler<>))
                    {
                        _services.AddScoped(iface, type);
                        var notificationType = iface.GetGenericArguments()[0];
                        if (!notificationWrappers.ContainsKey(notificationType))
                        {
                            notificationWrappers[notificationType] =
                                (NotificationHandlerBase)Activator.CreateInstance(
                                    typeof(NotificationHandlerWrapper<>).MakeGenericType(notificationType))!;
                        }
                    }
                }
            }
        }

        var registry = new DispatcherRegistry(
            actionWrappers.ToFrozenDictionary(),
            notificationWrappers.ToFrozenDictionary());

        _services.AddSingleton(registry);
        _services.AddSingleton<Dispatcher>();
        _services.AddSingleton<ISender>(sp => sp.GetRequiredService<Dispatcher>());
        _services.AddSingleton<IPublisher>(sp => sp.GetRequiredService<Dispatcher>());

        return _services;
    }
}
