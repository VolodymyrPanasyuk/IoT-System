using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Interfaces.Repositories;

public interface IRoleRepository : IRepositoryBase<Role>
{
    Task<OperationResult<Role?>> GetByNameAsync(string roleName);
}