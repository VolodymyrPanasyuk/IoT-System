using IoT_System.Domain.Abstractions;
using IoT_System.Domain.Entities.Enums;

namespace IoT_System.Domain.Entities;

/// <summary>
/// Represents access permissions to device
/// Can be assigned to users, roles, and/or groups in any combination
/// </summary>
public class DeviceAccessPermission : AuditableEntity
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    // Permission can be granted to users (nullable for flexibility)
    public Guid? UserId { get; set; }

    // Permission can be granted to roles (nullable for flexibility)
    public Guid? RoleId { get; set; }

    // Permission can be granted to groups (nullable for flexibility)
    public Guid? GroupId { get; set; }

    // Permission type
    public DevicePermissionType PermissionType { get; set; } = DevicePermissionType.View;
}