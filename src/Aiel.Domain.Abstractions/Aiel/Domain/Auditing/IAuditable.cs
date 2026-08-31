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

namespace Aiel.Domain.Auditing;

/// <summary>
/// Marker interface that indicates participation in auditing.
/// </summary>
public interface IAuditable;

/// <summary>
/// Declares the standard audit metadata required on persisted entities.
/// </summary>
public interface IAudited : ICreated, IUpdated
{
}

/// <summary>
/// Declares the standard audit metadata required on persisted entities,
/// including soft-delete support.
/// </summary>
/// <remarks>
/// Persistence and unit-of-work infrastructure are responsible for setting
/// these values.
/// </remarks>
public interface ISetAudited : IAudited, ISetCreated, ISetUpdated
{
}

/// <summary>
/// Indicates who and when an entity was created.
/// </summary>
/// <remarks>
/// The CreatedBy field intentionally uses a framework-neutral string
/// identifier so the domain layer does not depend on application-layer
/// execution-context contracts.
/// </remarks>
public interface ICreated : IAuditable
{
    DateTimeOffset CreatedAt { get; }
    String CreatedBy { get; }
}

/// <summary>
/// Indicates who and when an entity was created, and allows setting those values.
/// </summary>
/// <remarks>
/// The CreatedBy field intentionally uses a framework-neutral string
/// identifier so the domain layer does not depend on application-layer
/// execution-context contracts.
/// </remarks>
public interface ISetCreated : ICreated
{
    void SetCreated(String createdBy, DateTimeOffset createdAt);
}

/// <summary>
/// Indicates who and when an entity was last updated.
/// </summary>
/// <remarks>
/// The UpdatedBy field intentionally uses a framework-neutral string
/// identifier so the domain layer does not depend on application-layer
/// execution-context contracts.
/// </remarks>
public interface IUpdated : IAuditable
{
    DateTimeOffset UpdatedAt { get; }
    String UpdatedBy { get; }
}

/// <summary>
/// Indicates who and when an entity was last updated, and allows setting those values.
/// </summary>
/// <remarks>
/// The UpdatedBy field intentionally uses a framework-neutral string
/// identifier so the domain layer does not depend on application-layer
/// execution-context contracts.
/// </remarks>
public interface ISetUpdated : IUpdated
{
    void SetUpdated(String updatedBy, DateTimeOffset updatedAt);
}

/// <summary>
/// Indicates who and when an entity was deleted (soft delete).
/// </summary>
/// <remarks>
/// The DeletedBy field intentionally uses a framework-neutral string
/// identifier so the domain layer does not depend on application-layer
/// execution-context contracts.
/// </remarks>
public interface IDeleted : IAuditable
{
    DateTimeOffset? DeletedAt { get; }
    String? DeletedBy { get; }
}

/// <summary>
/// Indicates who and when an entity was soft deleted, and allows setting those values.
/// </summary>
/// <remarks>
/// The DeletedBy field intentionally uses a framework-neutral string
/// identifier so the domain layer does not depend on application-layer
/// execution-context contracts.
/// </remarks>
public interface ISetDeleted : IDeleted
{
    void SetDeleted(String deletedBy, DateTimeOffset deletedAt);
}

public static class AuditableExtensions
{
    /// <summary>
    /// A convenience method that sets the created audit metadata on an entity that implements ISetCreated.
    /// </summary>
    /// <param name="entity">The entity to set the created audit metadata on.</param>
    /// <param name="createdBy">The identifier of the user who created the entity.</param>
    /// <param name="createdAt">The timestamp when the entity was created.</param>
    public static void AuditCreate(this ISetCreated entity, String createdBy, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);
        ArgumentOutOfRangeException.ThrowIfLessThan(createdAt, DateTimeOffset.UnixEpoch);

        entity.SetCreated(createdBy, createdAt);
    }

    /// <summary>
    /// A convenience method that sets the updated audit metadata on an entity that implements ISetUpdated.
    /// </summary>
    /// <param name="entity">The entity to set the updated audit metadata on.</param>
    /// <param name="updatedBy">The identifier of the user who updated the entity.</param>
    /// <param name="updatedAt">The timestamp when the entity was updated.</param>
    public static void AuditUpdate(this ISetUpdated entity, String updatedBy, DateTimeOffset updatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);
        ArgumentOutOfRangeException.ThrowIfLessThan(updatedAt, DateTimeOffset.UnixEpoch);

        entity.SetUpdated(updatedBy, updatedAt);
    }

    /// <summary>
    /// A convenience method that sets the deleted audit metadata on an entity that implements ISetDeleted.
    /// </summary>
    /// <param name="entity">The entity to set the deleted audit metadata on.</param>
    /// <param name="deletedBy">The identifier of the user who deleted the entity.</param>
    /// <param name="deletedAt">The timestamp when the entity was deleted.</param>
    public static void AuditDelete(this ISetDeleted entity, String deletedBy, DateTimeOffset deletedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedBy);
        ArgumentOutOfRangeException.ThrowIfLessThan(deletedAt, DateTimeOffset.UnixEpoch);

        entity.SetDeleted(deletedBy, deletedAt);
    }
}
