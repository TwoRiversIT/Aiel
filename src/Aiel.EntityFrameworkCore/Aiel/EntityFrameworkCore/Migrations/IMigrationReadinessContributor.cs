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

namespace Aiel.EntityFrameworkCore.Migrations;

/// <summary>
/// Contributes migration readiness state to infrastructure health-check consumers.
/// </summary>
/// <remarks>
/// Aiel defines this contract. Consumers (typically Aviendha) provide an implementation that
/// queries a persistent store to determine whether all tenant migrations have completed
/// successfully. Health-check middleware resolves and polls implementations of this interface.
/// </remarks>
public interface IMigrationReadinessContributor
{
    /// <summary>
    /// Returns <see langword="true"/> when all known tenant migration targets have been
    /// successfully migrated and the application is safe to accept traffic.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the check.</param>
    Task<Boolean> IsReadyAsync(CancellationToken cancellationToken = default);
}
