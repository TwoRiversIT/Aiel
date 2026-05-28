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
/// Factory methods for well-known application-layer permission errors.
/// </summary>
public static class PermissionErrors
{
    /// <summary>
    /// Creates a <see cref="MissingAuthorizationStoryError"/> for the given permission name.
    /// </summary>
    /// <param name="permission">The permission name with no registered authorization story.</param>
    /// <returns>A <see cref="MissingAuthorizationStoryError"/> describing the gap.</returns>
    public static MissingAuthorizationStoryError MissingAuthorizationStory(PermissionName permission)
        => new(String.Format(PermissionApplicationErrorMessages.MissingAuthorizationStoryFormat, permission));

    /// <summary>
    /// Creates a <see cref="PermissionDeniedError"/> for the given permission name.
    /// </summary>
    /// <param name="permission">The permission name the actor was denied.</param>
    /// <returns>A <see cref="PermissionDeniedError"/> describing the denial.</returns>
    public static PermissionDeniedError PermissionDenied(PermissionName permission)
        => new(String.Format(PermissionApplicationErrorMessages.PermissionDeniedFormat, permission));

    /// <summary>
    /// Creates a <see cref="PermissionValidationError"/> for the given permission name and reason.
    /// </summary>
    /// <param name="permission">The permission name whose action failed validation.</param>
    /// <param name="reason">A human-readable description of the validation failure.</param>
    /// <returns>A <see cref="PermissionValidationError"/> describing the failure.</returns>
    public static PermissionValidationError ValidationFailed(PermissionName permission, String reason)
        => new(String.Format(PermissionApplicationErrorMessages.PermissionValidationFormat, permission, reason));
}
