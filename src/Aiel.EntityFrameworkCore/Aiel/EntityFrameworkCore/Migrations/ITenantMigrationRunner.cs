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
/// An out-of-band runner that applies migrations to every tenant in the registered
/// <see cref="ITenantMigrationTargetSource"/>.
/// </summary>
/// <remarks>
/// <para>
/// This runner is never invoked during normal web startup. Callers must resolve it explicitly
/// from a DI scope — for example in a CLI tool or a scheduled background job — and call
/// <see cref="ResumeAsync"/> with a <see cref="MigrationCheckpoint"/> that represents the
/// current state of completed migrations.
/// </para>
/// <para>
/// Individual target failures do not abort the run. The returned <see cref="TenantMigrationResult"/>
/// collects both successes and failures so the caller can act on the complete picture.
/// </para>
/// </remarks>
public interface ITenantMigrationRunner
{
    /// <summary>
    /// Migrates all targets returned by the registered <see cref="ITenantMigrationTargetSource"/>,
    /// skipping any target already present in <paramref name="checkpoint"/>.
    /// </summary>
    /// <param name="checkpoint">
    /// The set of targets already successfully migrated. Pass <see cref="MigrationCheckpoint.Empty"/>
    /// to process every target from the beginning.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the run.</param>
    /// <returns>
    /// A <see cref="TenantMigrationResult"/> summarising which targets succeeded and which failed.
    /// </returns>
    Task<TenantMigrationResult> ResumeAsync(
        MigrationCheckpoint checkpoint,
        CancellationToken cancellationToken = default);
}
