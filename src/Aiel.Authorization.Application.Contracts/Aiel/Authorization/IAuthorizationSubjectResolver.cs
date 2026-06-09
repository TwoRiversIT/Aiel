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
using Aiel.Execution;

namespace Aiel.Authorization;

/// <summary>
/// Resolves the permission subject key from an action execution context.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
/// <remarks>
/// <para>
/// Each action type decorated with <see cref="AuthorizationDefinitionAttribute"/> requires a corresponding
/// resolver registered in the DI container. The generated <see cref="IActionAuthorizationChecker{TAction}"/>
/// injects this resolver to identify the specific subject whose permission grants are evaluated.
/// </para>
/// <para>
/// A common implementation extracts the subject key from the actor's identity claim or from a
/// field on the action payload, depending on the bounded context's subject model.
/// </para>
/// </remarks>
public interface IAuthorizationSubjectResolver<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Resolves the subject key from <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The action execution context holding the payload and actor.</param>
    /// <returns>
    /// The <see cref="AuthorizationSubjectKey"/> identifying the subject for the permission check.
    /// </returns>
    AuthorizationSubjectKey ResolveSubjectKey(IActionExecutionContext<TAction> context);
}
