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
/// An opaque unit passed to an out-of-band migration runner for a single tenant database.
/// </summary>
/// <remarks>
/// <para>
/// Aiel defines only this contract; Aviendha (or any other consumer) provides the
/// implementation. The interface intentionally exposes no raw strings to prevent accidental
/// leakage of connection strings or other secrets into telemetry or logs.
/// </para>
/// <para>
/// Use <see cref="Key"/> for stable identity tracking (checkpoint resume) and
/// <see cref="Label"/> for safe display in logs and trace events.
/// </para>
/// </remarks>
public interface ITenantMigrationTarget
{
    /// <summary>
    /// Gets the opaque, stable key that uniquely identifies this tenant migration target.
    /// </summary>
    TenantMigrationKey Key { get; }

    /// <summary>
    /// Gets the non-sensitive human-readable label safe for use in logs and telemetry.
    /// </summary>
    TenantMigrationLabel Label { get; }
}
