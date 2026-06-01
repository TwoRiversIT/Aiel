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

namespace Aiel.Authorization;

internal static class PermissionDomainErrorMessages
{
    internal const String CatalogStableIdRequired = "Permission catalog entries require a non-default stable ID.";
    internal const String CatalogPermissionNameRequired = "Permission catalog entries require a valid permission name.";
    internal const String CatalogScopeTypeRequired = "Permission catalog entries require a valid scope type.";
    internal const String CatalogLifecycleRequired = "Permission catalog entry lifecycle must be a defined value.";
    internal const String GrantIdRequired = "Permission grants require a non-default grant ID.";
    internal const String GrantStableIdRequired = "Permission grants require a non-default stable ID.";
    internal const String GrantPermissionNameRequired = "Permission grants require a valid permission name.";
    internal const String GrantScopeTypeRequired = "Permission grants require a valid scope type.";
    internal const String GrantScopeKeyRequired = "Permission grants require a valid scope key.";
    internal const String GrantSubjectTypeRequired = "Permission grants require a valid subject type.";
    internal const String GrantSubjectKeyRequired = "Permission grants require a valid subject key.";
    internal const String GrantDecisionRequired = "Permission grant decision must be a defined value.";
    internal const String CatalogEntryRequired = "Permission grants require a catalog entry.";
    internal const String RemovedCatalogEntriesCannotIssueGrants = "Removed permission catalog entries cannot issue new grants.";
    internal const String LifecycleCanOnlyAdvanceForward = "Permission lifecycle can only advance from Active to Deprecated to Removed.";
}
