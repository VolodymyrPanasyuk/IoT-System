namespace IoT_System.Domain.Abstractions.Interfaces;

/// <summary>
/// Interface for entities that track creation and modification audit information
/// </summary>
public interface IAuditable
{
    Guid CreatedBy { get; set; }
    DateTime CreatedOn { get; set; }
    Guid? LastModifiedBy { get; set; }
    DateTime? LastModifiedOn { get; set; }
}