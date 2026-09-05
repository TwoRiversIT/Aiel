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

using Riok.Mapperly.Abstractions;

namespace Aiel.Authorization;

/// <summary>
/// Represents a static class that provides mapping methods for
/// authorization-related objects, specifically for mapping between
/// <see cref="AuthorizationGrant"/> and <see cref="AuthorizationGrantDto"/>.
/// </summary>
[Mapper]
public static partial class AuthorizationMapper
{
    /// <summary>
    /// Maps an <see cref="AuthorizationGrant"/> to an <see cref="AuthorizationGrantDto"/>.
    /// </summary>
    /// <param name="grant">The authorization grant to map.</param>
    /// <returns>The mapped authorization grant DTO.</returns>
    [MapProperty(nameof(AuthorizationGrant.Id), nameof(AuthorizationGrantDto.GrantId))]
    [MapperIgnoreSource(nameof(AuthorizationGrant.PermissionStableId))]
    [MapperIgnoreSource(nameof(AuthorizationGrant.DomainEvents))]
    [MapperIgnoreSource(nameof(AuthorizationGrant.Version))]
    public static partial AuthorizationGrantDto ToDto(this AuthorizationGrant grant);

    /// <summary>
    /// Maps a collection of <see cref="AuthorizationGrant"/> objects to a read-only list of <see cref="AuthorizationGrantDto"/> objects.
    /// </summary>
    /// <param name="grants"></param>
    /// <returns></returns>
    public static partial IReadOnlyList<AuthorizationGrantDto> ToDto(this IEnumerable<AuthorizationGrant> grants);
}
