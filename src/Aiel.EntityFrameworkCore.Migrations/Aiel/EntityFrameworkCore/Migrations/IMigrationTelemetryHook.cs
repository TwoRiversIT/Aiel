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
/// A plug-in observability seam that receives per-target migration lifecycle events.
/// </summary>
/// <remarks>
/// <para>
/// Aiel defines this contract; consumers provide an implementation that records events to
/// OpenTelemetry, metrics, or any other observability backend. The default registered
/// implementation is <see cref="NullMigrationTelemetryHook"/>, which discards all events.
/// </para>
/// <para>
/// All method parameters are typed — no raw strings — to prevent accidental leakage of
/// connection strings or other secrets through the observability seam.
/// </para>
/// </remarks>
public interface IMigrationTelemetryHook
{
    /// <summary>Called immediately before migrating <paramref name="target"/>.</summary>
    /// <param name="target">The target that is about to be migrated.</param>
    void OnStarted(ITenantMigrationTarget target);

    /// <summary>Called after <paramref name="target"/> has been successfully migrated.</summary>
    /// <param name="target">The target that completed migration.</param>
    void OnCompleted(ITenantMigrationTarget target);

    /// <summary>Called when migration of <paramref name="target"/> fails.</summary>
    /// <param name="target">The target whose migration failed.</param>
    /// <param name="failure">
    /// A sanitised failure record that contains no connection strings or other secrets.
    /// </param>
    void OnFailed(ITenantMigrationTarget target, MigrationFailedTarget failure);
}
