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

namespace Aiel.Dependencies;

/// <summary>
/// Configures services and options for a dependency during application startup.
/// </summary>
/// <remarks>
/// Configuration runs in two sequential phases across the full dependency graph. Phase 1
/// (<see cref="PreConfigureAsync"/>) completes for every module before phase 2
/// (<see cref="ConfigureAsync"/>) begins for any module. Both phases execute in
/// topological order — deepest dependencies first.
/// </remarks>
public interface IDependencyConfigurator
{
    /// <summary>
    /// Performs early shared setup before any module's <see cref="ConfigureAsync"/> runs.
    /// </summary>
    /// <remarks>
    /// Use this phase to register options builders, configure integration entry points,
    /// or perform any work that other modules need to have completed before their own
    /// configure phase begins. By the time <see cref="ConfigureAsync"/> is called on
    /// any module, every module in the graph has already completed this method.
    /// </remarks>
    /// <param name="context">The application configuration context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous pre-configuration operation.</returns>
    ValueTask PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures services and options for the current dependency.
    /// </summary>
    /// <remarks>
    /// This phase runs after <see cref="PreConfigureAsync"/> has completed for every
    /// module in the graph. Use it for main service registration and configuration,
    /// including reading options established during pre-configuration.
    /// </remarks>
    /// <param name="context">The application configuration context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A value task that represents the asynchronous configuration operation.</returns>
    ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default);
}
