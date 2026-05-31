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

/// <summary>
/// Helper methods for reading capability snapshot state.
/// </summary>
public static class ActionCapabilitySnapshotExtensions
{
    /// <summary>
    /// Determines whether a permission is currently executable according to the snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to inspect.</param>
    /// <param name="permission">The permission name.</param>
    /// <returns><see langword="true"/> when the permission is granted; otherwise, <see langword="false"/>.</returns>
    public static Boolean CanExecute(this ActionCapabilitySnapshot snapshot, PermissionName permission)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        foreach (var capability in snapshot.Capabilities)
        {
            if (capability.PermissionName == permission)
            {
                return capability.Decision == PermissionGrantDecision.Granted;
            }
        }

        return false;
    }
}
