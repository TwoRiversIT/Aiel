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

using Aiel.Actions;
using System.Globalization;
using System.Text;

namespace Aiel.Authorization;

/// <summary>
/// Factory methods for well-known application-layer authorization errors.
/// </summary>
public static class AuthorizationErrors
{
    private static readonly CompositeFormat Missing = CompositeFormat.Parse("No authorization story is registered for authorization '{0}'. Register a definition in IAuthorizationDefinitionRegistry.");
    private static readonly CompositeFormat Denied = CompositeFormat.Parse("The actor does not have the required '{0}' authorization for the requested scope.");
    private static readonly CompositeFormat Validation = CompositeFormat.Parse("Action validation failed for authorization '{0}': {1}");

    /// <summary>
    /// Creates a <see cref="MissingAuthorizationStoryError"/> for the given permission name.
    /// </summary>
    /// <param name="permission">The permission name with no registered authorization story.</param>
    /// <returns>A <see cref="MissingAuthorizationStoryError"/> describing the gap.</returns>
    public static MissingAuthorizationStoryError MissingAuthorizationStory<TAction>(PermissionName permission)
        where TAction : IAction
        => new(String.Format(CultureInfo.CurrentCulture, Missing, permission));

    /// <summary>
    /// Creates a <see cref="AuthorizationDeniedError"/> for the given permission name.
    /// </summary>
    /// <param name="permission">The permission name the actor was denied.</param>
    /// <returns>A <see cref="AuthorizationDeniedError"/> describing the denial.</returns>
    public static AuthorizationDeniedError PermissionDenied(PermissionName permission)
        => new(String.Format(CultureInfo.CurrentCulture, Denied, permission));

    /// <summary>
    /// Creates a <see cref="AuthorizationValidationError"/> for the given permission name and reason.
    /// </summary>
    /// <param name="permission">The permission name whose action failed validation.</param>
    /// <param name="reason">A human-readable description of the validation failure.</param>
    /// <returns>A <see cref="AuthorizationValidationError"/> describing the failure.</returns>
    public static AuthorizationValidationError ValidationFailed(PermissionName permission, String reason)
        => new(String.Format(CultureInfo.CurrentCulture, Validation, permission, reason));
}
