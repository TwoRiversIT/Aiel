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

namespace Aiel.Authorization.EntityFrameworkCore;

/// <summary>
/// EF Core persistence record for a permission grant.
/// </summary>
/// <remarks>
/// This is an infrastructure type exposed only because EF Core requires it.
/// Consume permission data through <see cref="IPermissionStore"/> instead.
/// </remarks>
public sealed class PermissionGrantRecord
{
    public Guid Id { get; set; }
    public String StableId { get; set; } = String.Empty;
    public String PermissionName { get; set; } = String.Empty;
    public String ScopeType { get; set; } = String.Empty;
    public String ScopeKey { get; set; } = String.Empty;
    public String SubjectType { get; set; } = String.Empty;
    public String SubjectKey { get; set; } = String.Empty;
    public Int32 Decision { get; set; }
    public DateTimeOffset GrantedAt { get; set; }

    public PermissionCatalogRecord Catalog { get; set; } = default!;
}
