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

[Collection("Sequential")]
public class CurrentTenantAccessorTests
{
    [Fact]
    public async Task CurrentTenantAccessor_FlowsInto_TaskRun()
    {
        // AsyncLocal<T> flows with the ExecutionContext into Task.Run — the value set in the
        // parent is visible inside the child work item.
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var tenant = TestHelper.BuildTenant();

        using (accessor.Change(tenant))
        {
            var result = await Task.Run(() => accessor.CurrentTenant);
            result.Should().Be(tenant);
        }
    }

    [Fact]
    public async Task CurrentTenantAccessor_MutationInside_TaskRun_DoesNotFlowBack()
    {
        // AsyncLocal<T> isolates child mutations: writes made inside Task.Run do not
        // propagate back to the parent execution context.
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var parent = TestHelper.BuildTenant();
        var child = TestHelper.BuildTenant();

        using (current.Change(parent))
        {
            await Task.Run(() =>
            {
                using (current.Change(child))
                {
                    current.CurrentTenant.Should().NotBe(parent);
                }
            }, TestContext.Current.CancellationToken);
            current.CurrentTenant.Should().Be(parent);
        }
    }

    [Fact]
    public async Task CurrentTenantAccessor_FlowsAcrossAwaitContinuations()
    {
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenant = TestHelper.BuildTenant();

        using (current.Change(tenant))
        {
            // Awaiting ambient Task that completes synchronously or yields will preserve ExecutionContext
            await Task.Yield();
            current.CurrentTenant.Should().Be(tenant);

            await Task.Delay(1, TestContext.Current.CancellationToken);
            current.CurrentTenant.Should().Be(tenant);
        }
    }
}
