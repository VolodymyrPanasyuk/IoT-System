namespace IoT_System.Domain.Abstractions.Interfaces;

/// <summary>
/// Base interface for all entities with Guid identifier
/// </summary>
public interface IEntity
{
    Guid Id { get; set; }
}