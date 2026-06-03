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

using Microsoft.Extensions.DependencyInjection;

namespace Aiel.MultiTenancy;

public class AmbientTenantContextTests
{
    [Fact]
    public void AmbientTenantContext_IsolatedAcrossInstances_And_ChangeRestores()
    {
        var a = new AmbientTenantContext();
        var b = new AmbientTenantContext();

        // initial state
        a.Current.Should().Be(TenantIdentity.Empty);
        b.Current.Should().Be(TenantIdentity.Empty);

        var tenant = new TenantIdentity(new TenantId(Guid.NewGuid()));

        using (var change = new CurrentTenant(a).Change(tenant))
        {
            // only 'ambient' should see the tenant
            a.Current.Should().Be(tenant);
            b.Current.Should().Be(TenantIdentity.Empty);
        }

        // restore happened
        a.Current.Should().Be(TenantIdentity.Empty);
    }

    [Fact]
    public async Task AmbientTenantContext_FlowsInto_TaskRun()
    {
        // AsyncLocal<T> flows with the ExecutionContext into Task.Run — the value set in the
        // parent is visible inside the child work item.
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenant = new TenantIdentity(new TenantId(Guid.NewGuid()));

        using (current.Change(tenant))
        {
            var result = await Task.Run(() => current.Current);
            result.Should().Be(tenant);
        }
    }

    [Fact]
    public async Task AmbientTenantContext_MutationInside_TaskRun_DoesNotFlowBack()
    {
        // AsyncLocal<T> isolates child mutations: writes made inside Task.Run do not
        // propagate back to the parent execution context.
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var parent = new TenantIdentity(new TenantId(Guid.NewGuid()));
        var child = new TenantIdentity(new TenantId(Guid.NewGuid()));

        using (current.Change(parent))
        {
            await Task.Run(() => current.Change(child));
            current.Current.Should().Be(parent);
        }
    }

    [Fact]
    public async Task AmbientTenantContext_FlowsAcrossAwaitContinuations()
    {
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenant = new TenantIdentity(new TenantId(Guid.NewGuid()));

        using (current.Change(tenant))
        {
            // Awaiting ambient Task that completes synchronously or yields will preserve ExecutionContext
            await Task.Yield();
            current.Current.Should().Be(tenant);

            await Task.Delay(1, TestContext.Current.CancellationToken);
            current.Current.Should().Be(tenant);
        }
    }

    [Fact]
    public async Task AmbientTenantContext_IsolatedPerDIScope_MessageHandlerPattern()
    {
        using var provider = MultitenancyTestServiceProvider.Build();

        // Simulate message 1
        using (var scope1 = provider.CreateScope())
        {
            var ambient1 = scope1.ServiceProvider.GetRequiredService<AmbientTenantContext>();
            var current1 = scope1.ServiceProvider.GetRequiredService<ICurrentTenant>();

            var tenant1 = new TenantIdentity(new TenantId(Guid.NewGuid()));
            ambient1.Current = tenant1;

            // resolved CurrentTenant in this scope sees tenant1
            current1.Current.Should().Be(tenant1);

            // Task.Run inside this scope should see the ambient
            var fromTask = await Task.Run(() => current1.Current);
            fromTask.Should().Be(tenant1);
        }

        // Simulate message 2 (different scope) — must not see tenant1
        using (var scope2 = provider.CreateScope())
        {
            var current2 = scope2.ServiceProvider.GetRequiredService<ICurrentTenant>();
            current2.Current.Should().Be(TenantIdentity.Empty);
        }
    }
}
