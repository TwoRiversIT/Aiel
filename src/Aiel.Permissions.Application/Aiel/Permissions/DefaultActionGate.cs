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
using Aiel.Results;

namespace Aiel.Permissions;

/// <summary>
/// Default application-layer gate that validates an action before running permission checks.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
public sealed class DefaultActionGate<TAction>(IServiceProvider serviceProvider) : IActionGate<TAction>
    where TAction : IAction
{
    /// <inheritdoc />
    public async Task<Result<IActionExecutionContext<TAction>>> AuthorizeAsync(
        IExecutionContext context,
        TAction action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(action);

        var actionContext = ActionExecutionContext<TAction>.CreateChild(context, action);
        var validator = ResolveOptional<IActionValidator<TAction>>();

        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(actionContext, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsSuccess)
            {
                return Result<IActionExecutionContext<TAction>>.Failure(validationResult.Error);
            }
        }

        var checker = ResolveOptional<IActionPermissionChecker<TAction>>();
        if (checker is null)
        {
            var permissionName = ResolveAuthorizationStoryName();
            var definitionRegistry = ResolveOptional<IPermissionDefinitionRegistry>();
            if (definitionRegistry is not null && definitionRegistry.TryGetForAction<TAction>(out var manifest))
            {
                permissionName = manifest.PermissionName;
            }

            return Result<IActionExecutionContext<TAction>>.Failure(
                PermissionErrors.MissingAuthorizationStory(permissionName));
        }

        var permissionResult = await checker.CheckPermissionAsync(actionContext, cancellationToken).ConfigureAwait(false);
        if (!permissionResult.IsSuccess)
        {
            return Result<IActionExecutionContext<TAction>>.Failure(permissionResult.Error);
        }

        return Result<IActionExecutionContext<TAction>>.Success(actionContext);
    }

    private TService? ResolveOptional<TService>()
        where TService : class
        => serviceProvider.GetService(typeof(TService)) as TService;

    private static PermissionName ResolveAuthorizationStoryName()
    {
        var actionName = typeof(TAction).Name.Split('`')[0];
        return PermissionName.From($"action.{actionName}");
    }
}
