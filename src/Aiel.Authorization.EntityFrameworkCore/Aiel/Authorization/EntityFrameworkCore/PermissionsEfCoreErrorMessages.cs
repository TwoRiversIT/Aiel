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

internal static class PermissionsEfCoreErrorMessages
{
    internal static String CatalogEntryNotFoundForPermissionName(String permissionName)
        => $"No catalog entry found for permission name '{permissionName}'. Seed the catalog via a migration before creating grants.";

    internal static String GrantNotFound(Guid grantId)
        => $"No permission grant found with ID '{grantId}'.";

    internal static String MigrationCatalogEntryNotFound(String stableId)
        => $"Migration references stable ID '{stableId}' but no catalog entry was found.";

    internal static String MigrationCatalogNameMismatch(String stableId, String expectedName, String actualName)
        => $"Migration expected stable ID '{stableId}' to have permission name '{expectedName}', but found '{actualName}'.";

    internal static String UnknownMigrationOperation(String operationType)
        => $"Unrecognized migration operation type '{operationType}'.";
}
