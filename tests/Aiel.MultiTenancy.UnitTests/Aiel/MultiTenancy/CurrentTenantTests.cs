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
public class CurrentTenantTests
{
    [Fact]
    public void Change_ShouldUpdateCurrentTenant()
    {
        // Arrange
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var tenantId = new TenantId(Guid.NewGuid());

        // Act
        using (accessor.Change(TestHelper.BuildTenant(tenantId)))
        {
            // Assert
            accessor.CurrentTenant.Should().NotBeNull();
            accessor.CurrentTenant.TenantId.Should().Be(tenantId);
        }
    }

    [Fact]
    public void Change_ShouldRestoreOriginalTenant()
    {
        // Arrange
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        using (accessor.Change(TestHelper.BuildTenant(tenantId1)))
        {
            // Act
            using (accessor.Change(TestHelper.BuildTenant(tenantId2)))
            {
                // Assert
                accessor.CurrentTenant.Should().NotBeNull();
                accessor.CurrentTenant.TenantId.Should().Be(tenantId2);
            }

            // Assert that the original tenant is restored after the using block
            accessor.CurrentTenant.Should().NotBeNull();
            accessor.CurrentTenant.TenantId.Should().Be(tenantId1);
        }
    }

    [Fact]
    public void Change_ShouldNotAllowNullTenant()
    {
        // Arrange
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var tenantId = new TenantId(Guid.NewGuid());

        using (accessor.Change(TestHelper.BuildTenant(tenantId)))
        {
            // Act
            var act = () =>
            {
                using (accessor.Change(null!))
                {
                    // If the test passes, this line should never be reached
                    // because an exception should be thrown
                }
            };

            act.Should().Throw<ArgumentNullException>();

            accessor.CurrentTenant.TenantId.Should().Be(tenantId);
        }

        accessor.CurrentTenant.Should().Be(CurrentTenant.Empty);
    }

    [Fact]
    public async Task Change_ShouldBeThreadSafe()
    {
        // Arrange
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        using (accessor.Change(TestHelper.BuildTenant(tenantId1)))
        {
            // Act
            var task1 = Task.Run(() =>
            {
                using (accessor.Change(TestHelper.BuildTenant(tenantId2)))
                {
                    accessor.CurrentTenant.Should().NotBeNull();
                    accessor.CurrentTenant.TenantId.Should().Be(tenantId2);
                }
            }, TestContext.Current.CancellationToken);

            var task2 = Task.Run(() =>
            {
                accessor.CurrentTenant.Should().NotBeNull();
                accessor.CurrentTenant.TenantId.Should().Be(tenantId1);
            }, TestContext.Current.CancellationToken);

            await Task.WhenAll(task1, task2);

            // Assert that the original tenant is still intact after both tasks complete
            accessor.CurrentTenant.Should().NotBeNull();
            accessor.CurrentTenant.TenantId.Should().Be(tenantId1);
        }
    }

    [Fact]
    public void Change_ShouldAllowNestedChanges()
    {
        // Arrange
        // Arrange
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());
        var tenantId3 = new TenantId(Guid.NewGuid());
        accessor.Change(TestHelper.BuildTenant(tenantId1));

        // Act
        using (accessor.Change(TestHelper.BuildTenant(tenantId2)))
        {
            accessor.CurrentTenant.Should().NotBeNull();
            accessor.CurrentTenant.TenantId.Should().Be(tenantId2);

            using (accessor.Change(TestHelper.BuildTenant(tenantId3)))
            {
                accessor.CurrentTenant.Should().NotBeNull();
                accessor.CurrentTenant.TenantId.Should().Be(tenantId3);
            }

            // Assert that the tenant is restored to tenantId2 after the inner using block
            accessor.CurrentTenant.Should().NotBeNull();
            accessor.CurrentTenant.TenantId.Should().Be(tenantId2);
        }

        // Assert that the original tenant is restored after the outer using block
        accessor.CurrentTenant.Should().NotBeNull();
        accessor.CurrentTenant.TenantId.Should().Be(tenantId1);
    }

    [Fact]
    public async Task CurrentTenant_FlowsAcrossAsyncAwait()
    {
        // Arrange
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenantId = new TenantId(Guid.NewGuid());
        var tenant = TestHelper.BuildTenant(tenantId);

        // Act
        using (accessor.Change(tenant))
        {
            // Run on a different thread to ensure the tenant context flows correctly across async/await boundaries
            await Task.Run(() => accessor.CurrentTenant.Should().Be(tenant));
        }
    }

    [Fact]
    public async Task CurrentTenant_IsIsolatedAcrossParallelTasks()
    {
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenantId1 = new TenantId(Guid.NewGuid());
        var tenantId2 = new TenantId(Guid.NewGuid());

        CurrentTenant? t1Value = null;
        CurrentTenant? t2Value = null;

        var t1 = Task.Run(() =>
        {
            using (accessor.Change(TestHelper.BuildTenant(tenantId1)))
            {
                t1Value = accessor.CurrentTenant;
            }
        }, TestContext.Current.CancellationToken);

        var t2 = Task.Run(() =>
        {
            using (accessor.Change(TestHelper.BuildTenant(tenantId2)))
            {
                t2Value = accessor.CurrentTenant;
            }
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(t1, t2);

        t1Value!.TenantId.Should().Be(tenantId1);
        t2Value!.TenantId.Should().Be(tenantId2);
    }

    [Fact]
    public void CurrentTenant_NestedScopesRestoreCorrectly()
    {
        using var provider = TestHelper.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var root = TestHelper.BuildTenant(new TenantId(Guid.NewGuid()));
        var inner = TestHelper.BuildTenant(new TenantId(Guid.NewGuid()));

        using (accessor.Change(root))
        {
            accessor.CurrentTenant.Should().Be(root);

            using (accessor.Change(inner))
            {
                accessor.CurrentTenant.Should().Be(inner);
            }

            accessor.CurrentTenant.Should().Be(root);
        }

        accessor.CurrentTenant.Should().Be(CurrentTenant.Empty);
    }
}
