using IoT_System.Domain.Abstractions.Interfaces;

namespace IoT_System.Domain.Abstractions;

/// <summary>
/// Base abstract class for entities with audit tracking
/// </summary>
public abstract class AuditableEntity : Entity, IAuditable
{
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}