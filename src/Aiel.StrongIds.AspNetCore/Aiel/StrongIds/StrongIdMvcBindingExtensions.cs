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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Aiel.StrongIds;

/// <summary>
/// Provides extension methods for registering strong ID type converters in ASP.NET Core MVC.
/// </summary>
public static class StrongIdMvcBindingExtensions
{
    private static readonly ConcurrentDictionary<Type, Byte> RegisteredStrongIdConverters = new();

    /// <summary>
    /// Registers strong ID type converters for all types implementing <see cref="IStrongId{T}"/> found in the specified assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the type converters to.</param>
    /// <param name="assemblies">The assemblies to scan for strong ID types.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddStrongIdTypeConverters(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var assembly in assemblies)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            foreach (var strongIdType in assembly.GetTypes())
            {
                if (!TryGetStrongIdValueType(strongIdType, out var valueType))
                {
                    continue;
                }

                if (!RegisteredStrongIdConverters.TryAdd(strongIdType, 0))
                {
                    continue;
                }

                var converterType = typeof(StrongIdTypeConverter<,>).MakeGenericType(strongIdType, valueType);
                TypeDescriptor.AddAttributes(strongIdType, new TypeConverterAttribute(converterType));
            }
        }

        return services;
    }

    /// <summary>
    /// Registers strong ID type converters for all types implementing <see cref="IStrongId{T}"/> found in the assembly containing the specified marker type.
    /// </summary>
    /// <typeparam name="TMarker">A type contained in the assembly to scan for strong ID types.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the type converters to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddStrongIdTypeConvertersFromAssemblyContaining<TMarker>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddStrongIdTypeConverters(typeof(TMarker).Assembly);
    }

    private static Boolean TryGetStrongIdValueType(Type candidateType, out Type valueType)
    {
        var interfaceType = candidateType
            .GetInterfaces()
            .FirstOrDefault(static interfaceCandidate =>
                interfaceCandidate.IsGenericType
                && interfaceCandidate.GetGenericTypeDefinition() == typeof(IStrongId<>));

        if (interfaceType is null)
        {
            valueType = typeof(Object);
            return false;
        }

        valueType = interfaceType.GetGenericArguments()[0];
        return true;
    }
}
