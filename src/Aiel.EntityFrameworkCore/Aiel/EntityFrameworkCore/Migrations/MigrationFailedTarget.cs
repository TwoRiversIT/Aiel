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
/// Describes a tenant migration target whose migration attempt failed during an out-of-band run.
/// </summary>
/// <remarks>
/// This record deliberately carries only the exception type name — not the full exception object
/// or its message — to prevent accidental leakage of connection strings or other secrets into the
/// result. Full exception details are available through structured logging at the point of failure.
/// </remarks>
/// <param name="Key">The stable key of the target whose migration failed.</param>
/// <param name="Label">The non-sensitive display label for the failed target.</param>
/// <param name="ExceptionTypeName">
/// The <see cref="Type.Name"/> of the exception that caused the failure.
/// </param>
public sealed record MigrationFailedTarget(
    TenantMigrationKey Key,
    TenantMigrationLabel Label,
    String ExceptionTypeName);
