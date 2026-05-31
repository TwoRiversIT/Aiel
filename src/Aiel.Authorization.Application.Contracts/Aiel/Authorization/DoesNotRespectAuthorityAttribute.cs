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

namespace Aiel.Authorization;

/// <summary>
/// Marks an <see cref="IAction"/> implementation as deliberately not requiring
/// an <see cref="IActionPermissionChecker{TAction}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute when an action is genuinely permission-free — for example,
/// actions available to unauthenticated callers, actions that only read public data,
/// or actions used in test infrastructure.
/// </para>
/// <para>
/// The <see cref="Reason"/> property is required and must contain a non-empty, auditable
/// justification. The <c>ActionAuthorizationAnalyzer</c> (TRPA0001/TRPA0002) enforces this
/// at compile time: a missing authorization story is an error, and an empty Reason is also an error.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DoesNotRespectAuthorityAttribute : Attribute
{
    /// <summary>Gets the justification for why this action does not require a permission check.</summary>
    /// <remarks>
    /// Must be non-empty and non-whitespace. The <c>ActionAuthorizationAnalyzer</c> enforces this
    /// as a compile-time error (TRPA0002).
    /// </remarks>
    public required String Reason { get; init; }
}
