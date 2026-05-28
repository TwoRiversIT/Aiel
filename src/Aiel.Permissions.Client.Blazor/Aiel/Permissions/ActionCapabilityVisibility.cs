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

namespace Aiel.Permissions;

/// <summary>
/// Blazor-focused visibility helpers for capability snapshots.
/// </summary>
public static class ActionCapabilityVisibility
{
    /// <summary>
    /// Determines whether the current snapshot allows the supplied permission.
    /// </summary>
    /// <param name="snapshot">The current capability snapshot.</param>
    /// <param name="permission">The permission name to evaluate.</param>
    /// <returns><see langword="true"/> when the permission is granted; otherwise, <see langword="false"/>.</returns>
    public static Boolean CanExecute(ActionCapabilitySnapshot snapshot, PermissionName permission)
        => snapshot.CanExecute(permission);
}
