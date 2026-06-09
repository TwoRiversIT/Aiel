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

using Aiel.Results;

namespace Aiel.Authorization.EntityFrameworkCore;

/// <summary>
/// Represents a failure when a permission catalog entry cannot be found for the requested permission name.
/// </summary>
public sealed partial class PermissionCatalogEntryNotFoundError : Error;

/// <summary>
/// Represents a failure when a permission grant cannot be found for the requested grant ID.
/// </summary>
public sealed partial class AuthorizationGrantNotFoundError : Error;

/// <summary>
/// Represents a failure when a migration operation references a catalog entry that does not exist.
/// </summary>
public sealed partial class MigrationCatalogEntryNotFoundError : Error;

/// <summary>
/// Represents a failure when a rename migration does not match the catalog's current permission name.
/// </summary>
public sealed partial class MigrationCatalogNameMismatchError : Error;

/// <summary>
/// Represents a failure caused by an unrecognized migration operation type.
/// </summary>
public sealed partial class UnknownMigrationOperationError : Error;
