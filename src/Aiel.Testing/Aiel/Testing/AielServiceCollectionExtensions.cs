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

namespace Aiel.Testing;

public static class AielServiceCollectionExtensions
{
    /// <summary>
    /// <b>Do not use this!</b> It is intended for use in Aiel' internal code
    /// to work around limitations of the built-in .NET DI container. Gets the
    /// last concrete instance singleton registered in the collection, if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The instance of the service if found; otherwise, <c>null</c>.</returns>
    public static T? GetInstance<T>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        try
        {
            // Walk from the end so we match the container's resolution order
            for (var i = services.Count - 1; i >= 0; i--)
            {
                var descriptor = services[i];

                if (descriptor.ServiceType != typeof(T))
                {
                    continue;
                }

                if (descriptor.ImplementationInstance is T instance)
                {
                    return instance;
                }

                // If it matches the service type but has no instance,
                // keep searching earlier registrations.
            }
        }
        catch (InvalidOperationException)
        {
            // This can happen if the collection is modified while we're enumerating it.
        }

        return default;
    }
}
