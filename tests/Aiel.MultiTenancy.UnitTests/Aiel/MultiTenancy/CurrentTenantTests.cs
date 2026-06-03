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

public class CurrentTenantTests
{
    [Fact]
    public void Change_ShouldUpdateCurrentTenant()
    {
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var tenantId = new TenantId(Guid.NewGuid());

        // Act
        using (current.Change(new TenantIdentity(tenantId)))
        {
            // Assert
            current.Current.Should().NotBeNull();
            current.Current.TenantId.Should().Be(tenantId);
        }
    }

    [Fact]
    public void Change_ShouldRestoreOriginalTenant()
    {
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        ambient.Current = new TenantIdentity(tenantId1);

        // Act
        using (current.Change(new TenantIdentity(tenantId2)))
        {
            // Assert
            current.Current.Should().NotBeNull();
            current.Current.TenantId.Should().Be(tenantId2);
        }

        // Assert that the original tenant is restored after the using block
        current.Current.Should().NotBeNull();
        current.Current.TenantId.Should().Be(tenantId1);
    }

    [Fact]
    public void Change_ShouldAllowNullTenant()
    {
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId = new TenantId(Guid.NewGuid());
        ambient.Current = new TenantIdentity(tenantId);

        // Act
        using (current.Change(null))
        {
            // Assert
            current.Current.Should().Be(TenantIdentity.Empty);
        }

        // Assert that the original tenant is restored after the using block
        current.Current.Should().NotBeNull();
        current.Current.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Change_ShouldBeThreadSafe()
    {
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        ambient.Current = new TenantIdentity(tenantId1);

        // Act
        var task1 = Task.Run(() =>
        {
            using (current.Change(new TenantIdentity(tenantId2)))
            {
                current.Current.Should().NotBeNull();
                current.Current.TenantId.Should().Be(tenantId2);
            }
        }, TestContext.Current.CancellationToken);

        var task2 = Task.Run(() =>
        {
            current.Current.Should().NotBeNull();
            current.Current.TenantId.Should().Be(tenantId1);
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(task1, task2);

        // Assert that the original tenant is still intact after both tasks complete
        current.Current.Should().NotBeNull();
        current.Current.TenantId.Should().Be(tenantId1);
    }

    [Fact]
    public void Change_ShouldAllowNestedChanges()
    {
        // Arrange
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        var tenantId3 = new TenantId(Guid.NewGuid());
        ambient.Current = new TenantIdentity(tenantId1);

        // Act
        using (current.Change(new TenantIdentity(tenantId2)))
        {
            current.Current.Should().NotBeNull();
            current.Current.TenantId.Should().Be(tenantId2);

            using (current.Change(new TenantIdentity(tenantId3)))
            {
                current.Current.Should().NotBeNull();
                current.Current.TenantId.Should().Be(tenantId3);
            }

            // Assert that the tenant is restored to tenantId2 after the inner using block
            current.Current.Should().NotBeNull();
            current.Current.TenantId.Should().Be(tenantId2);
        }

        // Assert that the original tenant is restored after the outer using block
        current.Current.Should().NotBeNull();
        current.Current.TenantId.Should().Be(tenantId1);
    }

    [Fact]
    public void Change_ShouldAllowNestedNullChanges()
    {
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        ambient.Current = new TenantIdentity(tenantId1);

        // Act
        using (current.Change(null))
        {
            current.Current.Should().Be(TenantIdentity.Empty);
            using (current.Change(new TenantIdentity(tenantId2)))
            {
                current.Current.Should().NotBeNull();
                current.Current.TenantId.Should().Be(tenantId2);
            }

            // Assert that the tenant is restored to null after the inner using block
            current.Current.Should().Be(TenantIdentity.Empty);
        }

        // Assert that the original tenant is restored after the outer using block
        current.Current.Should().NotBeNull();
        current.Current.TenantId.Should().Be(tenantId1);
    }

    [Fact]
    public async Task CurrentTenant_FlowsAcrossAsyncAwait()
    {
        // Arrange
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId = new TenantId(Guid.NewGuid());
        var tenant = new TenantIdentity(tenantId);

        // Act
        using (current.Change(tenant))
        {
            // Run on a different thread to ensure the tenant context flows correctly across async/await boundaries
            await Task.Run(() => current.Current.Should().Be(tenant));
        }
    }

    [Fact]
    public async Task CurrentTenant_IsIsolatedAcrossParallelTasks()
    {
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());

        TenantIdentity? t1Value = null;
        TenantIdentity? t2Value = null;

        var t1 = Task.Run(() =>
        {
            using (current.Change(new TenantIdentity(tenantId1)))
            {
                t1Value = current.Current;
            }
        }, TestContext.Current.CancellationToken);

        var t2 = Task.Run(() =>
        {
            using (current.Change(new TenantIdentity(tenantId2)))
            {
                t2Value = current.Current;
            }
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(t1, t2);

        t1Value!.TenantId.Should().Be(tenantId1);
        t2Value!.TenantId.Should().Be(tenantId2);
    }

    [Fact]
    public void CurrentTenant_NestedScopesRestoreCorrectly()
    {
        using var provider = MultitenancyTestServiceProvider.Build();
        using var scope = provider.CreateScope();
        var ambient = scope.ServiceProvider.GetRequiredService<AmbientTenantContext>();
        var current = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        var root = new TenantIdentity(new TenantId(Guid.NewGuid()));
        var inner = new TenantIdentity(new TenantId(Guid.NewGuid()));

        using (current.Change(root))
        {
            current.Current.Should().Be(root);

            using (current.Change(inner))
            {
                current.Current.Should().Be(inner);
            }

            current.Current.Should().Be(root);
        }

        current.Current.Should().Be(TenantIdentity.Empty);
    }
}
