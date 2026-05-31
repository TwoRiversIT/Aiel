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
/// Declares that the decorated action class defines a named permission.
/// </summary>
/// <remarks>
/// <para>
/// Applying this attribute causes the <c>Aiel.Authorization.Generators</c> source generator to emit:
/// </para>
/// <list type="bullet">
///   <item>A string constant on <c>GeneratedPermissionNames</c> for the permission name.</item>
///   <item>An <see cref="IActionPermissionChecker{TAction}"/> implementation that delegates to
///         <see cref="IPermissionGrantEvaluator"/> via injected <see cref="IPermissionScopeResolver{TAction}"/>
///         and <see cref="IPermissionSubjectResolver{TAction}"/>.</item>
///   <item>A <c>PermissionDefinitionManifest</c> entry in <c>GeneratedPermissionManifests.GetManifests()</c>
///         for registration with <see cref="IPermissionDefinitionRegistry"/> at startup.</item>
/// </list>
/// <para>
/// The <c>ActionAuthorizationAnalyzer</c> (TRAF01001) treats the generated
/// <see cref="IActionPermissionChecker{TAction}"/> as a valid authorization story for this action.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new <see cref="DefinesPermissionAttribute"/> with required permission metadata.
/// </remarks>
/// <param name="permissionName">
/// The canonical dot-delimited permission name (e.g., <c>scheduling.RescheduleAppointment</c>).
/// Must satisfy the validation rules enforced by <see cref="PermissionName.From"/>.
/// </param>
/// <param name="scopeType">
/// The scope type name this permission applies to (e.g., <c>Location</c>).
/// Used in the generated manifest and passed to <see cref="IPermissionGrantEvaluator.EvaluateAsync"/>.
/// </param>
/// <param name="subjectType">
/// The subject type name this permission targets (e.g., <c>User</c>).
/// Used in the generated manifest and passed to <see cref="IPermissionGrantEvaluator.EvaluateAsync"/>.
/// </param>
/// <param name="displayName">A human-readable label for this permission.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DefinesPermissionAttribute(
    String permissionName,
    String scopeType,
    String subjectType,
    String displayName) : Attribute
{

    /// <summary>Gets the canonical dot-delimited permission name.</summary>
    public String PermissionName { get; } = permissionName;

    /// <summary>Gets the scope type name this permission applies to.</summary>
    public String ScopeType { get; } = scopeType;

    /// <summary>Gets the subject type name this permission targets.</summary>
    public String SubjectType { get; } = subjectType;

    /// <summary>Gets the human-readable display name for this permission.</summary>
    public String DisplayName { get; } = displayName;

    /// <summary>
    /// Gets or sets an optional description of what this permission allows or denies.
    /// Defaults to an empty string when not provided.
    /// </summary>
    public String Description { get; init; } = String.Empty;

    /// <summary>
    /// Gets or sets the lifecycle state of this permission definition.
    /// Defaults to <see cref="PermissionLifecycle.Active"/> when not provided.
    /// </summary>
    public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;

    /// <summary>
    /// Gets or sets previous canonical names this permission was known by.
    /// Populate this when renaming a permission so that existing grants referencing the old name
    /// can be detected and migrated.
    /// </summary>
    public String[] PreviousNames { get; init; } = [];

    /// <summary>
    /// Gets or sets an explicit stable identifier for this permission definition.
    /// </summary>
    /// <remarks>
    /// When not provided, the generator uses the value of <see cref="PermissionName"/> as the stable ID.
    /// Set this explicitly — and populate <see cref="PreviousNames"/> — when renaming a permission
    /// so that previously persisted grants can still be matched by their stable ID.
    /// </remarks>
    public String? StableId { get; init; }
}
