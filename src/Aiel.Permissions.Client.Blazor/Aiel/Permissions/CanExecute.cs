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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Aiel.Permissions;

/// <summary>
/// Renders child content only when the supplied permission is available in the current snapshot.
/// </summary>
public sealed class CanExecute : ComponentBase
{
    private static readonly RenderFragment EmptyChildContent = static _ => { };

    /// <summary>
    /// Gets or sets the capability snapshot.
    /// </summary>
    [Parameter]
    public ActionCapabilitySnapshot Snapshot { get; set; } = ActionCapabilitySnapshot.Empty;

    /// <summary>
    /// Gets or sets the permission to evaluate.
    /// </summary>
    [Parameter]
    public PermissionName Permission { get; set; }

    /// <summary>
    /// Gets or sets the child content rendered when the permission is granted.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = EmptyChildContent;

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ActionCapabilityVisibility.CanExecute(Snapshot, Permission))
        {
            builder.AddContent(0, ChildContent);
        }
    }
}
