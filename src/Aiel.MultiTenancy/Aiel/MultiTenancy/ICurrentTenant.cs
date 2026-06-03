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

namespace Aiel.MultiTenancy;

public interface ICurrentTenant
{
    TenantIdentity? Current { get; }
    IDisposable Change(TenantIdentity? tenant);
}

public sealed class AmbientTenantContext
{
    private readonly AsyncLocal<TenantIdentity?> _current = new();

    public TenantIdentity Current
    {
        get => _current.Value ?? TenantIdentity.Empty;
        set => _current.Value = value;
    }
}

public class CurrentTenant : ICurrentTenant
{
    private readonly AmbientTenantContext _context;

    public CurrentTenant(AmbientTenantContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

    public TenantIdentity Current => _context.Current;

    public IDisposable Change(TenantIdentity? tenant)
    {
        var previous = _context.Current;
        _context.Current = tenant ?? TenantIdentity.Empty;
        return new TenantChangeContext(() => _context.Current = previous);
    }
}

internal sealed class TenantChangeContext : IDisposable
{
    private Action? _restore;
    public TenantChangeContext(Action restore) => _restore = restore ?? throw new ArgumentNullException(nameof(restore));
    public void Dispose()
    {
        var restore = Interlocked.Exchange(ref _restore, null);
        restore?.Invoke();
    }
}
