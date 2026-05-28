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
/// Produces the sequence of tenant migration targets for an out-of-band migration run.
/// </summary>
/// <remarks>
/// Aiel defines this contract; Aviendha (or any other consumer) provides an implementation
/// backed by the tenant catalog. This source is never invoked during normal web startup —
/// callers must opt in explicitly by resolving <see cref="ITenantMigrationRunner"/> and
/// calling <see cref="ITenantMigrationRunner.ResumeAsync"/>.
/// </remarks>
public interface ITenantMigrationTargetSource
{
    /// <summary>Enumerates all tenant migration targets asynchronously.</summary>
    /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
    /// <returns>An async sequence of <see cref="ITenantMigrationTarget"/> instances.</returns>
    IAsyncEnumerable<ITenantMigrationTarget> GetTargetsAsync(CancellationToken cancellationToken = default);
}
