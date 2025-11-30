using IoT_System.Domain.Abstractions.Interfaces;

namespace IoT_System.Domain.Abstractions;

/// <summary>
/// Base abstract class for all entities
/// </summary>
public abstract class Entity : IEntity
{
    public Guid Id { get; set; }
}