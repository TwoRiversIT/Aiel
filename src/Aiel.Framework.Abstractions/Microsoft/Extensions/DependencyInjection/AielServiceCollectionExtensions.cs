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

using Aiel.Collections;
using Aiel.Framework;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface to facilitate service registration and retrieval in the Aiel framework.
/// </summary>
public static class AielServiceCollectionExtensions
{
    /// <summary>
    /// Replaces all existing registrations of <typeparamref name="TInterface"/> with a new registration of <typeparamref name="TImplementation"/> using the specified <paramref name="serviceLifetime"/>.
    /// </summary>
    /// <typeparam name="TInterface">The type of the service interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceLifetime">The lifetime of the service.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="services"/> parameter is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="serviceLifetime"/> parameter is not a valid <see cref="ServiceLifetime"/> value.</exception>
    public static IServiceCollection Replace<TInterface, TImplementation>(this IServiceCollection services, ServiceLifetime serviceLifetime)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var existing in services.Find<TInterface>())
        {
            services.Remove(existing);
        }

        return serviceLifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<TInterface, TImplementation>(),
            ServiceLifetime.Scoped => services.AddScoped<TInterface, TImplementation>(),
            ServiceLifetime.Transient => services.AddTransient<TInterface, TImplementation>(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null)
        };
    }

    /// <summary>
    /// Finds all service descriptors in the collection that match the specified interface type <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">The type of the service interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>An array of matching <see cref="ServiceDescriptor"/> instances.</returns>
    public static ServiceDescriptor[] Find<TInterface>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.Where(d => d.ServiceType == typeof(TInterface)).ToArray();
    }

    /// <summary>
    /// Gets the last concrete instance singleton registered in the collection, throwing an exception if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The instance of the service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the singleton instance is not found.</exception>
    public static T GetRequiredSingleton<T>(this IServiceCollection services)
        where T : class
        => services.GetSingleton<T>()
            ?? throw new InvalidOperationException("Could not find singleton instance for: " + typeof(T).AssemblyQualifiedName);

    /// <summary>
    /// Gets the last concrete instance singleton registered in the collection, if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The instance of the service if found; otherwise, <c>null</c>.</returns>
    public static T? GetSingleton<T>(this IServiceCollection services)
        where T : class
    {
        return services
            .FirstOrDefault(d => d.ServiceType == typeof(T))
            ?.GetInstance<T>();
    }

    /// <summary>
    /// Normalizes the implementation instance data between keyed and not keyed services.
    /// </summary>
    /// <param name="descriptor">
    /// The <see cref="ServiceDescriptor"/> to normalize.
    /// </param>
    /// <returns>
    /// The appropriate implementation instance from the service descriptor.
    /// </returns>
    private static T? GetInstance<T>(this ServiceDescriptor descriptor)
        where T : class
        => descriptor.IsKeyedService
            ? descriptor.KeyedImplementationInstance as T
            : descriptor.ImplementationInstance as T;

    /// <summary>
    /// Registers a callback that is invoked each time a <see cref="ServiceDescriptor"/> is added to
    /// <paramref name="services"/> after this call.
    /// </summary>
    /// <param name="services">
    /// Must be an <see cref="ObservableServiceCollection"/>; the Aiel
    /// dependency framework ensures this when it creates <c>ConfigurationContext.Services</c>.
    /// </param>
    /// <param name="callback">The action to invoke with each newly added <see cref="ServiceDescriptor"/>.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="callback"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="services"/> is not an
    /// <see cref="ObservableServiceCollection"/>.
    /// </exception>
    public static IServiceCollection OnAdding(this IServiceCollection services, Action<ServiceDescriptor> callback)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(callback);

        if (services is not ObservableServiceCollection observable)
        {
            throw new InvalidOperationException(
                $"{nameof(OnAdding)} requires an {nameof(ObservableServiceCollection)}. " +
                "Ensure the Aiel dependency framework created the service collection.");
        }

        observable.Subscribe(callback);
        return services;
    }

    /// <summary>
    /// Decorates an existing <see cref="ICollection{T}"/> registration with <see cref="CollectionDecorator{T}"/>.
    /// </summary>
    /// <typeparam name="T">The item type in the collection.</typeparam>
    /// <param name="services">The service collection containing an <see cref="ICollection{T}"/> registration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="ICollection{T}"/> has not been registered.</exception>
    public static IServiceCollection DecorateCollection<T>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.Decorate<ICollection<T>, CollectionDecorator<T>>();
    }

    /// <summary>
    /// Decorates an existing service registration with a specified decorator type, allowing for additional behavior to
    /// be applied to the service.
    /// </summary>
    /// <remarks>This method allows for modifying the behavior of an existing service by wrapping it with a
    /// decorator. It is important to ensure that the service to be decorated is already registered before calling this
    /// method.</remarks>
    /// <typeparam name="TService">The type of the service to be decorated.</typeparam>
    /// <typeparam name="TDecorator">The type of the decorator that implements the service interface.</typeparam>
    /// <param name="services">The collection of service descriptors to which the service and its decorator will be added.</param>
    /// <returns>The updated IServiceCollection instance, allowing for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified service type is not registered in the service collection.</exception>
    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService
    {
        ArgumentNullException.ThrowIfNull(services);

        // Find the existing registration
        var original = services.LastOrDefault(s => s.ServiceType == typeof(TService))
            ?? throw new InvalidOperationException(
                $"Cannot decorate {typeof(TService).Name} because it is not registered.");

        // Remove the original registration
        services.Remove(original);

        // Re-register the original implementation under a private type
        var innerType = original.ImplementationType
                        ?? original.ImplementationInstance?.GetType()
                        ?? original.ImplementationFactory?.GetType()
                        ?? throw new InvalidOperationException("Unsupported registration type.");

        var innerDescriptor = ServiceDescriptor.Describe(
            innerType,
            original.ImplementationFactory ?? (sp => ActivatorUtilities.CreateInstance(sp, innerType)),
            original.Lifetime);

        services.Add(innerDescriptor);

        // Register the decorator as the new TService
        services.Add(new ServiceDescriptor(
            typeof(TService),
            sp =>
            {
                var inner = sp.GetRequiredService(innerType);
                return ActivatorUtilities.CreateInstance<TDecorator>(sp, inner);
            },
            original.Lifetime));

        return services;
    }
}
