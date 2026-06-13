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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aiel.Framework
{
    /// <summary>
    /// Provides access to dependency metadata and orchestrates startup for all configured dependencies.
    /// </summary>
    /// <remarks>
    /// The intention is to keep all the logic related to dependency configuration and initialization out of the
    /// dependency injection container and application host builders, and instead have a single, well-defined place
    /// to manage these concerns, since they only happen once during application startup and will not be needed again
    /// until the application is restarted.
    /// </remarks>
    public interface IDependencyManager
    {
        /// <summary>
    	/// Gets the collection of dependencies that are known to the manager.
        /// </summary>
    	IReadOnlyCollection<DependencyDescriptor> Dependencies { get; }

        /// <summary>
    	/// Configures all dependencies using the supplied configuration context.
        /// </summary>
    	/// <param name="context">The application configuration context.</param>
    	/// <returns>A task that represents the asynchronous configuration operation.</returns>
    	ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default);

        /// <summary>
    	/// Initializes all dependencies using the supplied initialization context.
        /// </summary>
    	/// <param name="context">The application initialization context.</param>
    	/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    	/// <returns>A task that represents the asynchronous initialization operation.</returns>
    	ValueTask InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken = default);
    }
}
