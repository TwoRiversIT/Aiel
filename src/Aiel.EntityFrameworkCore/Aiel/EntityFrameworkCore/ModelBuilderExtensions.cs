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

using Aiel.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Aiel.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly PropertyInfo HasResolvedTenantProperty =
        typeof(AielDbContext).GetProperty("HasResolvedTenant", InstanceNonPublic)
        ?? throw new InvalidOperationException("AielDbContext must expose HasResolvedTenant.");
    private static readonly PropertyInfo ResolvedTenantIdValueProperty =
        typeof(AielDbContext).GetProperty("ResolvedTenantIdValue", InstanceNonPublic)
        ?? throw new InvalidOperationException("AielDbContext must expose ResolvedTenantIdValue.");

    public static void ApplyMultiTenantQueryFilters(this ModelBuilder modelBuilder, AielDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(dbContext);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(IMultiTenant).IsAssignableFrom(clrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "entity");
            var tenantIdProperty = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
            var tenantIdValue = Expression.Property(tenantIdProperty, nameof(TenantId.Value));
            var context = Expression.Constant(dbContext);
            var hasTenantContext = Expression.Property(context, HasResolvedTenantProperty);
            var currentTenantId = Expression.Property(context, ResolvedTenantIdValueProperty);
            var equalsTenant = Expression.Equal(tenantIdValue, currentTenantId);
            var body = Expression.AndAlso(hasTenantContext, equalsTenant);
            var lambda = Expression.Lambda(body, parameter);

            entityType.SetQueryFilter(lambda);
        }
    }
}
