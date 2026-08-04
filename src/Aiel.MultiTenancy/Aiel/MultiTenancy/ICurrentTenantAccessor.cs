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

using System.Diagnostics;

namespace Aiel.MultiTenancy;

/// <summary>
/// Provides access to the resolved <see cref="CurrentTenant"/> for the current execution context.
/// </summary>
/// <remarks>
/// Implementations throw <see cref="InvalidOperationException"/> when called outside a resolved
/// tenant context. Use <see cref="ITenantResolver"/> when explicit handling of all resolution
/// outcomes — including <see cref="TenantResolution.Missing"/> — is required.
/// </remarks>
public interface ICurrentTenantAccessor
{
    CurrentTenant CurrentTenant { get; }
    IDisposable Change(CurrentTenant currentTenant);
}

/// <summary>
/// Provides an implementation of <see cref="ICurrentTenantAccessor" /> based on the current execution context.
/// </summary>
[DebuggerDisplay("CurrentTenant = {CurrentTenant}")]
public class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private static readonly AsyncLocal<CurrentTenantHolder> Local = new();

    /// <inheritdoc/>
    public CurrentTenant CurrentTenant => Local.Value?.CurrentTenant ?? CurrentTenant.Empty;

    public IDisposable Change(CurrentTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        var previous = Local.Value?.CurrentTenant ?? CurrentTenant.Empty;

        // Clear current CurrentTenant trapped in the AsyncLocals, as its done.
        //Local.Value.CurrentTenant = CurrentTenant.Empty;

        // Use an object indirection to hold the CurrentTenant in the AsyncLocal,
        // so it can be cleared in all ExecutionContexts when its cleared.
        Local.Value = new CurrentTenantHolder(tenant);

        return new TenantChangeContext(() => Local.Value = new CurrentTenantHolder(previous));
    }

    private sealed record CurrentTenantHolder(CurrentTenant CurrentTenant);

    private sealed class TenantChangeContext(Action restore) : IDisposable
    {
        private Action? _restore = restore ?? throw new ArgumentNullException(nameof(restore));

        public void Dispose()
        {
            var restore = Interlocked.Exchange(ref _restore, null);
            restore?.Invoke();
        }
    }
}
