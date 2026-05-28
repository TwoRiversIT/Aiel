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
using System.Reflection;

namespace Aiel.Mediator;

/// <summary>
/// Provides registration helpers for the Aiel mediator dispatcher.
/// </summary>
public static class DispatcherServiceCollectionExtensions
{
    /// <summary>
    /// Scans the supplied assemblies for dispatcher handlers and returns a <see cref="DispatcherBuilder"/>
    /// so you can register pipeline behaviors before finalizing the dispatcher.
    /// </summary>
    /// <param name="services">The service collection that receives dispatcher registrations.</param>
    /// <param name="assemblies">
    /// The assemblies to scan for <c>IActionHandler&lt;&gt;</c> and <c>INotificationHandler&lt;&gt;</c> implementations.
    /// </param>
    /// <returns>
    /// A builder that can register behaviors and complete dispatcher registration with <see cref="DispatcherBuilder.Build"/>.
    /// </returns>
    public static DispatcherBuilder AddDispatcher(this IServiceCollection services, params Assembly[] assemblies)
        => new(services, assemblies);
}
